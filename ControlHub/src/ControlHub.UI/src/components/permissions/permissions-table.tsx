import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import type { Permission } from "@/services/api"

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  })
}

interface PermissionsTableProps {
  permissions: Permission[]
}

export function PermissionsTable({ permissions }: PermissionsTableProps) {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Description</TableHead>
            <TableHead>Created</TableHead>
            <TableHead>Updated</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {permissions.length === 0 ? (
            <TableRow>
              <TableCell colSpan={4} className="text-center text-muted-foreground">
                No permissions found
              </TableCell>
            </TableRow>
          ) : (
            permissions.map((permission) => (
              <TableRow key={permission.id}>
                <TableCell className="font-medium">
                  <Badge variant="outline">{permission.name}</Badge>
                </TableCell>
                <TableCell>{permission.description || "-"}</TableCell>
                <TableCell className="text-muted-foreground text-sm">
                  {formatDate(permission.createdAt)}
                </TableCell>
                <TableCell className="text-muted-foreground text-sm">
                  {permission.updatedAt ? formatDate(permission.updatedAt) : "-"}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  )
}
