import { fetchJson } from './client';
import { loadAuth } from '@/auth/storage';

// ═══════════════════════════════════════════════════════════════════
// V3 AUDIT API SERVICE
// ═══════════════════════════════════════════════════════════════════

export interface V3InvestigateRequest {
  query: string;
  correlationId?: string;
}

export interface V3InvestigateResponse {
  query: string;
  answer: string;
  plan: string[];
  executionResults: string[];
  verificationPassed: boolean;
  iterations: number;
  confidence: number;
  error?: string;
  version: string;
}

export interface AgentEvent {
  type: string;
  nodeName?: string;
  message: string;
  durationMs?: number;
  timestamp: string;
  data?: Record<string, unknown>;
}

export interface AgentTraceResponse {
  events: AgentEvent[];
  summary: string;
}

export interface AuditVersionInfo {
  version: string;
  features: string[];
}

const getToken = () => loadAuth()?.accessToken;

// Get AI version info
export const getAuditVersion = async (): Promise<AuditVersionInfo> => {
  return fetchJson<AuditVersionInfo>('/api/audit/version', { accessToken: getToken() });
};

// V3 Agentic Investigation
export const investigateV3 = async (request: V3InvestigateRequest): Promise<V3InvestigateResponse> => {
  return fetchJson<V3InvestigateResponse>('/api/audit/v3/investigate', {
    method: 'POST',
    body: request,
    accessToken: getToken()
  });
};

// V3 Agent Trace (for debugging)
export const getAgentTrace = async (): Promise<AgentTraceResponse> => {
  return fetchJson<AgentTraceResponse>('/api/audit/v3/trace', { accessToken: getToken() });
};

// Legacy V1/V2.5 APIs (existing)
export interface ChatRequest {
  question: string;
  startTime?: string;
  endTime?: string;
  correlationId?: string;
}

export interface ChatResponse {
  question: string;
  answer: string;
  logCount: number;
  toolsUsed?: string[];
  version: string;
}

export const chatWithLogs = async (request: ChatRequest): Promise<ChatResponse> => {
  return fetchJson<ChatResponse>('/api/audit/chat', {
    method: 'POST',
    body: request,
    accessToken: getToken()
  });
};

export interface AnalyzeResponse {
  correlationId: string;
  analysis: string;
  toolsUsed?: string[];
  templates?: unknown[];
  version: string;
}

export const analyzeSession = async (correlationId: string, lang: string = 'en'): Promise<AnalyzeResponse> => {
  return fetchJson<AnalyzeResponse>(`/api/audit/analyze/${correlationId}?lang=${lang}`, {
    accessToken: getToken()
  });
};

export const learnLogDefinitions = async (): Promise<void> => {
  await fetchJson<void>('/api/audit/learn', { method: 'POST', accessToken: getToken() });
};
