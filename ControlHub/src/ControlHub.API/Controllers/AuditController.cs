using ControlHub.Application.AI;
using ControlHub.Application.Common.Logging;
using ControlHub.Application.Common.Logging.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly LogKnowledgeService _knowledgeService;
        private readonly ILogReaderService _logReader;

        public AuditController(LogKnowledgeService knowledgeService, ILogReaderService logReader)
        {
            _knowledgeService = knowledgeService;
            _logReader = logReader;
        }

        // Endpoint 1: Dạy cho AI biết về các Log Code hiện có (chạy 1 lần sau khi update code)
        [HttpPost("learn")]
        public async Task<IActionResult> LearnLogDefinitions()
        {
            await _knowledgeService.IngestLogDefinitionsAsync();
            return Ok("Ingestion started. Check Qdrant for vectors.");
        }

        // Endpoint 2: Phân tích Session
        [HttpGet("analyze/{correlationId}")]
        public async Task<IActionResult> Analyze(string correlationId, [FromQuery] string lang = "en")
        {
            // 1. Lấy log gốc
            var logs = await _logReader.GetLogsByCorrelationIdAsync(correlationId);
            if (logs.Count == 0) return NotFound("Log session not found.");

            // 2. Chạy qua RAG Service
            var result = await _knowledgeService.AnalyzeSessionAsync(logs, lang);

            return Ok(new
            {
                CorrelationId = correlationId,
                Analysis = result,
                LogCount = logs.Count
            });
        }
    }
}
