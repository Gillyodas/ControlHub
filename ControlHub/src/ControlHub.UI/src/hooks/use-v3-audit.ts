import { useState, useCallback } from 'react';
import { investigateV3, getAgentTrace, V3InvestigateRequest, V3InvestigateResponse, AgentTraceResponse } from '@/services/api/audit';

export interface UseV3AuditReturn {
    // State
    result: V3InvestigateResponse | null;
    trace: AgentTraceResponse | null;
    isLoading: boolean;
    error: string | null;

    // Actions
    investigate: (request: V3InvestigateRequest) => Promise<void>;
    fetchTrace: () => Promise<void>;
    reset: () => void;
}

export function useV3Audit(): UseV3AuditReturn {
    const [result, setResult] = useState<V3InvestigateResponse | null>(null);
    const [trace, setTrace] = useState<AgentTraceResponse | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const investigate = useCallback(async (request: V3InvestigateRequest) => {
        setIsLoading(true);
        setError(null);
        try {
            const response = await investigateV3(request);
            setResult(response);
        } catch (err) {
            const message = err instanceof Error ? err.message : 'Investigation failed';
            setError(message);
        } finally {
            setIsLoading(false);
        }
    }, []);

    const fetchTrace = useCallback(async () => {
        try {
            const response = await getAgentTrace();
            setTrace(response);
        } catch (err) {
            console.error('Failed to fetch trace:', err);
        }
    }, []);

    const reset = useCallback(() => {
        setResult(null);
        setTrace(null);
        setError(null);
    }, []);

    return {
        result,
        trace,
        isLoading,
        error,
        investigate,
        fetchTrace,
        reset,
    };
}
