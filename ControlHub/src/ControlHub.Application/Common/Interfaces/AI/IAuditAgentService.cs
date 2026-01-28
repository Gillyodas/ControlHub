using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControlHub.Application.Common.Interfaces.AI
{
    public interface IAuditAgentService
    {
        Task<AuditResult> InvestigateSessionAsync(string correlationId, string lang = "en");
        Task<string> ChatAsync(string question, string correlationId, string lang = "en");
    }

    public record AuditResult(
        string Analysis, 
        List<LogTemplate> ProcessedTemplates, 
        List<string> ToolsUsed
    );
}
