import * as React from "react"
import { useNavigate, useSearchParams } from "react-router-dom"
import { Button } from "@/components/ui/button"
import * as api from "@/auth/api"
import { Eye, EyeOff } from "lucide-react"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"

function inputClassName(hasError: boolean) {
  return [
    "h-10 w-full rounded-md border bg-zinc-950 px-3 text-sm text-zinc-100",
    "placeholder:text-zinc-500 focus-visible:outline-none focus-visible:ring-1",
    hasError ? "border-red-500/70 focus-visible:ring-red-500/60" : "border-zinc-700 focus-visible:ring-zinc-500",
  ].join(" ")
}

export function ResetPasswordPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const tokenFromUrl = searchParams.get("token") || ""
  const { t } = useTranslation()

  const [token, setToken] = React.useState(tokenFromUrl)
  const [password, setPassword] = React.useState("")
  const [confirmPassword, setConfirmPassword] = React.useState("")
  const [showPassword, setShowPassword] = React.useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = React.useState(false)
  const [submitting, setSubmitting] = React.useState(false)
  const [touched, setTouched] = React.useState<{ [k: string]: boolean }>({})
  const [error, setError] = React.useState<string | null>(null)
  const [success, setSuccess] = React.useState(false)

  React.useEffect(() => {
    if (tokenFromUrl) {
      setToken(tokenFromUrl)
    }
  }, [tokenFromUrl])

  const tokenError = React.useMemo(() => {
    if (!token.trim()) return t('auth.validation.tokenRequired')
    return null
  }, [token, t])

  const passwordError = React.useMemo(() => {
    if (!password.trim()) return t('auth.validation.passwordRequired')
    if (password.length < 8) return t('auth.validation.passwordMinLength')
    if (!/[a-z]/.test(password)) return t('auth.validation.passwordLowercase')
    if (!/[A-Z]/.test(password)) return t('auth.validation.passwordUppercase')
    if (!/[0-9]/.test(password)) return t('auth.validation.passwordNumber')
    if (!/[\W_]/.test(password)) return t('auth.validation.passwordSpecial')
    return null
  }, [password, t])

  const confirmError = React.useMemo(() => {
    if (!confirmPassword.trim()) return t('auth.validation.confirmPasswordRequired')
    if (confirmPassword !== password) return t('auth.validation.passwordsDoNotMatch')
    return null
  }, [confirmPassword, password, t])

  const canSubmit = !tokenError && !passwordError && !confirmError

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setError(null)

    try {
      await api.resetPassword({ token, password })
      toast.success(t('settings.changePasswordSuccess'))
      setSuccess(true)
      setTimeout(() => {
        navigate("/login")
      }, 2000)
    } catch (err) {
      const error = err as Error
      setError(error.message || t('errors.generic'))
      toast.error(error.message || t('errors.generic'))
    } finally {
      setSubmitting(false)
    }
  }

  const SuccessView = (
    <div className="min-h-screen bg-background flex items-center justify-center p-6 relative overflow-hidden">
      <div className="absolute top-0 left-0 w-full h-full pointer-events-none">
        <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-primary/20 rounded-full blur-[120px]" />
        <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-purple-500/10 rounded-full blur-[120px]" />
      </div>
      <div className="w-full max-w-md rounded-2xl border border-sidebar-border bg-sidebar/80 backdrop-blur-xl shadow-2xl relative z-10 overflow-hidden p-8 text-center">
        <h1 className="text-2xl font-bold text-zinc-100">{t('auth.passwordResetSuccess')}</h1>
        <p className="text-muted-foreground mt-2">{t('auth.passwordResetSuccessMessage')}</p>
        <Button
          variant="vibrant"
          onClick={() => navigate('/login')}
          className="mt-6 w-full font-bold py-6 rounded-xl"
        >
          {t('auth.backToLogin')}
        </Button>
      </div>
    </div>
  )

  if (success) return SuccessView

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
            {t('auth.resetPassword')}
          </p>
        </div>

        <div className="p-8">
          {error && (
            <div className="mb-4 rounded-md border border-red-500/40 bg-red-500/10 px-3 py-2 text-sm text-red-200">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            {!tokenFromUrl && (
              <div className="space-y-1">
                <label htmlFor="token" className="text-sm text-zinc-300">
                  {t('auth.resetToken')}
                </label>
                <input
                  id="token"
                  type="text"
                  value={token}
                  onChange={(e) => setToken(e.target.value)}
                  onBlur={() => setTouched(t => ({ ...t, token: true }))}
                  className={inputClassName(!!tokenError && (touched.token || submitting))}
                  placeholder={t('auth.resetTokenPlaceholder')}
                />
                {tokenError && (touched.token || submitting) && (
                  <p className="text-xs text-red-500 mt-1">{tokenError}</p>
                )}
              </div>
            )}

            <div className="space-y-1">
              <label htmlFor="password" className="text-sm text-zinc-300">
                {t('auth.newPassword')}
              </label>
              <div className="relative">
                <input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  onBlur={() => setTouched(t => ({ ...t, password: true }))}
                  className={[
                    inputClassName(!!passwordError && (touched.password || submitting)),
                    "pr-10"
                  ].join(" ")}
                  placeholder={t('auth.newPasswordPlaceholder')}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 text-zinc-400 hover:text-zinc-200"
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
              {passwordError && (touched.password || submitting) && (
                <p className="text-xs text-red-500 mt-1">{passwordError}</p>
              )}
            </div>

            <div className="space-y-1">
              <label htmlFor="confirmPassword" className="text-sm text-zinc-300">
                {t('auth.confirmPassword')}
              </label>
              <div className="relative">
                <input
                  id="confirmPassword"
                  type={showConfirmPassword ? "text" : "password"}
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  onBlur={() => setTouched(t => ({ ...t, confirmPassword: true }))}
                  className={[
                    inputClassName(!!confirmError && (touched.confirmPassword || submitting)),
                    "pr-10"
                  ].join(" ")}
                  placeholder={t('auth.confirmPasswordPlaceholder')}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 text-zinc-400 hover:text-zinc-200"
                >
                  {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
              {confirmError && (touched.confirmPassword || submitting) && (
                <p className="text-xs text-red-500 mt-1">{confirmError}</p>
              )}
            </div>

            <Button
              type="submit"
              variant="vibrant"
              className="w-full mt-2 text-white font-bold py-6 rounded-xl shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-50"
              disabled={!canSubmit || submitting}
            >
              {submitting ? t('auth.resettingPassword') : t('auth.resetPassword')}
            </Button>

            <div className="flex justify-between pt-2">
              <Button
                type="button"
                variant="link"
                className="text-zinc-400 hover:text-white px-0"
                onClick={() => navigate('/login')}
              >
                {t('auth.backToLogin')}
              </Button>
              <Button
                type="button"
                variant="link"
                className="text-zinc-400 hover:text-white px-0"
                onClick={() => navigate('/forgot-password')}
              >
                {t('auth.requestNewToken')}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
