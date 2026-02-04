using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ControlHub.Application.Common.Interfaces.AI.V3.Agentic;
using ControlHub.Application.Common.Interfaces.AI.V3.RAG;
using ControlHub.Application.Common.Interfaces.AI.V3.Reasoning;
using ControlHub.Application.Common.Interfaces.AI.V3.Observability;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.AI.V3.Agentic.Nodes
{
    /// <summary>
    /// ExecutorNode - Execute plan steps using tools and RAG.
    /// </summary>
    public class ExecutorNode : IAgentNode
    {
        private readonly IAgenticRAG _agenticRag;
        private readonly IReasoningModel _reasoningModel;
        private readonly IAgentObserver? _observer;
        private readonly ILogger<ExecutorNode> _logger;

        public string Name => "Executor";
        public string Description => "Executes plan steps using available tools";

        public ExecutorNode(
            IAgenticRAG agenticRag, 
            IReasoningModel reasoningModel,
            IAgentObserver? observer, 
            ILogger<ExecutorNode> logger)
        {
            _agenticRag = agenticRag;
            _reasoningModel = reasoningModel;
            _observer = observer;
            _logger = logger;
        }

        public async Task<IAgentState> ExecuteAsync(IAgentState state, CancellationToken ct = default)
        {
            var clone = (AgentState)state.Clone();

            // Get plan and current step
            var plan = clone.GetContext<List<string>>("plan");
            var currentStep = clone.GetContextValue("current_step", 0);

            if (plan == null || !plan.Any())
            {
                clone.Error = "No plan found. Planner node must run first.";
                clone.IsComplete = true;
                return clone;
            }

            if (currentStep >= plan.Count)
            {
                // All steps completed
                clone.Context["execution_complete"] = true;
                _logger.LogInformation("All {Count} steps executed", plan.Count);
                return clone;
            }

            var step = plan[currentStep];
            _logger.LogInformation("Executing step {StepNum}/{Total}: {Step}", 
                currentStep + 1, plan.Count, step);

            // Get correlationId from state (if provided)
            var correlationId = clone.GetContext<string>("correlationId");

            // Use RAG to gather relevant information
            var ragOptions = new AgenticRAGOptions(CorrelationId: correlationId);
            var ragResult = await _agenticRag.RetrieveAsync(step, ragOptions, ct);

            string stepResult;
            if (ragResult.Documents.Any())
            {
                // Use reasoning model to interpret and summarize the findings for THIS step
                var analysisContext = new ReasoningContext(
                    Query: $"Based on the following retrieved logs, provide a concise summary of the findings for the task step: '{step}'",
                    RetrievedDocs: ragResult.Documents
                );

                var analysis = await _reasoningModel.ReasonAsync(analysisContext, new ReasoningOptions(Temperature: 0.3f), ct);
                stepResult = $"Step {currentStep + 1}: {step}\n{analysis.Solution}\n_(Source: {ragResult.Documents.Count} logs via {ragResult.StrategyUsed})_";
            }
            else
            {
                stepResult = $"Step {currentStep + 1}: {step}\nNo relevant log entries found for this step.";
            }

            // Store execution result
            var executionResults = clone.GetContext<List<string>>("execution_results") ?? new List<string>();
            executionResults.Add(stepResult);
            clone.Context["execution_results"] = executionResults;

            // Move to next step
            clone.Context["current_step"] = currentStep + 1;

            clone.Messages.Add(new AgentMessage(
                "tool",
                $"Executed step {currentStep + 1}: {step}",
                "Executor"
            ));

            return clone;
        }
    }
}
