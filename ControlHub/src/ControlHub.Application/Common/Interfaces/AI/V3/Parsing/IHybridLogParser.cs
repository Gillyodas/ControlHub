using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ControlHub.Application.Common.Logging;
namespace ControlHub.Application.Common.Interfaces.AI.V3.Parsing
{
    /// <summary>
    /// Hybrid parser kết hợp Drain3 (fast, rule-based) và Semantic Classifier (slow, ML-based).
    /// Strategy: Dùng Drain3 trước, nếu confidence thấp thì dùng Semantic Classifier.
    /// </summary>
    public interface IHybridLogParser
    {
        /// <summary>
        /// Parse danh sách logs với hybrid strategy.
        /// </summary>
        /// <param name="logs">Raw log entries</param>
        /// <param name="options">Parsing options (confidence threshold, fallback behavior)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Parsed result với templates và metadata</returns>
        Task<HybridParseResult> ParseLogsAsync(
            List<LogEntry> logs,
            HybridParsingOptions? options = null,
            CancellationToken ct = default
        );

        /// <summary>
        /// Parse một log line đơn lẻ (dùng cho real-time processing).
        /// </summary>
        Task<ParsedLog> ParseSingleAsync(string logLine, CancellationToken ct = default);
    }
    /// <summary>
    /// Kết quả parsing với metadata về strategy được dùng.
    /// </summary>
    public record HybridParseResult(
        /// <summary>Danh sách templates (giống V2.5)</summary>
        List<LogTemplate> Templates,

        /// <summary>Mapping từ template ID → raw logs</summary>
        Dictionary<string, List<LogEntry>> TemplateToLogs,

        /// <summary>Metadata về parsing strategy</summary>
        ParsingMetadata Metadata
    );
    /// <summary>
    /// Metadata về quá trình parsing.
    /// </summary>
    public record ParsingMetadata(
        /// <summary>Số logs được parse bởi Drain3</summary>
        int Drain3Count,

        /// <summary>Số logs được parse bởi Semantic Classifier</summary>
        int SemanticCount,

        /// <summary>Số logs failed (không parse được)</summary>
        int FailedCount,

        /// <summary>Average confidence score</summary>
        float AverageConfidence,

        /// <summary>Thời gian xử lý (milliseconds)</summary>
        long ProcessingTimeMs
    );
    /// <summary>
    /// Kết quả parse một log line.
    /// </summary>
    public record ParsedLog(
        string OriginalLine,
        string Template,
        LogClassification? Classification,
        ParsingMethod Method,
        float Confidence
    );
    /// <summary>
    /// Method được dùng để parse log.
    /// </summary>
    public enum ParsingMethod
    {
        Drain3,
        Semantic,
        Failed
    }
    /// <summary>
    /// Options cho hybrid parsing.
    /// </summary>
    public record HybridParsingOptions(
        /// <summary>Confidence threshold để fallback sang Semantic (default: 0.7)</summary>
        float ConfidenceThreshold = 0.7f,

        /// <summary>Có enable Semantic Classifier không (default: true)</summary>
        bool EnableSemantic = true,

        /// <summary>Có enable Drain3 không (default: true)</summary>
        bool EnableDrain3 = true,

        /// <summary>Max logs để xử lý bằng Semantic (tránh quá chậm, default: 100)</summary>
        int MaxSemanticLogs = 100
    );
}