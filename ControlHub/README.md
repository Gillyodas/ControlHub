# ControlHub

Identity & Access Management System with AI-powered Audit capabilities.

## Audit AI V2.5 (Hybrid Agentic RAG)

The Audit AI module has been upgraded to version 2.5, introducing a hybrid architecture that combines standard Log Analysis with an Agentic Workflow.

### Features
*   **Drain3 Log Parsing**: High-performance log parsing using a tree-based algorithm to extract templates and mask variables (IPs, IDs).
*   **Weighted Reservoir Sampling**: Intelligent sampling strategy that prioritizes critical (Error/Fatal) and rare logs for analysis, ensuring important events aren't missed.
*   **Agentic Workflow (ReAct)**: An autonomous agent that can:
    *   Parse and Sample logs.
    *   Search a Runbook Knowledge Base (Qdrant) for known solutions.
    *   Reason about root causes using Chain-of-Thought prompts.
*   **Runbook Management**: Store and retrieve operational runbooks using semantic search.

### Setup Instructions

1.  **Prerequisites**:
    *   Docker & Docker Compose
    *   Qdrant (Vector Database)
    *   Ollama (LLM) running `llama3` or compatible model.

2.  **Configuration**:
    Ensure `appsettings.json` has the following section:
    ```json
    "AuditAI": {
        "Version": "V2.5",
        "Drain3Enabled": true,
        "SamplingStrategy": "WeightedReservoir",
        "Qdrant": {
            "Host": "http://localhost:6333",
            "CollectionName": "ControlHub_Runbooks"
        }
    }
    ```

3.  **Docker Support**:
    The `docker-compose.yml` includes the `qdrant` service.
    ```bash
    docker-compose up -d qdrant
    ```

4.  **Ingesting Runbooks**:
    Use the `POST /api/Audit/ingest-runbooks` endpoint to load your initial set of runbooks.
    Example payload:
    ```json
    [
        {
            "Title": "Database Connection Timeout",
            "Symptoms": ["Connection timed out", "SQL Exception", "Error 500"],
            "RootCause": "Connection pool exhaustion or network partition.",
            "Resolution": "Check database server status and scale connection pool."
        }
    ]
    ```

### Usage

*   **Analyze Session**: `GET /api/Audit/analyze/{correlationId}`
    *   Triggers the V2.5 Agentic workflow.
    *   Returns analysis, identified templates, and tool investigation steps.
*   **Check Version**: `GET /api/Audit/version`

## Architecture

*   **Application Layer**: Defines `IAuditAgentService`, `ILogParserService`, `ISamplingStrategy`.
*   **Infrastructure Layer**: Implements `Drain3ParserService`, `WeightedReservoirSamplingStrategy`, `QdrantVectorStore`.
*   **API Layer**: Exposes endpoints via `AuditController`.
