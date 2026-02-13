import { useState } from 'react';
import ReactMarkdown from 'react-markdown';
import { Send, Loader2, CheckCircle, XCircle, History, RotateCcw } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { useV3Audit } from '@/hooks/use-v3-audit';

export function V3InvestigationPanel() {
    const [query, setQuery] = useState('');
    const [correlationId, setCorrelationId] = useState('');
    const [showTrace, setShowTrace] = useState(false);

    const { result, trace, isLoading, error, investigate, fetchTrace, reset } = useV3Audit();

    const handleInvestigate = async () => {
        if (!query.trim()) return;
        await investigate({
            query: query.trim(),
            correlationId: correlationId.trim() || undefined
        });
        // Fetch trace after investigation
        await fetchTrace();
    };

    const handleReset = () => {
        setQuery('');
        setCorrelationId('');
        reset();
    };

    return (
        <div className="space-y-6">
            {/* Input Section */}
            <Card>
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        🤖 V3.0 Agentic Investigation
                        <Badge variant="outline" className="ml-2">Plan → Execute → Verify → Reflect</Badge>
                    </CardTitle>
                    <CardDescription>
                        AI Agent sẽ phân tích câu hỏi, tạo kế hoạch, thực thi và tự đánh giá kết quả.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <Textarea
                        placeholder="Nhập câu hỏi để Agent điều tra... (VD: Tại sao login bị lỗi 401?)"
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        className="min-h-[100px]"
                    />
                    <div className="flex gap-4">
                        <Input
                            placeholder="Correlation ID (optional)"
                            value={correlationId}
                            onChange={(e) => setCorrelationId(e.target.value)}
                            className="flex-1"
                        />
                        <Button
                            onClick={handleInvestigate}
                            disabled={isLoading || !query.trim()}
                        >
                            {isLoading ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Đang điều tra...
                                </>
                            ) : (
                                <>
                                    <Send className="mr-2 h-4 w-4" />
                                    Bắt đầu
                                </>
                            )}
                        </Button>
                        {result && (
                            <Button variant="outline" onClick={handleReset}>
                                <RotateCcw className="mr-2 h-4 w-4" />
                                Reset
                            </Button>
                        )}
                    </div>
                </CardContent>
            </Card>

            {/* Error Display */}
            {error && (
                <Card className="border-red-500 bg-red-50 dark:bg-red-950">
                    <CardContent className="pt-6">
                        <div className="flex items-center gap-2 text-red-600 dark:text-red-400">
                            <XCircle className="h-5 w-5" />
                            <span className="font-medium">Lỗi: {error}</span>
                        </div>
                    </CardContent>
                </Card>
            )}

            {/* Loading State */}
            {isLoading && (
                <Card>
                    <CardContent className="pt-6">
                        <div className="flex flex-col items-center gap-4 py-8">
                            <Loader2 className="h-12 w-12 animate-spin text-primary" />
                            <div className="text-center">
                                <p className="font-medium">Agent đang điều tra...</p>
                                <p className="text-sm text-muted-foreground">
                                    Plan → Execute → Verify → Reflect
                                </p>
                            </div>
                        </div>
                    </CardContent>
                </Card>
            )}

            {/* Result Display */}
            {result && !isLoading && (
                <div className="space-y-4">
                    {/* Status Overview */}
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                {result.verificationPassed ? (
                                    <CheckCircle className="h-5 w-5 text-green-500" />
                                ) : (
                                    <XCircle className="h-5 w-5 text-red-500" />
                                )}
                                Kết quả điều tra
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
                                <div className="text-center p-3 bg-muted rounded-lg">
                                    <p className="text-2xl font-bold">{result.iterations}</p>
                                    <p className="text-sm text-muted-foreground">Iterations</p>
                                </div>
                                <div className="text-center p-3 bg-muted rounded-lg">
                                    <p className="text-2xl font-bold">{result.plan.length}</p>
                                    <p className="text-sm text-muted-foreground">Steps</p>
                                </div>
                                <div className="text-center p-3 bg-muted rounded-lg">
                                    <p className="text-2xl font-bold">{(result.confidence * 100).toFixed(0)}%</p>
                                    <p className="text-sm text-muted-foreground">Confidence</p>
                                </div>
                                <div className="text-center p-3 bg-muted rounded-lg">
                                    <Badge variant={result.verificationPassed ? 'default' : 'destructive'}>
                                        {result.verificationPassed ? 'Passed' : 'Failed'}
                                    </Badge>
                                    <p className="text-sm text-muted-foreground mt-1">Verification</p>
                                </div>
                            </div>

                            {/* Confidence Progress */}
                            <div className="mb-4">
                                <div className="flex justify-between mb-1">
                                    <span className="text-sm">Confidence Score</span>
                                    <span className="text-sm font-medium">{(result.confidence * 100).toFixed(1)}%</span>
                                </div>
                                <div className="w-full bg-muted rounded-full h-2">
                                    <div
                                        className="bg-primary h-2 rounded-full transition-all"
                                        style={{ width: `${result.confidence * 100}%` }}
                                    />
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Execution Plan */}
                    {result.plan.length > 0 && (
                        <Collapsible defaultOpen>
                            <Card>
                                <CardHeader>
                                    <CollapsibleTrigger className="flex w-full items-center justify-between">
                                        <CardTitle className="text-base">📋 Execution Plan ({result.plan.length} steps)</CardTitle>
                                    </CollapsibleTrigger>
                                </CardHeader>
                                <CollapsibleContent>
                                    <CardContent>
                                        <ol className="list-decimal list-inside space-y-2">
                                            {result.plan.map((step, i) => (
                                                <li key={i} className="text-sm">{step}</li>
                                            ))}
                                        </ol>
                                    </CardContent>
                                </CollapsibleContent>
                            </Card>
                        </Collapsible>
                    )}

                    {/* Answer */}
                    <Card>
                        <CardHeader>
                            <CardTitle className="text-base">📝 Answer</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="prose dark:prose-invert max-w-none">
                                <ReactMarkdown>{result.answer}</ReactMarkdown>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Agent Trace */}
                    {trace && (
                        <Collapsible open={showTrace} onOpenChange={setShowTrace}>
                            <Card>
                                <CardHeader>
                                    <CollapsibleTrigger className="flex w-full items-center justify-between">
                                        <CardTitle className="flex items-center gap-2 text-base">
                                            <History className="h-4 w-4" />
                                            Agent Trace ({trace.events.length} events)
                                        </CardTitle>
                                    </CollapsibleTrigger>
                                </CardHeader>
                                <CollapsibleContent>
                                    <CardContent>
                                        <div className="bg-muted rounded-lg p-4 font-mono text-xs overflow-x-auto">
                                            <pre>{trace.summary}</pre>
                                        </div>
                                    </CardContent>
                                </CollapsibleContent>
                            </Card>
                        </Collapsible>
                    )}
                </div>
            )}
        </div>
    );
}

export default V3InvestigationPanel;
