import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { rolesApi } from "@/services/api"
import { toast } from "sonner"
import { Plus, X } from "lucide-react"

interface CreateRolesDialogProps {
  accessToken: string
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
}

export function CreateRolesDialog({ accessToken, open, onOpenChange, onSuccess }: CreateRolesDialogProps) {
  const [roles, setRoles] = useState<string[]>([""])
  const [isLoading, setIsLoading] = useState(false)

  const addRole = () => {
    setRoles([...roles, ""])
  }

  const removeRole = (index: number) => {
    setRoles(roles.filter((_, i) => i !== index))
  }

  const updateRole = (index: number, value: string) => {
    const updated = [...roles]
    updated[index] = value
    setRoles(updated)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const validRoles = roles.filter((r) => r.trim() !== "")
    if (validRoles.length === 0) {
      toast.error("Please enter at least one role")
      return
    }

    setIsLoading(true)
    try {
      const result = await rolesApi.createRoles({ roles: validRoles }, accessToken)
      
      if (result.failureCount > 0) {
        toast.warning(`${result.message}\nSuccess: ${result.successCount}, Failed: ${result.failureCount}`)
      } else {
        toast.success(result.message)
      }
      
      setRoles([""])
      onSuccess()
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to create roles")
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[525px]">
        <DialogHeader>
          <DialogTitle>Create Roles</DialogTitle>
          <DialogDescription>Add new roles to the system</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4 max-h-[400px] overflow-y-auto">
            {roles.map((role, index) => (
              <div key={index} className="flex gap-2 items-start">
                <div className="flex-1">
                  <Label htmlFor={`role-${index}`}>Role {index + 1}</Label>
                  <Input
                    id={`role-${index}`}
                    placeholder="e.g., Admin, Moderator, User"
                    value={role}
                    onChange={(e) => updateRole(index, e.target.value)}
                    required
                  />
                </div>
                {roles.length > 1 && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="mt-6"
                    onClick={() => removeRole(index)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                )}
              </div>
            ))}
            <Button type="button" variant="outline" onClick={addRole} className="w-full">
              <Plus className="h-4 w-4 mr-2" />
              Add Another Role
            </Button>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? "Creating..." : "Create Roles"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
