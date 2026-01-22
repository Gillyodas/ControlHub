import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import ReactMarkdown from 'react-markdown';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Loader2, Brain, Sparkles, RefreshCw } from 'lucide-react';
import { useSearchParams } from 'react-router-dom';
import { fetchJson } from '@/services/api/client';

const AiAuditPage = () => {
    const { t, i18n } = useTranslation();
    const [searchParams] = useSearchParams();
    const [correlationId, setCorrelationId] = useState(searchParams.get('correlationId') || '');
    const [analysis, setAnalysis] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [knowledgeLoading, setKnowledgeLoading] = useState(false);

    // Auto-analyze if correlationId is in URL
    useEffect(() => {
        const idFromUrl = searchParams.get('correlationId');
        if (idFromUrl) {
            setCorrelationId(idFromUrl);
            analyzeLog(idFromUrl);
        }
    }, [searchParams]);



    // ... (imports remain)

    // ... (logic remains)

    const analyzeLog = async (id: string) => {
        if (!id) return;
        setLoading(true);
        setAnalysis(null);
        try {
            const lang = i18n.language || 'en';
            // fetchJson throws on error, so we catch it below
            const data = await fetchJson<{ analysis: string }>(`/api/audit/analyze/${id}?lang=${lang}`);
            setAnalysis(data.analysis);
        } catch (error) {
            console.error('AI Analysis Error:', error);
            setAnalysis('Error: Session not found or API error.');
        } finally {
            setLoading(false);
        }
    };

    const handleAnalyze = () => analyzeLog(correlationId);

    const handleLearn = async () => {
        setKnowledgeLoading(true);
        try {
            await fetch('/api/audit/learn', { method: 'POST' });
            alert(t('ai.knowledgeRefreshed', 'Knowledge Base updated!'));
        } catch (error) {
            alert('Failed to update knowledge base.');
        } finally {
            setKnowledgeLoading(false);
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">{t('ai.auditTitle', 'AI Log Audit')}</h2>
                    <p className="text-muted-foreground">
                        {t('ai.auditSubtitle', 'Analyze system logs and detect root causes using Local AI.')}
                    </p>
                </div>
                <Button variant="outline" onClick={handleLearn} disabled={knowledgeLoading}>
                    {knowledgeLoading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <RefreshCw className="mr-2 h-4 w-4" />}
                    {t('ai.refreshKnowledge', 'Refresh Knowledge')}
                </Button>
            </div>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                {/* Input Section */}
                <Card className="col-span-1">
                    <CardHeader>
                        <CardTitle>{t('ai.inputSession', 'Session Analysis')}</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="flex flex-col space-y-2">
                            <label className="text-sm font-medium">Correlation ID</label>
                            <Input
                                placeholder="e.g. 550e8400-e29b..."
                                value={correlationId}
                                onChange={(e) => setCorrelationId(e.target.value)}
                            />
                        </div>
                        <Button className="w-full" onClick={handleAnalyze} disabled={loading || !correlationId}>
                            {loading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <Sparkles className="mr-2 h-4 w-4" />}
                            {t('ai.analyzeBtn', 'Analyze with AI')}
                        </Button>
                    </CardContent>
                </Card>

                {/* Result Section */}
                <Card className="col-span-1 md:col-span-2 lg:col-span-2 h-full min-h-[500px]">
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <Brain className="h-5 w-5 text-primary" />
                            {t('ai.resultTitle', 'AI Insights')}
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="prose prose-sm dark:prose-invert max-w-none">
                        {loading && (
                            <div className="flex flex-col items-center justify-center h-64 space-y-4 text-muted-foreground">
                                <Loader2 className="h-8 w-8 animate-spin" />
                                <p>{t('ai.analyzing', 'Reading logs and thinking...')}</p>
                            </div>
                        )}
                        {!loading && analysis && (
                            <ReactMarkdown>{analysis}</ReactMarkdown>
                        )}
                        {!loading && !analysis && (
                            <div className="flex flex-col items-center justify-center h-64 text-muted-foreground border-2 border-dashed rounded-lg">
                                <Sparkles className="h-8 w-8 mb-2 opacity-50" />
                                <p>{t('ai.placeholder', 'Enter a Correlation ID to start analysis')}</p>
                            </div>
                        )}
                    </CardContent>
                </Card>
            </div>
        </div>
    );
};

export default AiAuditPage;
