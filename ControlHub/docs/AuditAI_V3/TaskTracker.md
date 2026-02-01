# AuditAI V3 Upgrade - Task Tracker

> Started: 2026-02-02 | Target: V3.0 Production Ready

---

## Phase 9.1: Reasoning Foundation (Week 1-4)

### Week 1-2: Semantic Log Parsing
- [ ] 9.1.1 - Tạo folder `Application/Common/Interfaces/AI/V3/`
- [ ] 9.1.2 - Define `ISemanticLogClassifier` interface
- [ ] 9.1.3 - Define `IHybridLogParser` interface
- [ ] 9.1.4 - Download DistilBERT ONNX model
- [ ] 9.1.5 - Implement `OnnxLogClassifier`
- [ ] 9.1.6 - Implement `HybridLogParser`
- [ ] 9.1.7 - Unit test: HybridLogParser
- [ ] 9.1.8 - Update appsettings.json V3 config
- [ ] 9.1.9 - Register Phase1 DI

### Week 2-3: Enhanced RAG
- [ ] 9.1.10 - Define `IReranker` interface
- [ ] 9.1.11 - Define `IMultiHopRetriever` interface
- [ ] 9.1.12 - Download Cross-Encoder ONNX model
- [ ] 9.1.13 - Implement `OnnxReranker`
- [ ] 9.1.14 - Implement `MultiHopRetriever`
- [ ] 9.1.15 - Define `IAgenticRAG` interface
- [ ] 9.1.16 - Implement `AgenticRAGService`
- [ ] 9.1.17 - Unit test: Reranking
- [ ] 9.1.18 - Integration test: RAG pipeline

### Week 3-4: Reasoning Integration
- [ ] 9.1.19 - Define `IReasoningModel` interface
- [ ] 9.1.20 - Define `IConfidenceScorer` interface
- [ ] 9.1.21 - Implement `ReasoningModelClient`
- [ ] 9.1.22 - Implement `ConfidenceScorer`
- [ ] 9.1.23 - Update AgenticAuditService (optional flag)
- [ ] 9.1.24 - Evaluation: V2.5 vs V3.0-Phase1

---

## Phase 9.2: Agentic Orchestration (Week 5-8)

### Week 5: State Machine Foundation
- [ ] 9.2.1 - Define `IAgentState` interface
- [ ] 9.2.2 - Implement `AgentState` class
- [ ] 9.2.3 - Define `IAgentNode` interface
- [ ] 9.2.4 - Define `IStateGraph` interface
- [ ] 9.2.5 - Implement `StateGraph`
- [ ] 9.2.6 - Unit test: 3-node graph

### Week 6: Core Agent Nodes
- [ ] 9.2.7 - Implement `PlannerNode`
- [ ] 9.2.8 - Implement `ExecutorNode`
- [ ] 9.2.9 - Implement `VerifierNode`
- [ ] 9.2.10 - Unit test: Each node
- [ ] 9.2.11 - Integration test: P→E→V flow

### Week 7: Reflexion Loop
- [ ] 9.2.12 - Define `IReflexionLoop` interface
- [ ] 9.2.13 - Implement `ReflectorNode`
- [ ] 9.2.14 - Implement `ReflexionLoop`
- [ ] 9.2.15 - Add reflexion edges
- [ ] 9.2.16 - Unit test: Reflexion
- [ ] 9.2.17 - Integration test: Full loop

### Week 8: Tool Registry & Main Agent
- [ ] 9.2.18 - Define `IToolRegistry` interface
- [ ] 9.2.19 - Implement `ToolRegistry`
- [ ] 9.2.20 - Register existing tools
- [ ] 9.2.21 - Define `IAuditAgentV3` interface
- [ ] 9.2.22 - Implement `AuditAgentV3`
- [ ] 9.2.23 - Wire up V3.0-Phase2 DI
- [ ] 9.2.24 - End-to-end test

---

## Phase 9.3: Production Hardening (Week 9-11)

### Week 9: Observability
- [ ] 9.3.1 - Define `IAgentObserver` interface
- [ ] 9.3.2 - Implement `AgentTracer`
- [ ] 9.3.3 - Implement `ThoughtLogger`
- [ ] 9.3.4 - Add observer hooks
- [ ] 9.3.5 - Dashboard view

### Week 10: Error Handling
- [ ] 9.3.6 - Graceful degradation V3→V2.5→V1
- [ ] 9.3.7 - Circuit breaker for ONNX
- [ ] 9.3.8 - Timeout handling
- [ ] 9.3.9 - Agent cancellation

### Week 11: Testing & Docs
- [ ] 9.3.10 - Regression test suite
- [ ] 9.3.11 - Performance benchmark
- [ ] 9.3.12 - Update Swagger docs
- [ ] 9.3.13 - Create V3 DeepDive doc
- [ ] 9.3.14 - Update appsettings.Example.json

---

## Summary

| Phase | Tasks | Est. Duration |
|-------|-------|---------------|
| 9.1 Reasoning Foundation | 24 | 4 weeks |
| 9.2 Agentic Orchestration | 24 | 4 weeks |
| 9.3 Production Hardening | 14 | 3 weeks |
| **Total** | **62** | **11 weeks** |
