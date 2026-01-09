import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { permissionsApi } from "@/services/api"
import { toast } from "sonner"
import { Plus, X } from "lucide-react"

interface CreatePermissionsDialogProps {
  accessToken: string
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
}

export function CreatePermissionsDialog({ accessToken, open, onOpenChange, onSuccess }: CreatePermissionsDialogProps) {
  const [permissions, setPermissions] = useState<string[]>([""])
  const [isLoading, setIsLoading] = useState(false)

  const addPermission = () => {
    setPermissions([...permissions, ""])
  }

  const removePermission = (index: number) => {
    setPermissions(permissions.filter((_, i) => i !== index))
  }

  const updatePermission = (index: number, value: string) => {
    const updated = [...permissions]
    updated[index] = value
    setPermissions(updated)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const validPermissions = permissions.filter((p) => p.trim() !== "")
    if (validPermissions.length === 0) {
      toast.error("Please enter at least one permission")
      return
    }

    setIsLoading(true)
    try {
      await permissionsApi.createPermissions(
        { permissions: validPermissions },
        accessToken
      )
      toast.success("Permissions created successfully")
      setPermissions([""])
      onSuccess()
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to create permissions")
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[525px]">
        <DialogHeader>
          <DialogTitle>Create Permissions</DialogTitle>
          <DialogDescription>Add new permissions to the system</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4 max-h-[400px] overflow-y-auto">
            {permissions.map((permission, index) => (
              <div key={index} className="flex gap-2 items-start">
                <div className="flex-1">
                  <Label htmlFor={`permission-${index}`}>Permission {index + 1}</Label>
                  <Input
                    id={`permission-${index}`}
                    placeholder="e.g., user.view, user.edit"
                    value={permission}
                    onChange={(e) => updatePermission(index, e.target.value)}
                    required
                  />
                </div>
                {permissions.length > 1 && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="mt-6"
                    onClick={() => removePermission(index)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                )}
              </div>
            ))}
            <Button type="button" variant="outline" onClick={addPermission} className="w-full">
              <Plus className="h-4 w-4 mr-2" />
              Add Another Permission
            </Button>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? "Creating..." : "Create Permissions"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
