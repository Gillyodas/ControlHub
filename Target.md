# 8. Phân tích Khoảng cách (Gap Analysis)

Bảng dưới đây tổng hợp các khoảng cách kỹ thuật chính cần được lấp đầy để đưa **AuditAI** từ V2.5 lên phiên bản kế tiếp.

| Lĩnh vực | Trạng thái AuditAI V2.5 (Hiện tại) | Trạng thái Mục tiêu (Target State) | Khoảng cách & Rủi ro |
| :--- | :--- | :--- | :--- |
| **Reasoning** | **Static Chain-of-Thought**<br>(Tuyến tính, một chiều). | **Dynamic ReAct / Reflection Loops**<br>(Đa chiều, tự sửa lỗi). | Thiếu khả năng thích ứng khi kế hoạch ban đầu thất bại.<br>_Rủi ro vận hành sai lệch._ |
| **Orchestration** | **Scripted Flows / Simple Chains.** | **LangGraph State Machines**<br>(Có trạng thái, kiểm soát luồng). | Khó khăn trong việc xử lý các quy trình phức tạp, kéo dài và cần sự bền bỉ (resilience). |
| **Data Parsing** | **Drain3**<br>(Static Trees). | **Semantic Log Parsing**<br>(LLM/Embedding based). | Bỏ sót các mẫu log mới lạ (Zero-day anomalies).<br>_Nhiễu dữ liệu đầu vào cho AI._ |
| **Knowledge** | **Static RAG**<br>(Single-pass retrieval). | **Agentic RAG**<br>(Iterative, Multi-hop retrieval). | Câu trả lời thiếu chiều sâu, dễ bị hallucination do thiếu ngữ cảnh đầy đủ. |
| **Log Sampling** | **Weighted Reservoir Sampling.** | **Time-decayed Priority Sampling / Semantic Sampling.** | Dữ liệu mẫu không phản ánh đúng tình trạng hiện tại của hệ thống (Concept Drift). |
| **Inference** | **Ollama Local Models**<br>(Unverified output). | **Hybrid Inference**<br>(Local cho task nhẹ, Cloud/Reasoning Model cho task khó). | Độ tin cậy thấp trong các tác vụ suy luận phức tạp. |

---

# 9. Kiến trúc Mục tiêu và Lộ trình Kỹ thuật (Technical Roadmap)

Để giải quyết các khoảng cách trên, báo cáo đề xuất lộ trình 3 giai đoạn để chuyển đổi AuditAI.

## 9.1. Giai đoạn 1: Nền tảng Suy luận & Dữ liệu (The Reasoning Foundation)

**Mục tiêu:** Đạt chuẩn **OpenAI Level 2** và khắc phục các điểm yếu về dữ liệu.

* **Triển khai Hybrid Log Parsing:**
    * Thay thế dần Drain3 bằng mô hình parsing lai.
    * Sử dụng **Drain3** cho các log có cấu trúc rõ ràng (tốc độ cao).
    * Sử dụng một **mô hình LLM nhỏ** (như Bert-based hoặc DistilBERT) để phân loại các log phi cấu trúc dựa trên ngữ nghĩa vector.
* **Nâng cấp RAG Pipeline:**
    * Tích hợp **Qdrant Hybrid Search** và **Reranking**.
    * Đảm bảo rằng mọi dữ liệu đưa vào *context window* của LLM đều có độ liên quan (relevance score) cao nhất.
* **Tích hợp Reasoning Models:**
    * Đối với các tác vụ phân tích nguyên nhân gốc (RCA), sử dụng các mô hình chuyên về suy luận (như **mô hình chuỗi o1** hoặc các biến thể **CoT fine-tuned**) thay vì mô hình chat thông thường.
    * Điều này giúp giảm thiểu ảo giác (hallucination).

## 9.2. Giai đoạn 2: Chuyển đổi Agentic & Orchestration (The Agentic Shift)

**Mục tiêu:** Đạt chuẩn **OpenAI Level 3** và khả năng thực thi phức tạp.

* **Adopt LangGraph:**
    * Tái cấu trúc toàn bộ logic điều phối sang **LangGraph**.
    * Xây dựng các đồ thị trạng thái cho các quy trình vận hành tiêu chuẩn (SOPs).
    * Định nghĩa rõ các node: **"Planner"**, **"Executor"**, và **"Verifier"**.
* **Implement Self-Correction Loops:**
    * Thiết kế các vòng lặp phản hồi (feedback loops).
    * Đây là mô hình **"Reflexion"** giúp tăng đáng kể độ chính xác thực thi:
        * **Generator:** Đề xuất giải pháp.
        * **Critic:** Kiểm tra giải pháp dựa trên quy tắc an toàn và lịch sử dữ liệu.
        * **Reflector:** Nếu Critic từ chối, Reflector phân tích lý do và yêu cầu Generator thử lại.
* **Dynamic Tooling:**
    * Chuyển từ việc gọi công cụ cứng nhắc sang cơ chế Agent tự lựa chọn công cụ dựa trên ngữ cảnh (**Context-aware tool selection**).

## 9.3. Giai đoạn 3: Tự chủ An toàn (Safe Autonomy)

**Mục tiêu:** Đạt chuẩn **Gartner Autonomous** với sự giám sát của con người.

* **Human-on-the-loop Guardrails:**
    * Thay vì yêu cầu con người phê duyệt mọi bước (**In-the-loop**), chuyển sang mô hình giám sát (**On-the-loop**). 
    * Con người thiết lập các chính sách (Policies) và Agent tự do hành động trong phạm vi đó.
    * Các hành động vượt ngưỡng rủi ro sẽ tự động kích hoạt yêu cầu phê duyệt.
* **Continuous Learning Pipeline:**
    * Dữ liệu về các lần sửa lỗi thành công và thất bại của Agent được tự động thu thập để tinh chỉnh (**fine-tune**) lại các mô hình suy luận.
    * Cập nhật **vector database**, tạo ra một hệ thống tự học (**self-improving system**).