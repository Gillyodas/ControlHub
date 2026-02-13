using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ControlHub.Application.Common.Interfaces.AI.V3.Parsing
{
    /// <summary>
    /// Semantic log classifier sử dụng ML model để phân loại log dựa trên ngữ nghĩa.
    /// Khác với Drain3 (rule-based pattern matching), classifier này hiểu "ý nghĩa" của log.
    /// </summary>
    public interface ISemanticLogClassifier
    {
        /// <summary>
        /// Phân loại một dòng log thành category/subcategory.
        /// </summary>
        /// <param name="logLine">Raw log line (ví dụ: "User authentication failed: Invalid password")</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>LogClassification với category, confidence score, và extracted fields</returns>
        Task<LogClassification> ClassifyAsync(string logLine, CancellationToken ct = default);

        /// <summary>
        /// Tính confidence score cho một category cụ thể.
        /// Dùng để verify prediction hoặc so sánh với threshold.
        /// </summary>
        /// <param name="logLine">Raw log line</param>
        /// <param name="expectedCategory">Category cần kiểm tra (ví dụ: "auth_failure")</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Confidence score từ 0.0 đến 1.0</returns>
        Task<float> GetConfidenceAsync(string logLine, string expectedCategory, CancellationToken ct = default);
    }
    /// <summary>
    /// Kết quả phân loại log.
    /// </summary>
    public record LogClassification(
        /// <summary>Category chính (ví dụ: "authentication", "database", "network")</summary>
        string Category,

        /// <summary>SubCategory chi tiết (ví dụ: "mfa_timeout", "connection_pool_exhausted")</summary>
        string SubCategory,

        /// <summary>Confidence score (0.0 - 1.0). Nếu < 0.7 nên fallback sang Drain3</summary>
        float Confidence,

        /// <summary>
        /// Các fields được extract từ log (ví dụ: {"user": "admin@corp.com", "ip": "192.168.1.1"})
        /// </summary>
        Dictionary<string, string> ExtractedFields
    );
}