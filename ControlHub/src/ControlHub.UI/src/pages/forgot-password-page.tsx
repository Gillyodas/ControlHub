import * as React from "react"
import { useNavigate } from "react-router-dom"

import { Button } from "@/components/ui/button"
import * as api from "@/auth/api"
import { detectIdentifierType, validateIdentifierValue } from "@/auth/validators"
import { useTranslation } from "react-i18next"

function inputClassName(hasError: boolean) {
  return [
    "h-10 w-full rounded-md border bg-zinc-950 px-3 text-sm text-zinc-100",
    "placeholder:text-zinc-500 focus-visible:outline-none focus-visible:ring-1",
    hasError ? "border-red-500/70 focus-visible:ring-red-500/60" : "border-zinc-700 focus-visible:ring-zinc-500",
  ].join(" ")
}

export function ForgotPasswordPage() {
  const navigate = useNavigate()
  const { t } = useTranslation()

  const [value, setValue] = React.useState("")
  const [submitting, setSubmitting] = React.useState(false)
  const [touched, setTouched] = React.useState(false)
  const [error, setError] = React.useState<string | null>(null)
  const [success, setSuccess] = React.useState(false)

  const identifierType = React.useMemo(() => detectIdentifierType(value), [value])
  const identifyError = React.useMemo(() => validateIdentifierValue(identifierType, value), [identifierType, value])

  const canSubmit = !identifyError

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setTouched(true)
    setError(null)

    if (!canSubmit) return

    setSubmitting(true)
    try {
      await api.forgotPassword({ value: value.trim(), type: identifierType })
      setSuccess(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : t('errors.generic'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-6 relative overflow-hidden">
      {/* Decorative gradients */}
      <div className="absolute top-0 left-0 w-full h-full pointer-events-none">
        <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-primary/20 rounded-full blur-[120px]" />
        <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-purple-500/10 rounded-full blur-[120px]" />
      </div>

      <div className="w-full max-w-md rounded-2xl border border-sidebar-border bg-sidebar/80 backdrop-blur-xl shadow-2xl relative z-10 overflow-hidden">
        <div className="p-8 border-b border-sidebar-border text-center">
          <h1 className="text-3xl font-extrabold bg-[var(--vibrant-gradient)] bg-clip-text text-transparent">
            ControlHub
          </h1>
          <p className="text-muted-foreground mt-2 font-medium">
            {t('auth.forgotPasswordTitle')}
          </p>
        </div>

        <div className="p-8">
          {error ? (
            <div className="mb-4 rounded-md border border-red-500/40 bg-red-500/10 px-3 py-2 text-sm text-red-200">{error}</div>
          ) : null}

          {success ? (
            <div className="mb-4 rounded-md border border-emerald-500/40 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-200">
              {t('auth.resetEmailSent')}
            </div>
          ) : null}

          <form onSubmit={onSubmit} className="space-y-4">
            <div>
              <label className="block text-sm text-zinc-300 mb-1">{t('auth.identify')}</label>
              <input
                value={value}
                onChange={(e) => setValue(e.target.value)}
                onBlur={() => setTouched(true)}
                placeholder={t('auth.identifyPlaceholder')}
                className={inputClassName(Boolean(touched && identifyError))}
              />
              {touched && identifyError ? (
                <p className="mt-1 text-xs text-red-300">{identifyError}</p>
              ) : (
                <p className="mt-1 text-xs text-zinc-500">
                  {t('auth.detectedType')}: {identifierType === 0 ? t('auth.email') : identifierType === 1 ? t('auth.phone') : t('auth.username')}
                </p>
              )}
            </div>

            <Button
              type="submit"
              variant="vibrant"
              disabled={!canSubmit || submitting}
              className="w-full text-white font-bold py-6 rounded-xl shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-50"
            >
              {submitting ? t('auth.sending') : t('auth.sendResetToken')}
            </Button>

            <div className="flex items-center justify-between pt-2">
              <Button
                type="button"
                variant="link"
                className="px-0 text-zinc-200 hover:text-white"
                onClick={() => navigate("/login")}
              >
                {t('auth.backToLogin')}
              </Button>
              <Button
                type="button"
                variant="link"
                className="px-0 text-zinc-200 hover:text-white"
                onClick={() => navigate("/reset-password")}
              >
                {t('auth.alreadyHaveToken')}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
