using ControlHub.Application.Common.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControlHub.Application.Common.Interfaces.AI
{
    public interface ILogKnowledgeService
    {
        Task IngestLogDefinitionsAsync();
        Task<string> AnalyzeSessionAsync(List<LogEntry> logs, string lang = "en");
        Task<string> ChatWithLogsAsync(string userQuestion, List<LogEntry> logs, string lang = "en");
    }
}
