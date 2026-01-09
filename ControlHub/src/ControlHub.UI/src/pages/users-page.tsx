import { useState } from "react"
import { Button } from "@/components/ui/button"
import { UpdateUsernameDialog } from "@/components/users/update-username-dialog"
import { useAuth } from "@/auth/use-auth"
import { User } from "lucide-react"

export function UsersPage() {
  const { auth } = useAuth()
  const [updateUsernameOpen, setUpdateUsernameOpen] = useState(false)
  const [username, setUsername] = useState(auth?.username || "")

  const handleUsernameUpdate = (newUsername: string) => {
    setUsername(newUsername)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">User Profile</h1>
        <p className="text-muted-foreground">Manage your user profile</p>
      </div>

      <div className="rounded-lg border p-6 space-y-4">
        <h2 className="text-xl font-semibold">Profile Information</h2>
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
          <div className="pt-3 border-t">
            <p className="font-medium">Account ID</p>
            <p className="text-sm text-muted-foreground font-mono">{auth?.accountId}</p>
          </div>
        </div>
      </div>

      {auth && (
        <UpdateUsernameDialog
          userId={String(auth.accountId)}
          currentUsername={username}
          accessToken={auth.accessToken}
          open={updateUsernameOpen}
          onOpenChange={setUpdateUsernameOpen}
          onSuccess={handleUsernameUpdate}
        />
      )}
    </div>
  )
}
