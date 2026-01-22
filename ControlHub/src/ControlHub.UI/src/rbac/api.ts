type CreateRoleInput = {
  name: string
  description: string
  permissionIds: string[]
}

type CreateRolesResponse = {
  message: string
  successCount: number
  failureCount: number
  failedRoles?: string[]
}

type CreatePermissionInput = {
  code: string
  description?: string
}

type PagedResult<T> = {
  items: T[]
  totalCount: number
  pageIndex: number
  pageSize: number
  totalPages: number
}

type ApiPermission = {
  id: string
  code: string
  description: string
}

type ApiRole = {
  id: string
  name: string
  description: string
  isActive?: boolean
  permissions?: ApiPermission[]
}

import { fetchJson, fetchVoid } from "@/services/api/client"



export async function createRoles(roles: CreateRoleInput[], accessToken: string): Promise<CreateRolesResponse> {
  return fetchJson<CreateRolesResponse>("/api/Role/roles", {
    method: "POST",
    body: { roles },
    accessToken
  })
}

export async function createPermissions(permissions: CreatePermissionInput[], accessToken: string): Promise<void> {
  await fetchVoid("/api/Permission/permissions", {
    method: "POST",
    body: { permissions },
    accessToken
  })
}

export async function addPermissionsForRole(roleId: string, permissionIds: string[], accessToken: string) {
  return fetchJson<{ message: string; successCount: number; failureCount: number; failedRoles?: string[] }>(
    `/api/Role/roles/${roleId}/permissions`,
    {
      method: "POST",
      body: { permissionIds },
      accessToken
    }
  )
}

export async function getRoles(
  accessToken: string,
  opts?: { pageIndex?: number; pageSize?: number; searchTerm?: string },
): Promise<PagedResult<ApiRole>> {
  const pageIndex = opts?.pageIndex ?? 1
  const pageSize = opts?.pageSize ?? 1000
  const searchTerm = opts?.searchTerm

  const params = new URLSearchParams({
    pageIndex: String(pageIndex),
    pageSize: String(pageSize),
  })
  if (searchTerm) params.set("searchTerm", searchTerm)

  return fetchJson<PagedResult<ApiRole>>(`/api/Role?${params.toString()}`, {
    accessToken
  })
}

export async function getPermissions(
  accessToken: string,
  opts?: { pageIndex?: number; pageSize?: number; searchTerm?: string },
): Promise<PagedResult<ApiPermission>> {
  const pageIndex = opts?.pageIndex ?? 1
  const pageSize = opts?.pageSize ?? 1000
  const searchTerm = opts?.searchTerm

  const params = new URLSearchParams({
    pageIndex: String(pageIndex),
    pageSize: String(pageSize),
  })
  if (searchTerm) params.set("searchTerm", searchTerm)

  return fetchJson<PagedResult<ApiPermission>>(`/api/Permission?${params.toString()}`, {
    accessToken
  })
}
