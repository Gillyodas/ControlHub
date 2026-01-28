# AI Log Reading Implementation Research Report

## 1. Technology Stack

The current implementation leverages a modern, hybrid AI stack designed for local execution and privacy.

- **Frontend**: React (TypeScript) with Tailwind CSS / Shadcn UI.
- **Backend API**: ASP.NET Core 8 Web API.
- **AI Engine**: [Ollama](https://ollama.com/) (Local Inference).
  - **LLM**: `llama3` (Configurable via `appsettings.json`).
  - **Embeddings**: Used for RAG, served via Ollama.
- **Vector Database**: [Qdrant](https://qdrant.tech/) (Self-hosted/Local).
  - Used to store "Knowledge" about log definitions (`LogCode`).
- **Logging**: JSON-structured file logging (Serilog-compatible format).

## 2. Context & Data Flow

The system is designed to "audit" specific user sessions or system traces using a **Correlation ID**.

1.  **Log Generation**:
    - The application generates structured logs with `CorrelationId`, `LogCode` (a systematic error code), `Level`, and `Message`.
    - Logs are stored in local JSON files (e.g., `Logs/log-20240126.json`).

2.  **Log Retrieval**:
    - `LogReaderService` scans the JSON log files.
    - It filters logs by `CorrelationId` to reconstruct a specific user session or request chain.
    - It can also retrieve "Recent Logs" for general chat debugging.

## 3. Current AI Approach: RAG (Retrieval-Augmented Generation)

The core innovation is combining **Structured Log Codes** with **Generative AI** using a RAG pattern.

### 3.1. Knowledge Ingestion (The "Learning" Phase)
The system "learns" what the logs mean by scanning the source code:
- **Source**: Static classes ending in `*Logs` (e.g., `AuthLogs`, `OrderLogs`) which contain `LogCode` definitions.
- **Process**:
    1.  `LogKnowledgeService` reflects over these classes.
    2.  Extracts `Code` (e.g., "AUTH_001") and `Message` (e.g., "Invalid credentials").
    3.  Generates vector embeddings for these definitions using Ollama.
    4.  Stores them in Qdrant metadata.
- **Benefit**: The AI knows exactly what "AUTH_001" means, even if the runtime log message is truncated or obscured.

### 3.2. Session Analysis (The "Auditing" Phase)
When a user asks to analyze a session (via `AiAuditPage`):
1.  **Retrieve**: Fetch all logs associated with the `CorrelationId`.
2.  **Context Building (RAG)**:
    - Identify unique `LogCode`s in the session.
    - (Optional/Demo) query Qdrant for related documentation or similar error concepts to enrich the context.
3.  **Prompt Engineering**:
    - **Optimization**: Prioritizes `Error` and `Warning` logs. Truncates long messages (max 500 chars). Limits total history to avoid context window overflow.
    - **Instruction**: "You are an expert system troubleshooter."
    - **Localization**: Forces output in the user's interface language (e.g., Vietnamese, English).
4.  **Inference**:
    - Sends the enriched prompt to `llama3` via Ollama.
    - Returns a markdown-formatted root cause analysis.

### 3.3. Interactive Chat
- Allows developers to "Chat with Logs" (e.g., "Why did the payment fail?").
- Uses a sliding window of the last 50 logs.
- Similar prompting strategy to Session Analysis but optimized for Q&A.

## 4. Key Components Analysis

| Component | File | Responsibilities |
| :--- | :--- | :--- |
| **LogReaderService** | `Infrastructure\Logging\LogReaderService.cs` | High-performance reading of line-delimited JSON logs. Filtering by Time/CorrelationId. |
| **LocalAIAdapter** | `Infrastructure\AI\LocalAIAdapter.cs` | HTTP Client wrapper for Ollama API (`/api/generate`). Handling timeouts and JSON parsing. |
| **LogKnowledgeService** | `Application\AI\LogKnowledgeService.cs` | **The Brain**. Orchestrates Ingestion (Learning) and Analysis (RAG). Manages the Prompt context window. |
| **AiAuditPage** | `UI\src\pages\AiAuditPage.tsx` | User Interface. Handles two modes: "Session Analysis" (by ID) and "Chat" (general debugging). |

## 5. Potential Areas for Development/Research

Based on the code review, here are directions for future research:
1.  **Vector DB Usage**: Currently, the RAG implementation is basic (finding "related concepts"). It could be expanded to store **Solutions/Runbooks** (e.g., "If AUTH_001 happens, check Redis").
2.  **Log Volume**: Reading JSON files linearly (`LogReaderService`) will not scale for gigabytes of logs. Consider moving to a real Log Database (Elasticsearch/Loki/Seq) for the storage layer while keeping the AI logic.
3.  **Context Window Management**: The current "TakeLast(50)" approach is naive. Smart filtering (preserving the *first* error and the *last* generic log) would be better.
4.  **Agentic Capabilities**: The AI currently just "reads". It could be upgraded to "act" (e.g., "I see a database lock. Should I kill the session?").
