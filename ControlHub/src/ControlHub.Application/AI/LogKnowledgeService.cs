using System.Reflection;
using System.Text;
using ControlHub.Application.Common.Interfaces.AI;
using ControlHub.Application.Common.Logging;
using ControlHub.SharedKernel.Common.Logs; // Để truy cập LogCode

namespace ControlHub.Application.AI
{
    public class LogKnowledgeService
    {
        private readonly IVectorDatabase _vectorDb;
        private readonly IEmbeddingService _embeddingService;
        private readonly IAIAnalysisService _aiService;
        private const string CollectionName = "LogDefinitions";

        public LogKnowledgeService(
            IVectorDatabase vectorDb, 
            IEmbeddingService embeddingService, 
            IAIAnalysisService aiService)
        {
            _vectorDb = vectorDb;
            _embeddingService = embeddingService;
            _aiService = aiService;
        }

        // 1. INGESTION: Học các LogCode từ code
        public async Task IngestLogDefinitionsAsync()
        {
            // Scan toàn bộ assembly chứa CommonLogs để tìm các class XXXLogs
            var assembly = typeof(CommonLogs).Assembly;
            var logClasses = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Logs") && t.IsClass && t.IsSealed && t.IsAbstract == false); 
                // Lưu ý: C# static class là abstract sealed. Nhưng LogCode files của ta là public static class? Yes.
                // Điều kiện: tìm các class có tên *Logs.

            foreach (var type in logClasses)
            {
                // Lấy tất cả field static trả về LogCode
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(LogCode));

                foreach (var field in fields)
                {
                    var logCode = (LogCode?)field.GetValue(null);
                    if (logCode == null) continue;

                    // Text để tạo vector: Cần chứa cả Code lẫn Message để AI hiểu ngữ nghĩa
                    var textToEmbed = $"Code: {logCode.Code}. Meaning: {logCode.Message}";
                    
                    // Tạo Embedding
                    var vector = await _embeddingService.GenerateEmbeddingAsync(textToEmbed);
                    
                    if (vector.Length == 0) continue; // Skip nếu lỗi

                    // Lưu vào Qdrant
                    var payload = new Dictionary<string, object>
                    {
                        { "Code", logCode.Code },
                        { "Message", logCode.Message },
                        { "SourceClass", type.Name }
                    };

                    // ID là Code string (unique)
                    await _vectorDb.UpsertAsync(CollectionName, logCode.Code, vector, payload);
                }
            }
        }

        // 2. RAG ANALYSIS: Phân tích log dựa trên kiến thức
        public async Task<string> AnalyzeSessionAsync(List<LogEntry> logs, string lang = "en")
        {
            if (!logs.Any()) return "No logs to analyze.";

            // Bước 2.1: Tìm ngữ cảnh (Context)
            // Lấy các LogCode unique xuất hiện trong session này
            var uniqueCodes = logs
                .Where(l => l.LogCode != null && !string.IsNullOrEmpty(l.LogCode.Code))
                .Select(l => l.LogCode!.Code)
                .Distinct()
                .ToList();

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("Knowledge Context:");

            // Với mỗi LogCode xuất hiện, ta tìm trong Vector DB xem có thông tin gì thêm không
            // (Thực ra nếu ta đã có Message trong Log rồi thì cũng đã đủ. 
            // Nhưng RAG mạnh ở chỗ nếu ta lưu thêm "Cách Fix" trong Vector DB thì sẽ retrieve được).
            // Ở đây demo: Ta search Vector DB bằng chính Log Message để xem có "LogCode tương tự" nào khác không (ít tác dụng nếu data ít).
            // Tốt hơn: Search Vector Database để tìm "Tài liệu xử lý lỗi" (Documentation).
            
            // DEMO SIMPLE RAG:
            // Lấy 1 log lỗi đầu tiên và tìm kiếm trong DB xem có LogCode nào tương tự (để AI hiểu nhóm lỗi)
            var errorLog = logs.FirstOrDefault(l => l.Level == "Error" || l.Level == "Warning");
            if (errorLog != null)
            {
                var searchVector = await _embeddingService.GenerateEmbeddingAsync(errorLog.Message);
                if (searchVector.Length > 0)
                {
                    var relatedDocs = await _vectorDb.SearchAsync(CollectionName, searchVector, limit: 2);
                    foreach (var doc in relatedDocs)
                    {
                        if (doc.Payload.ContainsKey("Message"))
                        {
                            contextBuilder.AppendLine($"- Related Concept: {doc.Payload["Code"]} ({doc.Payload["Message"]})");
                        }
                    }
                }
            }

            // Bước 2.2: Build Prompt
            var prompt = new StringBuilder();
            prompt.AppendLine("You are an expert system troubleshooter.");
            prompt.AppendLine($"Please respond in the following language: {lang}."); // Thêm chỉ dẫn ngôn ngữ
            prompt.AppendLine(contextBuilder.ToString()); // Inject Context tìm được
            prompt.AppendLine("\nAnalyze the following log sequence and identify the root cause:");
            
            foreach (var log in logs)
            {
                prompt.AppendLine($"[{log.Timestamp:HH:mm:ss}] [{log.Level}] {log.LogCode?.Code ?? "NoCode"}: {log.Message}");
            }

            // Bước 2.3: Gọi AI
            return await _aiService.AnalyzeLogsAsync(prompt.ToString());
        }
    }
}
