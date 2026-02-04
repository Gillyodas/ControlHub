using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ControlHub.Application.Common.Interfaces.AI.V3.RAG;
using ControlHub.Application.Common.Interfaces.AI.V3.Reasoning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.AI.V3.Reasoning
{
    /// <summary>
    /// Reasoning Model Client sử dụng Ollama LLM.
    /// Chain-of-Thought prompting để generate step-by-step solutions.
    /// </summary>
    public class ReasoningModelClient : IReasoningModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReasoningModelClient> _logger;
        private readonly string _ollamaUrl;
        private readonly string _modelName;

        public ReasoningModelClient(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<ReasoningModelClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _ollamaUrl = config["AI:OllamaUrl"] ?? "http://localhost:11434/api/generate";
            _modelName = config["AI:ModelName"] ?? "llama3";
        }

        public async Task<ReasoningResult> ReasonAsync(
            ReasoningContext context,
            ReasoningOptions? options = null,
            CancellationToken ct = default)
        {
            options ??= new ReasoningOptions();

            try
            {
                // Build prompt với Chain-of-Thought
                var prompt = BuildPrompt(context, options);
                
                _logger.LogInformation("Reasoning for query: {Query}", context.Query);

                // Call Ollama
                var response = await CallOllamaAsync(prompt, options, ct);

                // Parse response
                var result = ParseResponse(response, context);

                _logger.LogInformation(
                    "Reasoning completed: {StepCount} steps, confidence {Confidence:F2}",
                    result.Steps.Count,
                    result.Confidence
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reasoning failed for query: {Query}", context.Query);
                return new ReasoningResult(
                    Solution: "Unable to generate solution due to an error.",
                    Explanation: ex.Message,
                    Steps: new List<string>(),
                    Confidence: 0f,
                    RawResponse: null
                );
            }
        }

        private string BuildPrompt(ReasoningContext context, ReasoningOptions options)
        {
            var sb = new StringBuilder();

            // System context
            sb.AppendLine("You are an expert IT auditor and troubleshooter.");
            sb.AppendLine("Analyze the following information and provide a solution.");
            sb.AppendLine();

            // User query
            sb.AppendLine("## User Query:");
            sb.AppendLine(context.Query);
            sb.AppendLine();

            // Classification (if available)
            if (context.Classification != null)
            {
                sb.AppendLine("## Log Classification:");
                sb.AppendLine($"- Category: {context.Classification.Category}");
                sb.AppendLine($"- SubCategory: {context.Classification.SubCategory}");
                sb.AppendLine($"- Confidence: {context.Classification.Confidence:F2}");
                sb.AppendLine();
            }

            // Retrieved documents
            if (context.RetrievedDocs.Count > 0)
            {
                sb.AppendLine("## Related Logs/Evidence:");
                var docsToInclude = context.RetrievedDocs.Take(options.MaxContextDocs);
                int i = 1;
                foreach (var doc in docsToInclude)
                {
                    sb.AppendLine($"[{i}] (Score: {doc.RelevanceScore:F2})");
                    sb.AppendLine($"    {doc.Content}");
                    i++;
                }
                sb.AppendLine();
            }

            // Instructions with Chain-of-Thought
            if (options.EnableCoT)
            {
                sb.AppendLine("## Instructions:");
                sb.AppendLine("Think step by step and provide your response in the following JSON format:");
                sb.AppendLine("IMPORTANT: Your entire response must be a single valid JSON object. Do not include any conversational text before or after the JSON.");
                sb.AppendLine("```json");
                sb.AppendLine("{");
                sb.AppendLine("  \"solution\": \"Brief solution summary\",");
                sb.AppendLine("  \"explanation\": \"Detailed explanation of the problem\",");
                sb.AppendLine("  \"steps\": [\"Step 1\", \"Step 2\", \"Step 3\"],");
                sb.AppendLine("  \"confidence\": 0.85");
                sb.AppendLine("}");
                sb.AppendLine("```");
            }
            else
            {
                sb.AppendLine("## Instructions:");
                sb.AppendLine("Provide a brief solution to the problem.");
            }

            return sb.ToString();
        }

        private async Task<string> CallOllamaAsync(
            string prompt,
            ReasoningOptions options,
            CancellationToken ct)
        {
            var requestBody = new
            {
                model = _modelName,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = options.Temperature,
                    num_predict = options.MaxTokens
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(_ollamaUrl, content, ct);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(responseJson);
            
            return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }

        private ReasoningResult ParseResponse(string rawResponse, ReasoningContext context)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
                return GetErrorResult("Empty response from LLM", rawResponse);

            try
            {
                // 1. Pre-sanitize: Remove common markdown and escaping artifacts
                var cleaned = SanitizeJson(rawResponse);

                // 2. Find JSON blocks
                var jsonStart = cleaned.IndexOf('{');
                var jsonEnd = cleaned.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonStr = cleaned.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    try
                    {
                        return ParseJson(jsonStr, rawResponse);
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Standard JSON parse failed, attempting aggressive cleaning...");
                        
                        // 3. Aggressive cleaning: Remove all escaped double quotes and literal backslashes
                        var aggressive = jsonStr.Replace("\\\"", "\"").Replace("\\\\", "\\");
                        try
                        {
                            return ParseJson(aggressive, rawResponse);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, "Aggressive cleaning failed. Raw snippet: {Snippet}", 
                                jsonStr.Length > 100 ? jsonStr.Substring(0, 100) : jsonStr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error during JSON parsing. Raw: {Raw}", rawResponse);
            }

            // Fallback: use raw response as solution if it looks somewhat meaningful
            return new ReasoningResult(
                Solution: rawResponse.Length > 200 ? "ID_" + rawResponse.GetHashCode().ToString("X") + ": " + rawResponse.Substring(0, 200) + "..." : rawResponse,
                Explanation: rawResponse,
                Steps: new List<string>(),
                Confidence: 0.3f,
                RawResponse: rawResponse
            );
        }

        private ReasoningResult GetErrorResult(string message, string? raw)
        {
            return new ReasoningResult(
                Solution: "Analysis failed due to response format issues.",
                Explanation: message,
                Steps: new List<string>(),
                Confidence: 0f,
                RawResponse: raw
            );
        }

        private ReasoningResult ParseJson(string jsonStr, string rawResponse)
        {
            var options = new JsonDocumentOptions { AllowTrailingCommas = true };
            using var doc = JsonDocument.Parse(jsonStr, options);
            var root = doc.RootElement;

            var solution = GetStringProperty(root, "solution");
            var explanation = GetStringProperty(root, "explanation");
            var confidence = root.TryGetProperty("confidence", out var c) ? (float)c.GetDouble() : 0.5f;

            var steps = new List<string>();
            if (root.TryGetProperty("steps", out var stepsArr) && stepsArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var step in stepsArr.EnumerateArray())
                {
                    var stepText = step.GetString();
                    if (!string.IsNullOrEmpty(stepText)) steps.Add(stepText);
                }
            }

            return new ReasoningResult(solution, explanation, steps, confidence, rawResponse);
        }

        private string GetStringProperty(JsonElement element, string propName)
        {
            return element.TryGetProperty(propName, out var p) ? p.GetString() ?? "" : "";
        }

        /// <summary>
        /// Sanitizes LLM response to ensure it can be parsed as JSON.
        /// </summary>
        private string SanitizeJson(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Remove Markdown code block markers
            var result = System.Text.RegularExpressions.Regex.Replace(input, @"```(json)?", "");
            
            // If the whole thing is wrapped in quotes (Ollama sometimes does this with double-escaping)
            if (result.Trim().StartsWith("\"") && result.Trim().EndsWith("\""))
            {
                result = result.Trim().Substring(1, result.Trim().Length - 2);
            }

            // Fix unescaped backslashes in paths NOT followed by valid escape sequence
            result = System.Text.RegularExpressions.Regex.Replace(result, @"(?<!\\)\\(?![""\\/bfnrtu])", @"\\");

            return result;
        }
    }
}
