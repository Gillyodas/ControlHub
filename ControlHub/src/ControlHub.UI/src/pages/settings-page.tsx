import { useState } from "react"
import { Button } from "@/components/ui/button"
import { ChangePasswordDialog } from "@/components/account/change-password-dialog"
import { UpdateUsernameDialog } from "@/components/users/update-username-dialog"
import { useAuth } from "@/auth/use-auth"
import { Lock, User, LogOut } from "lucide-react"

export function SettingsPage() {
  const { auth, signOut } = useAuth()
  const [changePasswordOpen, setChangePasswordOpen] = useState(false)
  const [updateUsernameOpen, setUpdateUsernameOpen] = useState(false)
  const [username, setUsername] = useState(auth?.username || "")

  const handleUsernameUpdate = (newUsername: string) => {
    setUsername(newUsername)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Settings</h1>
        <p className="text-muted-foreground">Manage your account settings</p>
      </div>

      <div className="grid gap-6">
        <div className="rounded-lg border p-6 space-y-4">
          <h2 className="text-xl font-semibold">Account Information</h2>
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <div>
                <p className="font-medium">Username</p>
                <p className="text-sm text-muted-foreground">{username}</p>
              </div>
              <Button variant="outline" onClick={() => setUpdateUsernameOpen(true)}>
                <User className="h-4 w-4 mr-2" />
                Update Username
              </Button>
            </div>
            <div className="flex items-center justify-between pt-3 border-t">
              <div>
                <p className="font-medium">Password</p>
                <p className="text-sm text-muted-foreground">Change your password</p>
              </div>
              <Button variant="outline" onClick={() => setChangePasswordOpen(true)}>
                <Lock className="h-4 w-4 mr-2" />
                Change Password
              </Button>
            </div>
          </div>
        </div>

        <div className="rounded-lg border p-6 space-y-4">
          <h2 className="text-xl font-semibold">Account Actions</h2>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Sign Out</p>
              <p className="text-sm text-muted-foreground">Sign out of your account</p>
            </div>
            <Button variant="destructive" onClick={signOut}>
              <LogOut className="h-4 w-4 mr-2" />
              Sign Out
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
            onOpenChange={setUpdateUsernameOpen}
            onSuccess={handleUsernameUpdate}
          />
        </>
      )}
    </div>
  )
}
