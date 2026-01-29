import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { Loader2, Plus, AlertCircle, CheckCircle2 } from 'lucide-react';
import { fetchJson } from '@/services/api/client';
import { loadAuth } from '@/auth/storage';

interface RunbookIngestDialogProps {
    onSuccess?: () => void;
}

const RunbookIngestDialog = ({ onSuccess }: RunbookIngestDialogProps) => {
    const { t } = useTranslation();
    const [open, setOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [jsonMode, setJsonMode] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    // Form State
    const [logCode, setLogCode] = useState('');
    const [problem, setProblem] = useState('');
    const [solution, setSolution] = useState('');
    const [tags, setTags] = useState('');

    // JSON Mode State
    const [rawJson, setRawJson] = useState('[\n  {\n    "LogCode": "ERR_001",\n    "Problem": "...",\n    "Solution": "...",\n    "Tags": ["tag1"]\n  }\n]');

    const resetForm = () => {
        setLogCode('');
        setProblem('');
        setSolution('');
        setTags('');
        setError(null);
        setSuccess(false);
    };

    const handleSubmit = async () => {
        setLoading(true);
        setError(null);
        setSuccess(false);

        try {
            let payload: any[] = [];

            if (jsonMode) {
                try {
                    payload = JSON.parse(rawJson);
                    if (!Array.isArray(payload)) {
                        payload = [payload];
                    }
                } catch (e) {
                    throw new Error("Invalid JSON format");
                }
            } else {
                if (!problem || !solution) {
                    throw new Error("Problem and Solution are required.");
                }
                payload = [{
                    LogCode: logCode,
                    Problem: problem,
                    Solution: solution,
                    Tags: tags.split(',').map(tag => tag.trim()).filter(t => t)
                }];
            }

            const accessToken = loadAuth()?.accessToken || '';
            await fetchJson('/api/audit/ingest-runbooks', {
                method: 'POST',
                body: payload,
                accessToken
            });

            setSuccess(true);
            setTimeout(() => {
                setOpen(false);
                resetForm();
                onSuccess?.();
            }, 1500);

        } catch (err: any) {
            console.error(err);
            setError(err.message || "Failed to ingest runbooks.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>
                <Button variant="outline" size="sm">
                    <Plus className="mr-2 h-4 w-4" />
                    {t('ai.ingestRunbook', 'Add Runbook')}
                </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-[425px] md:max-w-[600px]">
                <DialogHeader>
                    <DialogTitle>{t('ai.ingestTitle', 'Ingest Runbook Knowledge')}</DialogTitle>
                </DialogHeader>

                <div className="flex items-center space-x-2 mb-4">
                    <Switch id="json-mode" checked={jsonMode} onCheckedChange={setJsonMode} />
                    <Label htmlFor="json-mode">Advanced JSON Mode</Label>
                </div>

                {success ? (
                    <div className="flex flex-col items-center justify-center py-8 text-green-600">
                        <CheckCircle2 className="h-12 w-12 mb-2" />
                        <p className="font-medium">Runbook Ingested Successfully!</p>
                    </div>
                ) : (
                    <div className="grid gap-4 py-4">
                        {error && (
                            <div className="bg-destructive/15 text-destructive text-sm p-3 rounded flex items-center">
                                <AlertCircle className="h-4 w-4 mr-2" />
                                {error}
                            </div>
                        )}

                        {jsonMode ? (
                            <div className="grid gap-2">
                                <Label>Raw JSON Array</Label>
                                <Textarea
                                    className="font-mono text-xs min-h-[300px]"
                                    value={rawJson}
                                    onChange={(e) => setRawJson(e.target.value)}
                                />
                            </div>
                        ) : (
                            <>
                                <div className="grid gap-2">
                                    <Label htmlFor="logcode">Log Code / Pattern (Optional)</Label>
                                    <Input
                                        id="logcode"
                                        placeholder="e.g. ERR_DB_TIMEOUT or 'Connection failed'"
                                        value={logCode}
                                        onChange={(e) => setLogCode(e.target.value)}
                                    />
                                </div>
                                <div className="grid gap-2">
                                    <Label htmlFor="problem">Problem Description <span className="text-destructive">*</span></Label>
                                    <Input
                                        id="problem"
                                        placeholder="What is the issue?"
                                        value={problem}
                                        onChange={(e) => setProblem(e.target.value)}
                                    />
                                </div>
                                <div className="grid gap-2">
                                    <Label htmlFor="solution">Solution / Root Cause <span className="text-destructive">*</span></Label>
                                    <Textarea
                                        id="solution"
                                        placeholder="How to fix it? (Markdown supported)"
                                        value={solution}
                                        onChange={(e) => setSolution(e.target.value)}
                                        className="min-h-[100px]"
                                    />
                                </div>
                                <div className="grid gap-2">
                                    <Label htmlFor="tags">Tags</Label>
                                    <Input
                                        id="tags"
                                        placeholder="Comma separated: db, timeout, sql"
                                        value={tags}
                                        onChange={(e) => setTags(e.target.value)}
                                    />
                                </div>
                            </>
                        )}
                    </div>
                )}

                {!success && (
                    <DialogFooter>
                        <Button variant="outline" onClick={() => setOpen(false)} disabled={loading}>
                            Cancel
                        </Button>
                        <Button onClick={handleSubmit} disabled={loading}>
                            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                            {jsonMode ? 'Ingest JSON' : 'Save Runbook'}
                        </Button>
                    </DialogFooter>
                )}
            </DialogContent>
        </Dialog>
    );
};

export default RunbookIngestDialog;
