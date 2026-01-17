import { useState } from "react"
import { Button } from "@/components/ui/button"
import { ChangePasswordDialog } from "@/components/account/change-password-dialog"
import { UpdateUsernameDialog } from "@/components/users/update-username-dialog"
import { useAuth } from "@/auth/use-auth"
import { Lock, User, LogOut, Globe } from "lucide-react"
import { useTranslation } from "react-i18next"

export function SettingsPage() {
  const { auth, signOut } = useAuth()
  const { t, i18n } = useTranslation()
  const [changePasswordOpen, setChangePasswordOpen] = useState(false)
  const [updateUsernameOpen, setUpdateUsernameOpen] = useState(false)
  const [username, setUsername] = useState(auth?.username || "")

  const handleUsernameUpdate = (newUsername: string) => {
    setUsername(newUsername)
  }

  const handleLanguageChange = (lang: string) => {
    i18n.changeLanguage(lang)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('settings.title')}</h1>
        <p className="text-muted-foreground">{t('settings.description')}</p>
      </div>

      <div className="grid gap-6">
        <div className="rounded-lg border p-6 space-y-4">
          <h2 className="text-xl font-semibold">{t('settings.accountInformation')}</h2>
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <div>
                <p className="font-medium">{t('settings.username')}</p>
                <p className="text-sm text-muted-foreground">{username}</p>
              </div>
              <Button variant="outline" onClick={() => setUpdateUsernameOpen(true)}>
                <User className="h-4 w-4 mr-2" />
                {t('settings.updateUsername')}
              </Button>
            </div>
            <div className="flex items-center justify-between pt-3 border-t">
              <div>
                <p className="font-medium">{t('settings.password')}</p>
                <p className="text-sm text-muted-foreground">{t('settings.changePasswordDescription')}</p>
              </div>
              <Button variant="outline" onClick={() => setChangePasswordOpen(true)}>
                <Lock className="h-4 w-4 mr-2" />
                {t('settings.changePassword')}
              </Button>
            </div>
          </div>
        </div>

        <div className="rounded-lg border p-6 space-y-4">
          <h2 className="text-xl font-semibold">{t('settings.preferences')}</h2>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">{t('settings.language')}</p>
              <p className="text-sm text-muted-foreground">{t('settings.languageDescription')}</p>
            </div>
            <div className="flex gap-2">
              <Button
                variant={i18n.language === 'en' ? 'vibrant' : 'outline'}
                onClick={() => handleLanguageChange('en')}
                className="min-w-[80px]"
              >
                <Globe className="h-4 w-4 mr-2" />
                English
              </Button>
              <Button
                variant={i18n.language === 'vi' ? 'vibrant' : 'outline'}
                onClick={() => handleLanguageChange('vi')}
                className="min-w-[80px]"
              >
                <Globe className="h-4 w-4 mr-2" />
                Tiếng Việt
              </Button>
            </div>
          </div>
        </div>

        <div className="rounded-lg border p-6 space-y-4">
          <h2 className="text-xl font-semibold">{t('settings.accountActions')}</h2>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">{t('settings.signOut')}</p>
              <p className="text-sm text-muted-foreground">{t('settings.signOutDescription')}</p>
            </div>
            <Button variant="destructive" onClick={signOut}>
              <LogOut className="h-4 w-4 mr-2" />
              {t('settings.signOut')}
            </Button>
          </div>
        </div>
      </div>

      {auth && (
        <>
          <ChangePasswordDialog
            userId={String(auth.accountId)}
            accessToken={auth.accessToken}
            open={changePasswordOpen}
            onOpenChange={setChangePasswordOpen}
          />
          <UpdateUsernameDialog
            userId={String(auth.accountId)}
            currentUsername={username}
            accessToken={auth.accessToken}
            open={updateUsernameOpen}
            onOpenChange={(val) => setUpdateUsernameOpen(val)}
            onSuccess={handleUsernameUpdate}
          />
        </>
      )}
    </div>
  )
}
