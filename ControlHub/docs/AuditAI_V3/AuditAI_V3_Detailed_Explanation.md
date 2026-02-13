# AuditAI V3: Giải Thích Chi Tiết Cách Hoạt Động

## 📚 Tổng Quan: Cả Hệ Thống Làm Gì?

**Mục tiêu chính**: Khi có lỗi xảy ra trong hệ thống, AI sẽ:
1. **Hiểu** log lỗi đó nói về vấn đề gì (Parsing - Week 1-2)
2. **Tìm kiếm** các log tương tự đã xảy ra trước đó (RAG - Week 2-3)
3. **Đề xuất** giải pháp dựa trên kinh nghiệm (Reasoning - Week 3-4)

---

## 🔍 PHẦN 1: Week 1-2 (Tasks 9.1.1 - 9.1.9) - PARSING

### Nhiệm vụ: "Đọc hiểu" log message

**Ví dụ thực tế**: Giống như khi em đọc tin nhắn lỗi, em phải hiểu:
- Đây là lỗi gì? (Authentication? Database? Network?)
- Nghiêm trọng đến mức nào?
- Liên quan đến module nào?

### 🏗️ Kiến Trúc Parsing (2 tầng)

```
Raw Log Message
      ↓
┌─────────────────────────┐
│  1. Drain3 Parser       │ ← Nhanh, pattern matching
│  (V2.5 - Cũ)           │
└─────────────────────────┘
      ↓
  Confidence Check
      ↓
┌─────────────────────────┐
│  2. ONNX Classifier     │ ← Chậm hơn, nhưng chính xác
│  (V3 - Mới)            │
└─────────────────────────┘
      ↓
Structured Result
```

### 📝 Ví Dụ Cụ Thể

**Input (Raw Log)**:
```
"User login failed: Invalid password for account admin@example.com"
```

#### Bước 1: Drain3 Parser thử phân tích

**Drain3** hoạt động như thế nào?
- Nó nhớ các **template** (khuôn mẫu) đã thấy trước đó
- Ví dụ template: `"User login failed: Invalid password for account <*>"`
- Nó thay `admin@example.com` bằng `<*>` (wildcard)

**Kết quả Drain3**:
```csharp
new Drain3Result {
    Template = "User login failed: Invalid password for account <*>",
    Parameters = ["admin@example.com"],
    ClusterId = "cluster_42"
}
```

**Confidence Heuristic** (Độ tin cậy):
```csharp
// Code trong HybridLogParser.cs
float CalculateDrain3Confidence(Drain3Result result) {
    float confidence = 0.5f; // Base
    
    // Nếu template đã thấy nhiều lần → tin cậy hơn
    if (result.ClusterSize > 100) confidence += 0.3f;
    
    // Nếu ít parameters → template cụ thể hơn → tin cậy hơn
    if (result.Parameters.Count <= 2) confidence += 0.2f;
    
    return confidence; // VD: 0.5 + 0.3 + 0.2 = 1.0
}
```

**Nếu confidence >= 0.7** → Xong, dùng kết quả Drain3!

**Nếu confidence < 0.7** → Chuyển sang Bước 2

#### Bước 2: ONNX Semantic Classifier (Fallback)

**ONNX Classifier** làm gì?
- Dùng **BERT model** (AI thật sự) để hiểu **ngữ nghĩa**
- Không cần template, hiểu được cả log chưa từng thấy

**Process**:
```
1. Tokenize: "User login failed..." → [101, 2027, 7130, 3478, ...]
2. ONNX Inference: [101, 2027, ...] → [0.05, 0.92, 0.01, 0.02]
                                        ↑
                                    Category probabilities
3. Argmax: 0.92 → Category "Authentication.Login.Failed"
```

**Kết quả ONNX**:
```csharp
new LogClassification {
    Category = "Authentication",
    SubCategory = "Login.Failed",
    Confidence = 0.92f,
    ExtractedFields = new Dictionary<string, string> {
        ["email"] = "admin@example.com",
        ["reason"] = "Invalid password"
    }
}
```

### 🎯 Output Cuối Cùng (Structured)

```csharp
new HybridParseResult {
    Category = "Authentication",
    SubCategory = "Login.Failed",
    Confidence = 0.92f,
    ParsedFields = {
        ["email"] = "admin@example.com",
        ["reason"] = "Invalid password"
    },
    Metadata = new ParsingMetadata {
        UsedDrain3 = false,
        UsedSemanticClassifier = true,
        ProcessingTimeMs = 45
    }
}
```

---

## 🔎 PHẦN 2: Week 2-3 (Tasks 9.1.10 - 9.1.18) - RAG

### Nhiệm vụ: "Tìm kiếm thông minh" các log liên quan

**Ví dụ thực tế**: Giống như khi em search Google, nhưng:
- Không chỉ tìm từ khóa giống nhau
- Mà tìm **ý nghĩa** giống nhau
- Và **xếp hạng** kết quả theo độ liên quan

### 🏗️ Kiến Trúc RAG (3 tầng)

```
User Query: "Why did login fail?"
      ↓
┌─────────────────────────────────┐
│ 1. Agentic RAG (Orchestrator)  │ ← Quyết định strategy
│    - Analyze complexity         │
│    - Choose: Single vs Multi    │
└─────────────────────────────────┘
      ↓
   ┌──────┴──────┐
   │             │
Simple Query   Complex Query
   │             │
   ↓             ↓
┌──────────┐  ┌──────────────────┐
│ Single   │  │ Multi-Hop        │
│ Hop      │  │ Retriever        │
└──────────┘  └──────────────────┘
   │             │
   └──────┬──────┘
          ↓
┌─────────────────────────────────┐
│ 2. Reranker (Cross-Encoder)    │ ← Xếp hạng lại
│    - Score query-doc pairs     │
│    - Return top-K              │
└─────────────────────────────────┘
          ↓
   Top Documents
```

### 📝 Ví Dụ Cụ Thể

**Scenario**: Admin muốn tìm hiểu tại sao login lỗi

**Input Query**:
```
"Why did user login fail after password reset?"
```

#### Bước 1: Agentic RAG - Phân Tích Độ Phức Tạp

```csharp
// Code trong AgenticRAGService.cs
float AnalyzeQueryComplexity(string query) {
    float complexity = 0f;
    
    // Heuristic 1: Số từ
    var wordCount = query.Split(' ').Length; // 8 từ
    if (wordCount > 5) complexity += 0.15f; // → 0.15
    
    // Heuristic 2: Từ khóa phức tạp
    if (query.Contains("why")) complexity += 0.4f; // → 0.55
    if (query.Contains("after")) complexity += 0.3f; // → 0.85
    
    return complexity; // 0.85 > 0.5 threshold → COMPLEX!
}
```

**Kết quả**: Query này **phức tạp** → Dùng **Multi-Hop**

#### Bước 2A: Multi-Hop Retrieval (Cho Complex Query)

**Multi-Hop** là gì?
- Tìm kiếm **nhiều lần** (nhiều "hop")
- Mỗi lần tìm, **mở rộng query** dựa trên kết quả trước

**Hop 1**: Tìm với query gốc
```
Query: "Why did user login fail after password reset?"
      ↓
Vector DB Search (Top 20)
      ↓
Results:
1. "Password reset successful for user123" (score: 0.75)
2. "Login failed: session expired" (score: 0.72)
3. "User authentication error" (score: 0.68)
...
```

**Rerank** (Dùng Cross-Encoder):
```csharp
// OnnxReranker.cs
foreach (var doc in candidates) {
    // Tính score cho cặp (query, document)
    var score = CrossEncoderScore(query, doc.Content);
    // VD: ("Why did...", "Login failed: session expired") → 0.92
}

// Top-5 sau rerank:
1. "Login failed: session expired" (0.92) ← Tăng từ 0.72!
2. "Password reset successful" (0.88)
3. "Session timeout after password change" (0.85)
...
```

**Check Confidence**: Best score = 0.92 >= 0.7 → **Đủ tốt, dừng!**

**Nếu không đủ** → Hop 2:

**Query Expansion** (Mở rộng query):
```csharp
// MultiHopRetriever.cs
string ExpandQuery(string original, List<RankedDocument> topDocs) {
    // Lấy keywords từ top document
    var keywords = ExtractKeywords(topDocs[0].Content);
    // VD: ["session", "expired", "timeout"]
    
    return $"{original} {string.Join(" ", keywords)}";
    // → "Why did user login fail after password reset session expired timeout"
}
```

Rồi tìm lại với query mới này...

#### Bước 2B: Single-Hop (Cho Simple Query)

**Ví dụ Simple Query**: `"authentication error"`

```
Query: "authentication error"
      ↓
Vector DB Search (Top 20)
      ↓
Rerank (Top 10)
      ↓
Done!
```

Chỉ 1 lần tìm, không cần expand query.

### 🎯 Output Cuối Cùng

```csharp
new AgenticRAGResult {
    Documents = [
        new RankedDocument {
            Content = "Login failed: session expired after password reset",
            RelevanceScore = 0.92f,
            Metadata = { ["timestamp"] = "2026-02-01T10:30:00Z" }
        },
        new RankedDocument {
            Content = "Password reset invalidates active sessions",
            RelevanceScore = 0.88f,
            Metadata = { ["category"] = "Authentication" }
        },
        // ... top 10 documents
    ],
    StrategyUsed = RAGStrategy.MultiHop,
    Metadata = {
        ["hops"] = 1,
        ["total_candidates"] = 20,
        ["complexity"] = 0.85
    }
}
```

---

## 🔗 PHẦN 3: Cách 2 Phần Kết Hợp Với Nhau

### Luồng Xử Lý Hoàn Chỉnh

```
┌─────────────────────────────────────────────────────────────┐
│                    USER QUERY                               │
│  "Tại sao login lỗi sau khi reset password?"               │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  STEP 1: PARSING (Week 1-2)                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ HybridLogParser                                      │  │
│  │ - Drain3: Không match (query không phải log)        │  │
│  │ - ONNX: Classify → "Authentication.Login.Failed"    │  │
│  └──────────────────────────────────────────────────────┘  │
│  Output: Category = "Authentication.Login.Failed"          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  STEP 2: EMBEDDING (Chuyển text → vector)                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ EmbeddingService                                     │  │
│  │ Query → [0.12, -0.45, 0.78, ..., 0.33] (768 dims)  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  STEP 3: RAG - RETRIEVAL (Week 2-3)                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ AgenticRAG                                           │  │
│  │ 1. Analyze: complexity = 0.85 → Multi-Hop           │  │
│  │ 2. MultiHopRetriever:                               │  │
│  │    - Hop 1: Search vector DB                        │  │
│  │    - Rerank: OnnxReranker (cross-encoder)          │  │
│  │    - Check confidence → OK!                         │  │
│  └──────────────────────────────────────────────────────┘  │
│  Output: Top 10 relevant documents                         │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  STEP 4: REASONING (Week 3-4 - Chưa làm)                   │
│  - Analyze retrieved docs                                   │
│  - Generate solution                                        │
│  - Confidence scoring                                       │
└─────────────────────────────────────────────────────────────┘
```

### 🔄 Tương Tác Giữa 2 Phần

#### Scenario 1: Log Mới Vào Hệ Thống

```
1. Log Message: "User admin@test.com login failed: invalid password"
   ↓
2. PARSING (Week 1-2):
   - HybridLogParser phân tích
   - Output: Category = "Authentication.Login.Failed"
   ↓
3. LƯU VÀO VECTOR DB:
   - Embedding: Text → Vector [0.1, 0.2, ...]
   - Store: {
       id: "log_12345",
       vector: [0.1, 0.2, ...],
       payload: {
         Content: "User admin@test.com login failed...",
         Category: "Authentication.Login.Failed", ← Từ Parsing!
         Timestamp: "2026-02-03T01:00:00Z"
       }
     }
```

**→ Parsing giúp ENRICH metadata cho vector DB!**

#### Scenario 2: Admin Query

```
1. Query: "Tìm các lỗi login liên quan đến password reset"
   ↓
2. PARSING (Optional):
   - Có thể dùng ONNX để classify query
   - Biết query về "Authentication" → Filter vector DB
   ↓
3. RAG (Week 2-3):
   - AgenticRAG: Analyze complexity → Multi-Hop
   - Search vector DB với filter: Category = "Authentication"
   - Rerank kết quả
   ↓
4. Output: Top documents về password reset + login failed
```

**→ Parsing giúp RAG tìm kiếm CHÍNH XÁC hơn!**

---

## 💡 Ví Dụ Tổng Hợp: End-to-End

### Input: Admin nhận được alert

```
Alert: "Multiple login failures detected"
```

### Bước 1: Admin query hệ thống

```
Query: "Why are there so many login failures in the last hour?"
```

### Bước 2: Parsing (Classify query)

```csharp
// OnnxLogClassifier
var classification = await _classifier.ClassifyAsync(query);
// Result: Category = "Authentication", SubCategory = "Login.Failed"
```

### Bước 3: RAG - Complexity Analysis

```csharp
// AgenticRAGService
var complexity = AnalyzeQueryComplexity(query);
// "why" + "so many" + "in the last hour" → 0.85 → COMPLEX
```

### Bước 4: Multi-Hop Retrieval

**Hop 1**:
```
Search vector DB:
- Filter: Category = "Authentication" (từ Parsing!)
- Time range: Last 1 hour
- Top 20 candidates

Rerank:
- Top 5 most relevant logs
```

**Check**: Best score = 0.75 < 0.8 → Chưa đủ tốt

**Hop 2** (Expand query):
```
Original: "Why are there so many login failures in the last hour?"
Expanded: "... login failures password invalid session timeout"

Search again:
- More specific results
- Top 5 với score > 0.9
```

### Bước 5: Output

```json
{
  "documents": [
    {
      "content": "Login failed: password expired for 50 users",
      "score": 0.95,
      "category": "Authentication.Login.Failed",
      "timestamp": "2026-02-03T00:45:00Z"
    },
    {
      "content": "Bulk password expiration triggered by policy",
      "score": 0.92,
      "category": "Security.Policy.Enforcement",
      "timestamp": "2026-02-03T00:30:00Z"
    }
  ],
  "strategy": "MultiHop",
  "metadata": {
    "hops": 2,
    "root_cause": "Password policy caused mass expiration"
  }
}
```

---

## 🎓 Tóm Tắt: Vai Trò Của Từng Phần

### Week 1-2 (Parsing): "Người Đọc Hiểu"
- **Input**: Raw text (log hoặc query)
- **Output**: Structured data (Category, SubCategory, Fields)
- **Vai trò**: Biến text thành data có cấu trúc để máy hiểu được
- **Ví dụ**: "User login failed" → `{Category: "Auth", SubCategory: "Login.Failed"}`

### Week 2-3 (RAG): "Người Tìm Kiếm"
- **Input**: Query + Structured metadata (từ Parsing)
- **Output**: Ranked documents
- **Vai trò**: Tìm thông tin liên quan thông minh
- **Ví dụ**: Query "why login fail?" → Top 10 logs tương tự

### Kết Hợp:
```
Parsing cung cấp METADATA → RAG dùng để FILTER & RANK tốt hơn
```

---

## 🔧 Code Flow Thực Tế

```csharp
// Trong AgenticAuditService (sẽ làm ở Week 3-4)
public async Task<AuditSolution> AnalyzeAsync(string query) {
    // 1. PARSING: Hiểu query
    var classification = await _hybridParser.ParseAsync(query);
    
    // 2. RAG: Tìm logs liên quan
    var ragResult = await _agenticRAG.RetrieveAsync(
        query,
        new AgenticRAGOptions {
            // Dùng category từ parsing để filter
            FilterCategory = classification.Category
        }
    );
    
    // 3. REASONING: Phân tích & đề xuất (Week 3-4)
    var solution = await _reasoningModel.GenerateSolutionAsync(
        query,
        ragResult.Documents,
        classification
    );
    
    return solution;
}
```

---

**Hy vọng giờ em đã hiểu rõ cách cả hệ thống hoạt động! 🎉**

Nếu còn phần nào chưa rõ, em cứ hỏi anh nhé!
