# Architecture Decision Record (ADR): Định Hướng Triển Khai Audit AI & Nâng Cao Chất Lượng RAG

## Title
[ADR-005] Chiến lược phát triển hệ thống Audit AI và Nâng cao chất lượng phản hồi RAG

## Status
**Proposed** (Đề xuất)

## Context
Hệ thống ControlHub hiện tại đang triển khai tính năng **AI Log Audit** ở mức độ thử nghiệm (Proof of Concept) với các đặc điểm:
- **Stack**: .NET 8 (Backend), React (Frontend), Ollama (LLM - Llama3), Qdrant (Vector DB).
- **Cơ chế**: Retrieval-Augmented Generation (RAG) dựa trên định nghĩa mã lỗi (`LogCode`).
- **Dữ liệu**: Log được lưu trữ dưới dạng file JSON phân tán (`LogReaderService` đọc trực tiếp file).

**Vấn đề hiện tại (Problem Statement):**
1.  **Hạn chế về Context Window**: Cơ chế hiện tại chỉ lấy "500 log cuối cùng" (`TakeLast(500)`). Điều này dẫn đến việc mất ngữ cảnh nếu lỗi xảy ra sớm hơn trong chuỗi xử lý hoặc bị trôi bởi các log thông tin (Info logs).
2.  **Chất lượng phản hồi RAG (Hallucination)**: AI hiện tại chỉ được cung cấp "Định nghĩa lỗi" (Error Definition). Nó thiếu kiến thức về "Giải pháp khắc phục" (Runbooks) cụ thể, dẫn đến việc AI tự suy diễn giải pháp chung chung, đôi khi không chính xác.
3.  **Khả năng mở rộng (Scalability)**: Việc đọc trực tiếp file JSON không tối ưu cho hiệu năng khi dung lượng log tăng lên (GBs).
4.  **Phụ thuộc vào Prompt**: Logic phân tích phụ thuộc quá nhiều vào một prompt dài và phức tạp, khó bảo trì và debug.

Yêu cầu đặt ra là cần một chiến lược kiến trúc để đưa Audit AI từ mức độ POC lên Production-ready, giải quyết bài toán độ chính xác và hiệu năng.

## Decision (Quyết định)

Chúng tôi quyết định áp dụng chiến lược **"Structured RAG with Smart Context & Knowledge Graph"** cho Audit AI. Cụ thể bao gồm các quyết định kỹ thuật sau:

### 1. Mở rộng Knowledge Base trong Vector DB (Qdrant)
Thay vì chỉ lưu trữ **Định nghĩa lỗi (Log Definitions)**, hệ thống sẽ mở rộng schema của Vector DB để lưu trữ thêm **Sổ tay xử lý sự cố (Runbooks/Solutions)**.
- **Trước đây**: Chỉ map `LogCode` -> `Message`.
- **Thay đổi**: Map `LogCode` + `Context` -> `Solution/Action Plan`.
- **Cơ chế**: Implement `IEnrichedKnowledgeService` để ingest tài liệu kỹ thuật vào Qdrant. Khi RAG query, hệ thống sẽ tìm kiếm cả định nghĩa lỗi VÀ giải pháp đề xuất tương ứng.

### 2. Thay đổi chiến lược lấy mẫu Log (Smart Context Filtering)
Thay thế thuật toán `TakeLast(500)` ngây thơ bằng thuật toán **Significance-Based Sampling**:
- Luôn giữ lại **Log lỗi đầu tiên** (Root Cause Candidate) trong phiên làm việc.
- Giữ lại các log `Warning`/`Error` khác.
- Lấy mẫu (Sample) các log `Info`/`Debug` xung quanh thời điểm xảy ra lỗi (± 5 log) để cung cấp ngữ cảnh.
- Loại bỏ các log nhiễu (noise) lặp lại.

### 3. Tách biệt lớp lưu trữ Log (Decouple Log Storage)
Để giải quyết vấn đề hiệu năng I/O:
- **Quyết định**: Trong ngắn hạn, vẫn giữ file JSON nhưng implement **Caching Layer** cho `LogReaderService`.
- **Dài hạn**: Log sẽ được đẩy (sink) về một Log Database chuyên dụng (như **Seq** hoặc **Elasticsearch**) để tận dụng khả năng search của DB engine, Application chỉ query kết quả thay vì scan file. AI Service sẽ hoạt động trên tập kết quả đã filter từ Log DB.

### 4. Semantic Prompting & Localization
- Chuẩn hóa prompt templates, tách biệt phần "System Core Instructions" và "Dynamic Context".
- Bắt buộc tham số `Language` trong mọi request AI để đảm bảo phản hồi đúng ngôn ngữ người dùng (như đã demo trong `LogKnowledgeService.cs`).

## Consequences (Hệ quả)

### Ưu điểm (Pros)
- **Độ chính xác cao hơn**: Nhờ có Runbooks trong RAG, AI sẽ đưa ra giải pháp cụ thể ("Check Redis connection string") thay vì chung chung ("Check database").
- **Cost Effective**: Tối ưu context window giúp giảm số lượng token gửi đi (quan trọng nếu sau này chuyển sang tính phí per-token).
- **Privacy**: Vẫn duy trì kiến trúc Local-First (Ollama + Local Qdrant), dữ liệu log nhạy cảm không rời khỏi hạ tầng doanh nghiệp.
- **Maintainability**: Việc tách knowledge (Vector DB) khỏi Logic code giúp cập nhật kiến thức xử lý lỗi mà không cần redeploy app.

### Nhược điểm (Cons)
- **Độ phức tạp tăng**: Cần xây dựng quy trình (pipeline) để cập nhật Runbook vào Vector DB. Developer phải viết tài liệu giải pháp cấu trúc rõ ràng.
- **Tài nguyên**: Vector DB sẽ tiêu tốn thêm RAM khi số lượng Runbook tăng lên.
- **Latency**: Thêm bước search Solution trong Vector DB có thể tăng độ trễ phản hồi thêm 100-200ms.

## Compliance (Tuân thủ)
- **Data Privacy**: Mọi log message trước khi gửi vào Embedding hoặc LLM phải được chạy qua bộ lọc **PII Sanitizer** (ẩn email, số điện thoại, password).
- **Audit Trail**: Mọi câu hỏi và câu trả lời của AI phải được log lại để admin rà soát chất lượng (Human-in-the-loop).

---
*Tài liệu này được lập bởi Senior Software Architect dựa trên phân tích codebase hiện tại và báo cáo nghiên cứu AI Log Reading.*
