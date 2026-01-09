import { RolesPermissionsContent } from "@/components/dashboard/roles-permissions-content"

export function RolesPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Roles & Permissions</h1>
        <p className="text-muted-foreground">Manage roles and permissions together with drag-and-drop assignment.</p>
      </div>

      <RolesPermissionsContent />
    </div>
  )
}
