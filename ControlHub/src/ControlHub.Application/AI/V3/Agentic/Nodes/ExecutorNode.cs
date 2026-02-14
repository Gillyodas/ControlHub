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

            // Phase 4 & 5: Batch Execution - Nếu currentStep > 0 nghĩa là đã thực hiện batch rồi
            if (currentStep > 0)
            {
                _logger.LogInformation("Batch execution already completed. Skipping Executor loop.");
                clone.Context["execution_complete"] = true;
                return clone;
            }

            _logger.LogInformation("Starting BATCH execution for {Count} steps", plan.Count);

            // Get correlationId from state (if provided)
            var correlationId = clone.GetContext<string>("correlationId");
            var originalQuery = clone.GetContext<string>("query") ?? "IT Investigation";

            // Step 1: Use pre-retrieved docs or call RAG once with the combined query
            var preRetrievedDocs = clone.GetContext<List<RankedDocument>>("pre_retrieval_docs");
            List<RankedDocument> evidence;

            if (preRetrievedDocs != null && preRetrievedDocs.Any())
            {
                _logger.LogInformation("Executor: Using {Count} pre-retrieved evidence documents", preRetrievedDocs.Count);
                evidence = preRetrievedDocs;
            }
            else
            {
                _logger.LogInformation("Executor: No pre-retrieval docs found, performing batch retrieval");
                var ragOptions = new AgenticRAGOptions(CorrelationId: correlationId);
                var ragResult = await _agenticRag.RetrieveAsync(originalQuery, ragOptions, ct);
                evidence = ragResult.Documents;
            }

            // Step 2: Build batch execution prompt — demands diagnosis, not step echo
            var planText = string.Join("\n", plan.Select((s, i) => $"{i + 1}. {s}"));
            var batchPrompt = 
                $"You are a senior IT auditor executing an investigation.\n\n" +
                $"## Original Query:\n{originalQuery}\n\n" +
                $"## Investigation Plan:\n{planText}\n\n" +
                $"## Your Task:\n" +
                $"Analyze the provided log evidence following the investigation plan above.\n" +
                $"Then produce a FINAL DIAGNOSIS with these 3 mandatory sections:\n\n" +
                $"1. **Problem Summary**: What specific error or issue occurred? Include the exact error code, HTTP status code, and affected endpoint.\n" +
                $"2. **Root Cause**: WHY did this happen? Quote the specific WARNING/ERROR log entries that prove the root cause. Ignore INFO-level noise like CORS.\n" +
                $"3. **Recommendation**: How to fix this? Provide concrete, actionable steps. Reference any matching runbook if available.\n\n" +
                $"IMPORTANT: Your 'solution' field MUST contain the final diagnosis. Your 'steps' field should contain the 3 sections above as separate items.\n" +
                $"Do NOT just restate the plan steps. Provide actual findings from the evidence.";

            var batchContext = new ReasoningContext(
                Query: batchPrompt,
                RetrievedDocs: evidence
            );

            // Step 3: Single LLM call for the entire plan
            var analysis = await _reasoningModel.ReasonAsync(
                batchContext, 
                new ReasoningOptions(Temperature: 0.2f, MaxTokens: 6144),
                ct
            );

            // Step 4: Store diagnosis results
            var executionResults = new List<string>();
            
            // Primary: Use the LLM's solution + explanation as the main diagnosis
            if (!string.IsNullOrEmpty(analysis.Solution) && analysis.Solution != "Partial Diagnosis")
            {
                executionResults.Add($"## Problem Summary\n{analysis.Solution}");
                
                if (!string.IsNullOrEmpty(analysis.Explanation) && analysis.Explanation != "Refer to raw response")
                {
                    executionResults.Add($"## Root Cause Analysis\n{analysis.Explanation}");
                }

                // Add structured steps as recommendation if available
                if (analysis.Steps.Any())
                {
                    var stepsText = string.Join("\n", analysis.Steps.Select((s, idx) => $"- {s}"));
                    executionResults.Add($"## Recommendation\n{stepsText}");
                }
            }
            else
            {
                // Fallback: Use raw steps if solution is empty
                _logger.LogWarning("LLM did not return structured diagnosis, using step-based fallback");
                foreach (var step in analysis.Steps)
                {
                    executionResults.Add(step);
                }
                if (!executionResults.Any())
                {
                    executionResults.Add($"Investigation completed but no structured findings were returned.\n_(Source: {evidence.Count} logs)_");
                }
            }

            clone.Context["execution_results"] = executionResults;
            clone.Context["current_step"] = plan.Count;
            clone.Context["execution_complete"] = true;
            clone.Context["diagnosis_solution"] = analysis.Solution;
            clone.Context["diagnosis_explanation"] = analysis.Explanation;

            clone.Messages.Add(new AgentMessage(
                "assistant",
                $"Diagnosis complete: {(analysis.Solution.Length > 100 ? analysis.Solution.Substring(0, 100) : analysis.Solution)}...",
                "Executor"
            ));


            return clone;
        }
    }
}
