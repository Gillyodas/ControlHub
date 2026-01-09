import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { usersApi } from "@/services/api"
import { toast } from "sonner"

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
  const [username, setUsername] = useState(currentUsername)
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (username.trim() === currentUsername) {
      toast.info("Username is the same as current")
      return
    }

    setIsLoading(true)
    try {
      const result = await usersApi.updateUsername(userId, { username: username.trim() }, accessToken)
      toast.success("Username updated successfully")
      onSuccess(result.username)
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to update username")
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Update Username</DialogTitle>
          <DialogDescription>Change your username</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="username">New Username</Label>
              <Input
                id="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Enter new username"
                required
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? "Updating..." : "Update Username"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
