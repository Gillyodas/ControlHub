import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { usersApi } from "@/services/api"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"

interface UpdateUsernameDialogProps {
  userId: string
  currentUsername: string
  accessToken: string
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: (newUsername: string) => void
}

export function UpdateUsernameDialog({
  userId,
  currentUsername,
  accessToken,
  open,
  onOpenChange,
  onSuccess,
}: UpdateUsernameDialogProps) {
  const { t } = useTranslation()
  const [username, setUsername] = useState(currentUsername)
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (username.trim() === currentUsername) {
      toast.info(t('users.usernameSameError'))
      return
    }

    setIsLoading(true)
    try {
      const result = await usersApi.updateUsername(userId, { username: username.trim() }, accessToken)
      toast.success(t('users.updateUsernameSuccess'))
      onSuccess(result.username)
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof Error ? error.message : t('users.updateFailed'))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>{t('users.updateUsername')}</DialogTitle>
          <DialogDescription>{t('users.updateUsernameDescription')}</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="username">{t('users.newUsername')}</Label>
              <Input
                id="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder={t('users.newUsernamePlaceholder')}
                required
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? t('users.updating') : t('users.updateUsername')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
