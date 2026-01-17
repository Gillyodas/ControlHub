import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { accountApi } from "@/services/api"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"

interface ChangePasswordDialogProps {
  userId: string
  accessToken: string
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function ChangePasswordDialog({ userId, accessToken, open, onOpenChange }: ChangePasswordDialogProps) {
  const { t } = useTranslation()
  const [currentPassword, setCurrentPassword] = useState("")
  const [newPassword, setNewPassword] = useState("")
  const [confirmPassword, setConfirmPassword] = useState("")
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (newPassword !== confirmPassword) {
      toast.error(t('auth.validation.passwordsDoNotMatch'))
      return
    }

    setIsLoading(true)
    try {
      await accountApi.changePassword(
        userId,
        {
          curPass: currentPassword,
          newPass: newPassword,
        },
        accessToken
      )
      toast.success(t('settings.passwordChangedSuccess'))
      onOpenChange(false)
      setCurrentPassword("")
      setNewPassword("")
      setConfirmPassword("")
    } catch (error) {
      toast.error(error instanceof Error ? error.message : t('settings.passwordChangeFailed'))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px] bg-sidebar/95 backdrop-blur-xl border-sidebar-border rounded-2xl shadow-2xl">
        <DialogHeader>
          <DialogTitle className="text-xl font-bold tracking-tight text-foreground">{t('settings.changePassword')}</DialogTitle>
          <DialogDescription className="text-muted-foreground">{t('settings.changePasswordDescription')}</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="current-password" ***>{t('settings.currentPassword')}</Label>
              <Input
                id="current-password"
                type="password"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                className="bg-sidebar-accent/50 border-sidebar-border focus:ring-sidebar-primary/50"
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="new-password" ***>{t('settings.newPassword')}</Label>
              <Input
                id="new-password"
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className="bg-sidebar-accent/50 border-sidebar-border focus:ring-sidebar-primary/50"
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="confirm-password" ***>{t('settings.confirmNewPassword')}</Label>
              <Input
                id="confirm-password"
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="bg-sidebar-accent/50 border-sidebar-border focus:ring-sidebar-primary/50"
                required
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:gap-0">
            <Button type="button" variant="ghost" onClick={() => onOpenChange(false)} className="rounded-xl font-bold">
              {t('common.cancel')}
            </Button>
            <Button type="submit" disabled={isLoading} variant="vibrant" className="rounded-xl font-bold shadow-lg shadow-sidebar-primary/20">
              {isLoading ? t('settings.changing') : t('settings.changePassword')}
            </Button>
          </DialogFooter>
        </form >
      </DialogContent >
    </Dialog >
  )
}
