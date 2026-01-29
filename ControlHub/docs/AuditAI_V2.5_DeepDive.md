# AuditAI V2.5: Deep Dive - Under the Hood

**Author:** Senior Mentor (10+ YoE)  
**Scope:** Từ lúc nhận HTTP Request đến khi trả Response cho Frontend.

---

## Mục Lục

1. [Tổng Quan Kiến Trúc](#1-tổng-quan-kiến-trúc)
2. [Phase 1: Request Enters ASP.NET Core Pipeline](#2-phase-1-request-enters-aspnet-core-pipeline)
3. [Phase 2: Authentication & Authorization (AuthN/AuthZ)](#3-phase-2-authentication--authorization-authnauthz)
4. [Phase 3: Controller Action Execution](#4-phase-3-controller-action-execution)
5. [Phase 4: Dependency Injection Resolution](#5-phase-4-dependency-injection-resolution)
6. [Phase 5: Raw Log Retrieval (Database/File I/O)](#6-phase-5-raw-log-retrieval-databasefile-io)
7. [Phase 6: Drain3 Log Parsing](#7-phase-6-drain3-log-parsing)
8. [Phase 7: Weighted Reservoir Sampling](#8-phase-7-weighted-reservoir-sampling)
9. [Phase 8: Agentic Runbook Lookup (Qdrant)](#9-phase-8-agentic-runbook-lookup-qdrant)
10. [Phase 9: LLM Inference (Ollama)](#10-phase-9-llm-inference-ollama)
11. [Phase 10: Response Serialization & Return](#11-phase-10-response-serialization--return)
12. [Sequence Diagram (Mermaid)](#12-sequence-diagram-mermaid)

---

## 1. Tổng Quan Kiến Trúc

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  FRONTEND (React)                                                            │
│   └── HTTP GET /api/Audit/analyze/{correlationId}?lang=vi                    │
│       Headers: Authorization: Bearer <JWT>                                   │
└──────────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  ASP.NET Core Pipeline                                                       │
│                                                                              │
│  1. Kestrel (Webserver) nhận TCP connection                                  │
│  2. Middleware Chain:                                                        │
│     ├─ UseRouting()                                                          │
│     ├─ UseAuthentication()  ← JWT validation                                 │
│     ├─ UseAuthorization()   ← Policy check                                   │
│     └─ UseEndpoints() → MapControllers()                                     │
│  3. Route Matching: /api/[controller]/[action]                               │
└──────────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  AuditController.Analyze(correlationId, lang)                                │
│   └── [Authorize(Policy = Policies.CanViewSystemLogs)]                       │
└──────────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  Application Layer (Services)                                                │
│   ├── IAuditAgentService (AgenticAuditService)                               │
│   │      └── InvestigateSessionAsync()                                       │
│   ├── ILogReaderService (LogReaderService)                                   │
│   │      └── GetLogsByCorrelationIdAsync() ← Đọc file log JSON               │
│   ├── ILogParserService (Drain3ParserService)                                │
│   │      └── ParseLogsAsync() ← Cây quyết định, clustering                   │
│   ├── ISamplingStrategy (WeightedReservoirSamplingStrategy)                  │
│   │      └── Sample() ← Thuật toán A-Res                                     │
│   └── IRunbookService (RunbookService)                                       │
│          └── FindRelatedRunbooksAsync() ← Vector search trong Qdrant         │
└──────────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  Infrastructure Layer                                                        │
│   ├── QdrantVectorStore ← HTTP Client gọi Qdrant REST API                    │
│   ├── OllamaEmbeddingService ← HTTP Client gọi Ollama /api/embeddings        │
│   └── LocalAIAdapter ← HTTP Client gọi Ollama /api/generate                  │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Phase 1: Request Enters ASP.NET Core Pipeline

**File:** `Program.cs`

Khi Frontend gọi:
```http
GET /api/Audit/analyze/abc-123?lang=vi
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

1. **Kestrel** (hoặc HTTP.sys/IIS nếu Windows) nhận TCP request.
2. **Routing Middleware** (`app.UseRouting()`) parse URL:
   - Controller: `Audit`
   - Action: `Analyze`
   - Route Parameter: `correlationId = "abc-123"`
   - Query Parameter: `lang = "vi"`

---

## 3. Phase 2: Authentication & Authorization (AuthN/AuthZ)

### 3.1 Authentication (JWT Validation)

**File:** `ControlHubExtensions.cs` (line 225)
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
```

**File:** `ConfigureJwtBearerOptions.cs` (cấu hình từ `appsettings.json`)
```csharp
// Đọc từ appsettings.json
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidIssuer = configuration["Jwt:Issuer"],       // "ControlHub"
    ValidAudience = configuration["Jwt:Audience"],   // "ControlHubUsers"
    IssuerSigningKey = new SymmetricSecurityKey(...) // "super-secret-long-key..."
};
```

**Quá trình:**
1. Middleware `UseAuthentication()` tự động extract token từ Header `Authorization: Bearer <token>`.
2. Gọi `JwtSecurityTokenHandler.ValidateToken()` để:
   - Verify chữ ký (HMAC-SHA256).
   - Kiểm tra `exp` (expiration) chưa hết hạn.
   - Kiểm tra `iss` và `aud` khớp với config.
3. Nếu hợp lệ, gán `HttpContext.User` = `ClaimsPrincipal` chứa các Claims từ token.

**Kết quả:** 
- `HttpContext.User.Identity.IsAuthenticated == true`
- `HttpContext.User.Claims` chứa: `sub` (AccountId), `email`, `roles`, etc.

---

### 3.2 Authorization (Policy Check)

**File:** `AuditController.cs` (line 79)
```csharp
[Authorize(Policy = Policies.CanViewSystemLogs)]
[HttpGet("analyze/{correlationId}")]
public async Task<IActionResult> Analyze(...)
```

**File:** `Policies.cs` (Domain layer)
```csharp
public const string CanViewSystemLogs = "CanViewSystemLogs";
```

**Quá trình:**
1. `PermissionPolicyProvider.GetPolicyAsync("CanViewSystemLogs")` trả về dynamic policy.
2. `PermissionAuthorizationHandler.HandleRequirementAsync()` được gọi:
   ```csharp
   // Kiểm tra User có claim "Permission" với value "CanViewSystemLogs"
   var permissions = context.User.Claims.Where(c => c.Type == "Permission");
   if (permissions.Any(p => p.Value == requirement.Permission))
       context.Succeed(requirement);
   ```
3. **Permission Claims từ đâu?** → `PermissionClaimsTransformation.TransformAsync()`:
   - Đọc AccountId từ token.
   - Query database lấy Roles của Account.
   - Query database lấy Permissions của Roles.
   - Gắn vào `ClaimsIdentity`.

**Database Queries (Phase này):**
```sql
-- 1. Lấy Roles của Account
SELECT r.* FROM Roles r
JOIN AccountRoles ar ON ar.RoleId = r.Id
WHERE ar.AccountId = @AccountId;

-- 2. Lấy Permissions của Roles
SELECT p.* FROM Permissions p
JOIN RolePermissions rp ON rp.PermissionId = p.Id
WHERE rp.RoleId IN (@RoleIds);
```

**Optimization:** Sử dụng `CachedRoleRepository` và `CachedPermissionRepository` (decorator pattern) để cache trong `IMemoryCache`.

---

## 4. Phase 3: Controller Action Execution

**File:** `AuditController.cs` (line 81)
```csharp
public async Task<IActionResult> Analyze(string correlationId, [FromQuery] string lang = "en")
```

### 4.1 Language Detection
```csharp
// Line 84-89
if (lang == "en" && Request.Headers.ContainsKey("Accept-Language"))
{
    var header = Request.Headers["Accept-Language"].ToString(); // "vi-VN,vi;q=0.9"
    var firstLang = header.Split(',')[0].Split(';')[0];          // "vi-VN"
    lang = firstLang.Contains('-') ? firstLang.Split('-')[0] : firstLang; // "vi"
}
```

### 4.2 Version Check & Routing
```csharp
// Line 92-103
if (_agentService != null)  // <-- Điều kiện V2.5
{
    var auditResult = await _agentService.InvestigateSessionAsync(correlationId, lang);
    return Ok(new { ... Version = "V2.5" });
}
// Fallback to V1...
```

**Tại sao `_agentService` có thể null?**
→ Được resolve từ DI container với điều kiện:
```csharp
// ControlHubExtensions.cs line 197-201
var aiVersion = configuration["AuditAI:Version"] ?? "V1";
if (aiVersion == "V2.5")
{
    services.AddScoped<IAuditAgentService, AgenticAuditService>();
}
```

---

## 5. Phase 4: Dependency Injection Resolution

**Tại thời điểm Controller được khởi tạo**, DI Container resolve:

```csharp
public AuditController(
    ILogKnowledgeService knowledgeService,  // Always registered (V1 compat)
    ILogReaderService logReader,            // Scoped
    IServiceProvider sp,                    // Root container access
    IConfiguration config)                  // Singleton (appsettings.json)
{
    // Lazy resolve optional V2.5 services
    _agentService = sp.GetService<IAuditAgentService>(); // null nếu V1
    _logParser = sp.GetService<ILogParserService>();     // Drain3ParserService
    _runbookService = sp.GetService<IRunbookService>();  // RunbookService
}
```

**AgenticAuditService Constructor:**
```csharp
public AgenticAuditService(
    ILogParserService parserService,      // Drain3ParserService
    ISamplingStrategy samplingStrategy,   // WeightedReservoirSamplingStrategy
    IRunbookService runbookService,       // RunbookService
    IAIAnalysisService aiService,         // LocalAIAdapter (Ollama)
    ILogReaderService logReader,          // LogReaderService
    IConfiguration config)
```

**Dependency Graph:**
```
AgenticAuditService
├── ILogParserService → Drain3ParserService
├── ISamplingStrategy → WeightedReservoirSamplingStrategy
├── IRunbookService → RunbookService
│   ├── IVectorDatabase → QdrantVectorStore (HttpClient)
│   └── IEmbeddingService → OllamaEmbeddingService (HttpClient)
├── IAIAnalysisService → LocalAIAdapter (HttpClient)
└── ILogReaderService → LogReaderService (File I/O)
```

---

## 6. Phase 5: Raw Log Retrieval (Database/File I/O)

**File:** `AgenticAuditService.cs` (line 43)
```csharp
var rawLogs = await _logReader.GetLogsByCorrelationIdAsync(correlationId);
```

**File:** `LogReaderService.cs`

### 6.1 Xác định Log Directory
```csharp
// Line 20-31
public LogReaderService(ILogger<LogReaderService> logger, IConfiguration configuration)
{
    var configuredPath = configuration["Logging:LogDirectory"];
    if (!string.IsNullOrEmpty(configuredPath))
    {
        _logDirectory = configuredPath;
    }
    else
    {
        _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
    }
}
```

### 6.2 Đọc tất cả Log Files
```csharp
// Line 79
var files = Directory.GetFiles(_logDirectory, "log-*.json");

foreach (var file in files)
{
    using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
    
    while ((line = await streamReader.ReadLineAsync()) != null)
    {
        var entry = JsonSerializer.Deserialize<LogEntry>(line);
        if (entry != null) result.Add(entry);
    }
}
```

**Tại sao `FileShare.ReadWrite`?**
→ Cho phép đọc trong khi Serilog đang ghi log vào file (concurrent access).

### 6.3 Filter theo CorrelationId
```csharp
// Line 41-46
var matches = logs.Where(l =>
    (l.Properties.ContainsKey("CorrelationId") && l.Properties["CorrelationId"].ToString() == correlationId) ||
    (l.TraceId == correlationId) ||
    (l.RequestId == correlationId) ||
    (l.SerilogTraceId == correlationId)
).OrderBy(x => x.Timestamp).ToList();
```

**Note:** Không có Database query ở đây. Logs được lưu dưới dạng file JSON (Serilog + RenderedCompactJsonFormatter).

---

## 7. Phase 6: Drain3 Log Parsing

**File:** `AgenticAuditService.cs` (line 50)
```csharp
var parseResult = await _parserService.ParseLogsAsync(rawLogs);
```

**File:** `Drain3ParserService.cs`

### 7.1 Tổng Quan Thuật Toán Drain3

Drain3 là thuật toán log parsing dựa trên **Fixed-Depth Tree**. Mục tiêu: cluster hàng triệu dòng log thành vài trăm templates.

**Ví dụ:**
```
Input:  "Connection from 192.168.1.5 failed"
        "Connection from 10.0.0.1 failed"
Output: Template = "Connection from <IP> failed" (count=2)
```

### 7.2 Step 1: Variable Masking
```csharp
// Line 110-116
private string Mask(string logLine)
{
    logLine = IpRegex.Replace(logLine, "<IP>");       // 192.168.1.1 → <IP>
    logLine = GuidRegex.Replace(logLine, "<GUID>");   // a1b2c3d4-... → <GUID>
    logLine = NumberRegex.Replace(logLine, "<NUM>");  // 12345 → <NUM>
    return logLine;
}
```

**Regex Patterns (Precompiled):**
```csharp
private static readonly Regex IpRegex = new Regex(
    @"(?<!\d)(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}...(?!\d)", 
    RegexOptions.Compiled);
```

### 7.3 Step 2: Tokenization
```csharp
// Line 82
var tokens = content.Split(new[] { ' ', '\t', ',', ':', ';', '=', '[', ']' }, 
                           StringSplitOptions.RemoveEmptyEntries);
// "Connection from <IP> failed" → ["Connection", "from", "<IP>", "failed"]
```

### 7.4 Step 3: Tree Search & Cluster Matching
```csharp
// Line 118-171
private Cluster? TreeSearch(Node root, string[] tokens)
{
    var length = tokens.Length;
    // Level 1: Group by length (log dài 4 token vs 10 token → khác node)
    if (!root.Children.TryGetValue(length.ToString(), out var lengthNode))
        return null;

    // Level 2-N: Follow tokens or wildcard
    Node current = lengthNode;
    foreach (var token in tokens)
    {
        if (current.Children.TryGetValue(token, out var nextNode))
            current = nextNode;
        else if (current.Children.TryGetValue("*", out var wildcardNode))
            current = wildcardNode;
        else
            break;
    }

    // Find best match among clusters in leaf node
    foreach (var cluster in current.Clusters)
    {
        double sim = GetSeqSimilarity(cluster.LogTemplateTokens, tokens);
        if (sim >= _similarityThreshold) // default 0.5
            return cluster;
    }
    return null;
}
```

### 7.5 Step 4: Template Update (Generalization)
```csharp
// Line 217-226
private void UpdateTemplate(Cluster cluster, string[] tokens)
{
    for (int i = 0; i < cluster.LogTemplateTokens.Count; i++)
    {
        if (cluster.LogTemplateTokens[i] != tokens[i])
        {
            cluster.LogTemplateTokens[i] = "<*>";  // Wildcard
        }
    }
}
```

**Ví dụ Evolution:**
```
Log 1: ["User", "admin", "logged", "in"]  → Template: "User admin logged in"
Log 2: ["User", "john",  "logged", "in"]  → Template: "User <*> logged in"
```

### 7.6 Output: LogParseResult
```csharp
return Task.FromResult(new LogParseResult(templates, templateToLogs));
// templates: List<LogTemplate> = [{TemplateId, Pattern, Count, FirstSeen, LastSeen, Severity}]
// templateToLogs: Dictionary<string, List<LogEntry>> = mapping từ template ID → raw logs
```

---

## 8. Phase 7: Weighted Reservoir Sampling

**File:** `AgenticAuditService.cs` (line 52)
```csharp
var sampledTemplates = _samplingStrategy.Sample(parsedTemplates, 50);
```

**File:** `WeightedReservoirSamplingStrategy.cs`

### 8.1 Mục Đích
Nếu Drain3 trả về 500 templates, LLM không thể xử lý hết (token limit). Cần chọn 50 templates "quan trọng nhất".

### 8.2 Thuật Toán A-Res (Efraimidis & Spirakis)

```csharp
// Line 17-38
public List<LogTemplate> Sample(List<LogTemplate> templates, int maxCount = 50)
{
    if (templates.Count <= maxCount) return templates.OrderBy(t => t.FirstSeen).ToList();

    // 1. Calculate weight: w = SeverityWeight * (1 / log(Count + 1))
    var weightedItems = templates.Select(t => new
    {
        Template = t,
        Weight = CalculateWeight(t)
    }).ToList();

    // 2. Algorithm A-Res: key = u^(1/w) where u = Random(0,1)
    var random = new Random();
    var sampled = weightedItems
        .Select(x => new
        {
            x.Template,
            Key = Math.Pow(random.NextDouble(), 1.0 / x.Weight)
        })
        .OrderByDescending(x => x.Key)  // Higher key = higher probability of selection
        .Take(maxCount)
        .Select(x => x.Template)
        .OrderBy(t => t.FirstSeen)      // Re-order chronologically
        .ToList();

    return sampled;
}
```

### 8.3 Weight Calculation
```csharp
// Line 41-62
private double CalculateWeight(LogTemplate template)
{
    double severityFactor = template.Severity?.ToLower() switch
    {
        "fatal" => 100.0,
        "error" => 50.0,
        "warning" => 10.0,
        _ => 1.0
    };

    // Rare items (low count) → High factor
    // Count=1: 1/log(2) ≈ 3.32
    // Count=1000: 1/log(1001) ≈ 0.33
    return severityFactor * (1.0 / Math.Log(template.Count + 1));
}
```

**Kết quả:** Errors hiếm gặp (low count) có weight cao → được ưu tiên giữ lại.

---

## 9. Phase 8: Agentic Runbook Lookup (Qdrant)

**File:** `AgenticAuditService.cs` (line 67-83)

```csharp
var errorTemplates = sampledTemplates.Where(t => t.Severity == "Error" || t.Severity == "Fatal").ToList();

foreach (var tmpl in errorTemplates.Take(3)) // Limit to top 3
{
    var runbooks = await _runbookService.FindRelatedRunbooksAsync(tmpl.Pattern);
    // Append to runbookContext...
}
```

**File:** `RunbookService.cs`

### 9.1 Semantic Search Flow
```csharp
// Line 52-64
public async Task<List<RunbookEntry>> FindRelatedRunbooksAsync(string logCodeOrPattern, int limit = 3)
{
    // 1. Generate embedding for search query
    var vector = await _embeddingService.GenerateEmbeddingAsync(logCodeOrPattern);
    
    // 2. Search Qdrant
    var searchResults = await _vectorDb.SearchAsync(_collectionName, vector, limit);
    
    // 3. Map results to RunbookEntry
    return searchResults.Select(r => new RunbookEntry(...)).ToList();
}
```

### 9.2 Ollama Embedding Generation
**File:** `OllamaEmbeddingService.cs`

```csharp
// Line 19-43
public async Task<float[]> GenerateEmbeddingAsync(string text)
{
    var request = new { model = "all-minilm", prompt = text };
    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("http://localhost:11434/api/embeddings", content);
    var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(await response.Content.ReadAsStringAsync());
    
    return result?.Embedding ?? Array.Empty<float>(); // 384-dimensional vector
}
```

### 9.3 Qdrant Vector Search
**File:** `QdrantVectorStore.cs`

```csharp
// Line 51-87
public async Task<List<SearchResult>> SearchAsync(string collectionName, float[] vector, int limit = 3)
{
    var searchRequest = new { vector = vector, limit = limit, with_payload = true };
    
    var response = await _httpClient.PostAsync(
        $"/collections/{collectionName}/points/search", 
        new StringContent(JsonSerializer.Serialize(searchRequest), ...)
    );
    
    // Parse Qdrant response and return top K matches
}
```

**HTTP Request to Qdrant:**
```http
POST http://localhost:6333/collections/Runbooks/points/search
{
    "vector": [0.123, -0.456, ...], // 384 floats
    "limit": 3,
    "with_payload": true
}
```

**Qdrant Response:**
```json
{
    "result": [
        {
            "id": "abc123",
            "score": 0.89,
            "payload": {
                "LogCode": "ERR_DB_001",
                "Problem": "Database Connection Timeout",
                "Solution": "Check connection pool..."
            }
        }
    ]
}
```

---

## 10. Phase 9: LLM Inference (Ollama)

**File:** `AgenticAuditService.cs` (line 86-89)
```csharp
var prompt = BuildPrompt(sampledTemplates, runbookContext.ToString(), lang);
var aiResponse = await _aiService.AnalyzeLogsAsync(prompt);
```

### 10.1 Prompt Construction
```csharp
// Line 113-148
private string BuildPrompt(List<LogTemplate> templates, string runbookContext, string lang)
{
    var sb = new StringBuilder();
    
    sb.AppendLine("You are an expert Reliability Engineer (SRE).");
    sb.AppendLine($"Task: Analyze the log summary below and discover the root cause. Response in {languageName}.");
    sb.AppendLine("Use the provided Runbook knowledge if applicable.");
    
    if (!string.IsNullOrEmpty(runbookContext))
    {
        sb.AppendLine("\n=== KNOWLEDGE BASE / RUNBOOKS ===");
        sb.AppendLine(runbookContext);
    }
    
    sb.AppendLine("\n=== LOG SESSION SUMMARY ===");
    sb.AppendLine("Format: [Severity] [Count] [TimeRange] Template");
    
    foreach (var t in templates)
    {
        sb.AppendLine($"[{t.Severity}] [x{t.Count}] [{t.FirstSeen:HH:mm:ss}-{t.LastSeen:HH:mm:ss}] {t.Pattern}");
    }

    sb.AppendLine("\n=== INSTRUCTIONS ===");
    sb.AppendLine("1. Identify the primary error.");
    sb.AppendLine("2. Specific Log Patterns/Sequences that lead to the error.");
    sb.AppendLine("3. Suggest a concrete fix based on Runbooks or General Knowledge.");
    
    return sb.ToString();
}
```

### 10.2 Ollama API Call
**File:** `LocalAIAdapter.cs` (giả định)

```csharp
public async Task<string> AnalyzeLogsAsync(string prompt)
{
    var request = new { model = "llama3", prompt = prompt, stream = false };
    var response = await _httpClient.PostAsync(
        "http://localhost:11434/api/generate",
        new StringContent(JsonSerializer.Serialize(request), ...)
    );
    
    var result = JsonSerializer.Deserialize<OllamaResponse>(...);
    return result.Response;
}
```

**HTTP Request to Ollama:**
```http
POST http://localhost:11434/api/generate
{
    "model": "llama3",
    "prompt": "You are an expert Reliability Engineer...",
    "stream": false
}
```

---

## 11. Phase 10: Response Serialization & Return

**File:** `AuditController.cs` (line 95-102)
```csharp
return Ok(new
{
    CorrelationId = correlationId,
    Analysis = auditResult.Analysis,       // LLM response (Markdown)
    ToolsUsed = auditResult.ToolsUsed,     // ["Drain3Parser", "WeightedReservoirSampling", "RunbookLookup"]
    Templates = auditResult.ProcessedTemplates, // List<LogTemplate>
    Version = "V2.5"
});
```

ASP.NET Core's `SystemTextJsonOutputFormatter` serialize object thành JSON:
```json
{
    "correlationId": "abc-123",
    "analysis": "## Root Cause Analysis\n...",
    "toolsUsed": ["Drain3Parser", "WeightedReservoirSampling", "RunbookLookup"],
    "templates": [
        {
            "templateId": "a1b2c3d4",
            "pattern": "Connection from <IP> failed",
            "count": 42,
            "severity": "Error",
            "firstSeen": "2026-01-28T10:00:00",
            "lastSeen": "2026-01-28T10:05:00"
        }
    ],
    "version": "V2.5"
}
```

---

## 12. Sequence Diagram (Mermaid)

```mermaid
sequenceDiagram
    participant FE as Frontend (React)
    participant MW as ASP.NET Middleware
    participant AC as AuditController
    participant AAS as AgenticAuditService
    participant LRS as LogReaderService
    participant D3 as Drain3ParserService
    participant WRS as WeightedReservoirSampling
    participant RBS as RunbookService
    participant QD as Qdrant
    participant OL as Ollama

    FE->>MW: GET /api/Audit/analyze/abc-123
    Note over MW: 1. JWT Validation
    MW->>MW: Validate Token (Issuer, Exp, Signature)
    Note over MW: 2. Policy Check
    MW->>MW: Check Permission "CanViewSystemLogs"
    MW->>AC: Route to Analyze()
    
    AC->>AAS: InvestigateSessionAsync(correlationId, lang)
    
    AAS->>LRS: GetLogsByCorrelationIdAsync(correlationId)
    LRS->>LRS: Read Logs/*.json files
    LRS-->>AAS: List<LogEntry> (500 entries)
    
    AAS->>D3: ParseLogsAsync(rawLogs)
    D3->>D3: Mask → Tokenize → TreeSearch → Cluster
    D3-->>AAS: LogParseResult (80 templates)
    
    AAS->>WRS: Sample(templates, 50)
    WRS->>WRS: Weight by Severity × Rarity
    WRS-->>AAS: List<LogTemplate> (50 samples)
    
    loop For each Error Template (max 3)
        AAS->>RBS: FindRelatedRunbooksAsync(pattern)
        RBS->>OL: GenerateEmbeddingAsync(pattern)
        OL-->>RBS: float[384] vector
        RBS->>QD: POST /collections/Runbooks/points/search
        QD-->>RBS: SearchResult[] (top 3)
        RBS-->>AAS: List<RunbookEntry>
    end
    
    AAS->>AAS: BuildPrompt(templates, runbooks, lang)
    AAS->>OL: POST /api/generate (prompt)
    OL-->>AAS: LLM Response (Markdown)
    
    AAS-->>AC: AuditResult{Analysis, Templates, ToolsUsed}
    AC-->>MW: Ok(JSON)
    MW-->>FE: HTTP 200 + JSON Body
```

---

## Kết Luận

Flow hoàn chỉnh từ Request → Response bao gồm **10 phases chính**. Điểm đặc biệt của V2.5 so với V1:

| Aspect | V1 | V2.5 |
|--------|----|----|
| Log Preprocessing | None | Drain3 Parsing + Masking |
| Sampling | None (send all) | Weighted Reservoir A-Res |
| Knowledge Base | Static definitions | Vector Search (Qdrant) |
| LLM Context | Raw logs | Templates + Runbooks |
| Explainability | Low | High (ToolsUsed trace) |

**Database/External Calls Summary:**
1. SQL Server: Permission/Role lookup (cached).
2. File System: Log JSON files.
3. Qdrant (Vector DB): Runbook semantic search.
4. Ollama (LLM): Embedding + Text generation.

**End of Document.**
