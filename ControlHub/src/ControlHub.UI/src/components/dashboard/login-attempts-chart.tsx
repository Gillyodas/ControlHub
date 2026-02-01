import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useTranslation } from "react-i18next"
import type { LoginAttempt, ConnectionStatus } from "@/hooks/use-realtime-stats"

interface Props {
    attempts: LoginAttempt[]
    connectionStatus: ConnectionStatus
}

export function LoginAttemptsChart({ attempts, connectionStatus }: Props) {
    const { t } = useTranslation()

    const statusColor = {
        connecting: 'bg-yellow-500',
        connected: 'bg-emerald-500',
        disconnected: 'bg-zinc-500',
        error: 'bg-red-500'
    }

    return (
        <Card className="border-sidebar-border bg-sidebar/50 h-full">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
                <CardTitle className="text-base font-medium">{t('dashboard.recentActivity') || 'Recent Login Activity'}</CardTitle>
                <div className="flex items-center gap-2 text-xs text-muted-foreground">
                    <div className={`h-2 w-2 rounded-full ${statusColor[connectionStatus]} animate-pulse`} />
                    <span className="capitalize">{connectionStatus}</span>
                </div>
            </CardHeader>
            <CardContent>
                <div className="h-[300px] overflow-y-auto pr-2 space-y-2 custom-scrollbar">
                    {attempts.length === 0 ? (
                        <div className="h-full flex flex-col items-center justify-center text-muted-foreground italic gap-2">
                            <span className="text-sm">Waiting for live data...</span>
                        </div>
                    ) : (
                        attempts.map((attempt, idx) => (
                            <div
                                key={`${attempt.timestamp}-${idx}`}
                                className={`flex items-center justify-between p-3 rounded-md text-sm transition-all duration-300 animate-in fade-in slide-in-from-top-2 ${attempt.isSuccess
                                        ? 'bg-emerald-500/5 hover:bg-emerald-500/10 border-l-2 border-emerald-500'
                                        : 'bg-red-500/5 hover:bg-red-500/10 border-l-2 border-red-500'
                                    }`}
                            >
                                <div className="flex flex-col gap-1">
                                    <div className="flex items-center gap-2">
                                        <span className="font-mono text-xs text-muted-foreground">
                                            {new Date(attempt.timestamp).toLocaleTimeString()}
                                        </span>
                                        <span className="font-semibold">{attempt.maskedIdentifier}</span>
                                    </div>
                                    <span className="text-xs text-muted-foreground capitalize">
                                        {attempt.identifierType}
                                    </span>
                                </div>

                                <span className={`text-xs font-medium px-2 py-1 rounded-full ${attempt.isSuccess
                                        ? 'bg-emerald-500/10 text-emerald-500'
                                        : 'bg-red-500/10 text-red-500'
                                    }`}>
                                    {attempt.isSuccess ? 'Success' : (attempt.failureReason || 'Failed')}
                                </span>
                            </div>
                        ))
                    )}
                </div>
            </CardContent>
        </Card>
    )
}
