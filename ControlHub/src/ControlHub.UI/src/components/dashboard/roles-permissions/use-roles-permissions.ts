import * as React from "react"
import { initialPermissions, initialRoles } from "./data"
import type { Permission, PermissionDraft, Role, RoleDraft } from "./types"
import { useTranslation } from "react-i18next"

type NotifyFn = (input: { title: string; description?: string; variant?: "success" | "error" | "info" }) => void

type UseRolesPermissionsOptions = {
  notify: NotifyFn
  accessToken?: string
}

export function useRolesPermissions({ notify, accessToken }: UseRolesPermissionsOptions) {
  const { t } = useTranslation()
  const [roles, setRoles] = React.useState<Role[]>(() => initialRoles)
  const [permissions, setPermissions] = React.useState<Permission[]>(() => initialPermissions)

  const [rolesSearchTerm, setRolesSearchTerm] = React.useState("")
  const [rolesPageIndex, setRolesPageIndex] = React.useState(1)
  const [rolesPageSize, setRolesPageSize] = React.useState(10)
  const [rolesTotalCount, setRolesTotalCount] = React.useState(0)
  const [rolesTotalPages, setRolesTotalPages] = React.useState(1)
  const [loadingRoles, setLoadingRoles] = React.useState(false)
  const [rolesReloadToken, bumpRolesReloadToken] = React.useReducer((x: number) => x + 1, 0)

  const [permissionsSearchTerm, setPermissionsSearchTerm] = React.useState("")
  const [permissionsPageIndex, setPermissionsPageIndex] = React.useState(1)
  const [permissionsPageSize, setPermissionsPageSize] = React.useState(10)
  const [permissionsTotalCount, setPermissionsTotalCount] = React.useState(0)
  const [permissionsTotalPages, setPermissionsTotalPages] = React.useState(1)
  const [loadingPermissions, setLoadingPermissions] = React.useState(false)
  const [permissionsReloadToken, bumpPermissionsReloadToken] = React.useReducer((x: number) => x + 1, 0)

  const [roleDrafts, setRoleDrafts] = React.useState<RoleDraft[]>([])
  const [permissionDrafts, setPermissionDrafts] = React.useState<PermissionDraft[]>([])

  const [rolesDirty, setRolesDirty] = React.useState(false)
  const [permissionsDirty, setPermissionsDirty] = React.useState(false)

  const [savingRoles, setSavingRoles] = React.useState(false)
  const [savingPermissions, setSavingPermissions] = React.useState(false)

  const rolePermissionsSnapshotRef = React.useRef<Map<string, string[]>>(new Map())

  const [debouncedRolesSearchTerm, setDebouncedRolesSearchTerm] = React.useState(rolesSearchTerm)
  const [debouncedPermissionsSearchTerm, setDebouncedPermissionsSearchTerm] = React.useState(permissionsSearchTerm)

  React.useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedRolesSearchTerm(rolesSearchTerm), 300)
    return () => window.clearTimeout(timer)
  }, [rolesSearchTerm])

  React.useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedPermissionsSearchTerm(permissionsSearchTerm), 300)
    return () => window.clearTimeout(timer)
  }, [permissionsSearchTerm])

  React.useEffect(() => {
    if (!accessToken) return

    let cancelled = false
    setLoadingRoles(true)

      ; (async () => {
        try {
          const mod = await import("@/rbac/api")
          const res = await mod.getRoles(accessToken, {
            pageIndex: rolesPageIndex,
            pageSize: rolesPageSize,
            searchTerm: debouncedRolesSearchTerm.trim() ? debouncedRolesSearchTerm.trim() : undefined,
          })

          if (cancelled) return

          const nextRoles: Role[] = (res.items ?? []).map((r) => ({
            id: r.id,
            name: r.name,
            description: r.description,
            permissionIds: (r.permissions ?? []).map((p) => p.id),
          }))

          setRoles(nextRoles)
          setRolesTotalCount(res.totalCount ?? 0)
          setRolesTotalPages(res.totalPages ?? 1)
          setLoadingRoles(false)

          const map = new Map<string, string[]>()
          for (const r of nextRoles) {
            map.set(r.id, [...r.permissionIds])
          }
          rolePermissionsSnapshotRef.current = map
        } catch (e) {
          if (cancelled) return
          setLoadingRoles(false)
          notify({
            title: t('roles.loadFailed'),
            description: e instanceof Error ? e.message : String(e),
            variant: "error",
          })
        }
      })()

    return () => {
      cancelled = true
    }
  }, [
    accessToken,
    debouncedRolesSearchTerm,
    rolesPageIndex,
    rolesPageSize,
    rolesReloadToken,
    t,
    notify
  ])

  React.useEffect(() => {
    if (!accessToken) return

    let cancelled = false
    setLoadingPermissions(true)

      ; (async () => {
        try {
          const mod = await import("@/rbac/api")
          const res = await mod.getPermissions(accessToken, {
            pageIndex: permissionsPageIndex,
            pageSize: permissionsPageSize,
            searchTerm: debouncedPermissionsSearchTerm.trim() ? debouncedPermissionsSearchTerm.trim() : undefined,
          })

          if (cancelled) return

          const nextPermissions: Permission[] = (res.items ?? []).map((p) => ({
            id: p.id,
            code: p.code,
            description: p.description,
          }))

          setPermissions(nextPermissions)
          setPermissionsTotalCount(res.totalCount ?? 0)
          setPermissionsTotalPages(res.totalPages ?? 1)
          setLoadingPermissions(false)
        } catch (e) {
          if (cancelled) return
          setLoadingPermissions(false)
          notify({
            title: t('permissions.loadFailed'),
            description: e instanceof Error ? e.message : String(e),
            variant: "error",
          })
        }
      })()

    return () => {
      cancelled = true
    }
  }, [
    accessToken,
    debouncedPermissionsSearchTerm,
    permissionsPageIndex,
    permissionsPageSize,
    permissionsReloadToken,
    t,
    notify
  ])

  React.useEffect(() => {
    if (rolePermissionsSnapshotRef.current.size) return

    const map = new Map<string, string[]>()
    for (const r of roles) {
      map.set(r.id, [...r.permissionIds])
    }
    rolePermissionsSnapshotRef.current = map
  }, [roles])

  React.useEffect(() => {
    setPermissionsDirty(permissionDrafts.length > 0)
  }, [permissionDrafts.length])

  const startAddRole = React.useCallback(() => {
    setRoleDrafts((prev) => [...prev, { name: "", description: "", permissionIds: [] }])
  }, [])

  const startAddPermission = React.useCallback(() => {
    setPermissionDrafts((prev) => [...prev, { code: "", description: "" }])
  }, [])

  const updateRoleDraft = React.useCallback((index: number, patch: Partial<RoleDraft>) => {
    setRoleDrafts((prev) => prev.map((d, i) => (i === index ? { ...d, ...patch } : d)))
  }, [])

  const updatePermissionDraft = React.useCallback((index: number, patch: Partial<PermissionDraft>) => {
    setPermissionDrafts((prev) => prev.map((d, i) => (i === index ? { ...d, ...patch } : d)))
  }, [])

  const removeRoleDraft = React.useCallback((index: number) => {
    setRoleDrafts((prev) => prev.filter((_, i) => i !== index))
  }, [])

  const removePermissionDraft = React.useCallback((index: number) => {
    setPermissionDrafts((prev) => prev.filter((_, i) => i !== index))
  }, [])

  const addPermissionToRole = React.useCallback((roleId: string, permissionId: string) => {
    setRoles((prev) => {
      const next = prev.map((r) =>
        r.id === roleId
          ? r.permissionIds.includes(permissionId)
            ? r
            : { ...r, permissionIds: [...r.permissionIds, permissionId] }
          : r,
      )
      return next
    })
    setRolesDirty(true)
  }, [])

  const removePermissionFromRole = React.useCallback((roleId: string, permissionId: string) => {
    setRoles((prev) => prev.map((r) => (r.id === roleId ? { ...r, permissionIds: r.permissionIds.filter((p) => p !== permissionId) } : r)))
    setRolesDirty(true)
  }, [])

  const addPermissionToRoleDraft = React.useCallback((draftIndex: number, permissionId: string) => {
    setRoleDrafts((prev) =>
      prev.map((d, i) => {
        if (i !== draftIndex) return d
        if (d.permissionIds.includes(permissionId)) return d
        return { ...d, permissionIds: [...d.permissionIds, permissionId] }
      }),
    )
  }, [])

  const removePermissionFromRoleDraft = React.useCallback((draftIndex: number, permissionId: string) => {
    setRoleDrafts((prev) =>
      prev.map((d, i) => (i === draftIndex ? { ...d, permissionIds: d.permissionIds.filter((p) => p !== permissionId) } : d)),
    )
  }, [])

  const confirmAddRoles = React.useCallback(async () => {
    if (!roleDrafts.length) return

    const normalized = roleDrafts.map((d) => ({
      name: d.name.trim(),
      description: d.description.trim(),
      permissionIds: d.permissionIds,
    }))

    const missing = normalized.find((d) => !d.name || !d.description || !d.permissionIds.length)
    if (missing) {
      notify({
        title: t('roles.invalidRole'),
        description: t('roles.roleRequiredFields'),
        variant: "error"
      })
      return
    }

    if (!accessToken) {
      notify({
        title: t('roles.notAuthenticated'),
        description: t('roles.pleaseLogin'),
        variant: "error"
      })
      return
    }

    setSavingRoles(true)
    try {
      const mod = await import("@/rbac/api")
      const res = await mod.createRoles(normalized, accessToken)
      notify({
        title: res.message || t('roles.created'),
        description: `Success: ${res.successCount}. Failed: ${res.failureCount}.`,
        variant: res.failureCount ? "info" : "success",
      })
      setRoleDrafts([])
      bumpRolesReloadToken()
    } catch (e) {
      notify({
        title: t('roles.createFailed'),
        description: e instanceof Error ? e.message : String(e),
        variant: "error"
      })
    } finally {
      setSavingRoles(false)
    }
  }, [accessToken, notify, roleDrafts, t])

  const confirmAddPermissions = React.useCallback(async () => {
    if (!permissionDrafts.length) return

    const normalized = permissionDrafts.map((d) => ({
      code: d.code.trim(),
      description: d.description.trim(),
    }))

    const missing = normalized.find((d) => !d.code)
    if (missing) {
      notify({ title: t('permissions.codeRequired'), variant: "error" })
      return
    }

    if (!accessToken) {
      notify({
        title: t('roles.notAuthenticated'),
        description: t('roles.pleaseLogin'),
        variant: "error"
      })
      return
    }

    setSavingPermissions(true)
    try {
      const mod = await import("@/rbac/api")
      await mod.createPermissions(normalized, accessToken)
      notify({ title: t('permissions.created'), description: `Created: ${normalized.length}`, variant: "success" })
      setPermissionDrafts([])
      setPermissionsDirty(false)
      bumpPermissionsReloadToken()
    } catch (e) {
      notify({
        title: t('permissions.createFailed'),
        description: e instanceof Error ? e.message : String(e),
        variant: "error"
      })
    } finally {
      setSavingPermissions(false)
    }
  }, [accessToken, notify, permissionDrafts, t])

  const updateRoles = React.useCallback(async () => {
    setSavingRoles(true)
    try {
      if (!accessToken) {
        throw new Error(t('roles.notAuthenticated'))
      }

      const snapshot = rolePermissionsSnapshotRef.current
      const updates = roles
        .map((r) => {
          const prev = new Set(snapshot.get(r.id) ?? [])
          const curr = new Set(r.permissionIds)
          const added = r.permissionIds.filter((p) => !prev.has(p))
          const removed = [...prev].filter((p) => !curr.has(p))
          return { roleId: r.id, added, removed }
        })
        .filter((u) => u.added.length || u.removed.length)

      if (!updates.length) {
        setRolesDirty(false)
        notify({ title: t('roles.noChanges'), variant: "info" })
        return
      }

      const hasRemovals = updates.some((u) => u.removed.length)
      if (hasRemovals) {
        notify({
          title: t('roles.removeNotSupported'),
          description: t('roles.removeNotSupportedDesc'),
          variant: "info"
        })
      }

      const mod = await import("@/rbac/api")
      const toApply = updates.filter((u) => u.added.length)

      await Promise.all(toApply.map((u) => mod.addPermissionsForRole(u.roleId, u.added, accessToken)))

      for (const u of toApply) {
        const prev = snapshot.get(u.roleId) ?? []
        snapshot.set(u.roleId, [...new Set([...prev, ...u.added])])
      }

      setRolesDirty(false)
      notify({
        title: t('roles.updated'),
        description: t('roles.permissionsAdded'),
        variant: "success"
      })
    } catch (e) {
      notify({
        title: t('roles.updateFailed'),
        description: e instanceof Error ? e.message : t('roles.apiError'),
        variant: "error"
      })
    } finally {
      setSavingRoles(false)
    }
  }, [accessToken, notify, roles, t])

  const updatePermissions = React.useCallback(async () => {
    await confirmAddPermissions()
  }, [confirmAddPermissions])

  const deleteRole = React.useCallback(async (id: string) => {
    if (!accessToken) return
    if (!confirm(t('roles.confirmDelete'))) return

    try {
      const mod = await import("@/rbac/api")
      await mod.deleteRole(id, accessToken)
      notify({ title: t('roles.deleted'), variant: "success" })
      bumpRolesReloadToken()
    } catch (e) {
      notify({
        title: t('roles.deleteFailed'),
        description: e instanceof Error ? e.message : String(e),
        variant: "error"
      })
    }
  }, [accessToken, notify, t])

  const deletePermission = React.useCallback(async (id: string) => {
    if (!accessToken) return
    if (!confirm(t('permissions.confirmDelete'))) return

    try {
      const mod = await import("@/rbac/api")
      await mod.deletePermission(id, accessToken)
      notify({ title: t('permissions.deleted'), variant: "success" })
      bumpPermissionsReloadToken()
    } catch (e) {
      notify({
        title: t('permissions.deleteFailed'),
        description: e instanceof Error ? e.message : String(e),
        variant: "error"
      })
    }
  }, [accessToken, notify, t])

  const editRole = React.useCallback(async (id: string, data: { name: string; description: string }) => {
    if (!accessToken) return

    try {
      const mod = await import("@/rbac/api")
      await mod.updateRole(id, data, accessToken)
      notify({ title: t('roles.updated'), variant: "success" })
      bumpRolesReloadToken()
    } catch (e) {
      notify({
        title: t('roles.updateFailed'),
        description: e instanceof Error ? e.message : String(e),
        variant: "error"
      })
    }
  }, [accessToken, notify, t])

  const editPermission = React.useCallback(async (id: string, data: { code: string; description: string }) => {
    if (!accessToken) return

    try {
      const mod = await import("@/rbac/api")
      await mod.updatePermission(id, data, accessToken)
      notify({ title: t('permissions.updated'), variant: "success" })
      bumpPermissionsReloadToken()
    } catch (e) {
      notify({
        title: t('permissions.updateFailed'),
        description: e instanceof Error ? e.message : String(e),
        variant: "error"
      })
    }
  }, [accessToken, notify, t])

  const canConfirmRole = Boolean(
    roleDrafts.length && roleDrafts.every((d) => d.name.trim() && d.description.trim() && d.permissionIds.length),
  )
  const canConfirmPermission = Boolean(permissionDrafts.length && permissionDrafts.every((d) => d.code.trim()))

  return {
    roles,
    permissions,

    rolesSearchTerm,
    setRolesSearchTerm: (v: string) => {
      if (rolesDirty) {
        notify({
          title: t('roles.unsavedChanges'),
          description: t('roles.unsavedChangesSearchDesc'),
          variant: "info"
        })
        return
      }
      setRolesSearchTerm(v)
      setRolesPageIndex(1)
    },
    rolesPageIndex,
    rolesPageSize,
    rolesTotalCount,
    rolesTotalPages,
    loadingRoles,
    setRolesPageIndex: (v: number) => {
      if (rolesDirty) {
        notify({
          title: t('roles.unsavedChanges'),
          description: t('roles.unsavedChangesPageDesc'),
          variant: "info"
        })
        return
      }
      setRolesPageIndex(v)
    },
    setRolesPageSize: (v: number) => {
      if (rolesDirty) {
        notify({
          title: t('roles.unsavedChanges'),
          description: t('roles.unsavedChangesPageDesc'),
          variant: "info"
        })
        return
      }
      setRolesPageSize(v)
      setRolesPageIndex(1)
    },

    permissionsSearchTerm,
    setPermissionsSearchTerm: (v: string) => {
      if (permissionsDirty) {
        notify({
          title: t('roles.unsavedChanges'),
          description: t('permissions.unsavedChangesSearchDesc'),
          variant: "info"
        })
        return
      }
      setPermissionsSearchTerm(v)
      setPermissionsPageIndex(1)
    },
    permissionsPageIndex,
    permissionsPageSize,
    permissionsTotalCount,
    permissionsTotalPages,
    loadingPermissions,
    setPermissionsPageIndex: (v: number) => {
      if (permissionsDirty) {
        notify({
          title: t('roles.unsavedChanges'),
          description: t('permissions.unsavedChangesPageDesc'),
          variant: "info"
        })
        return
      }
      setPermissionsPageIndex(v)
    },
    setPermissionsPageSize: (v: number) => {
      if (permissionsDirty) {
        notify({
          title: t('roles.unsavedChanges'),
          description: t('permissions.unsavedChangesPageDesc'),
          variant: "info"
        })
        return
      }
      setPermissionsPageSize(v)
      setPermissionsPageIndex(1)
    },

    roleDrafts,
    permissionDrafts,

    rolesDirty,
    permissionsDirty,

    savingRoles,
    savingPermissions,

    startAddRole,
    startAddPermission,

    updateRoleDraft,
    updatePermissionDraft,

    removeRoleDraft,
    removePermissionDraft,

    addPermissionToRole,
    removePermissionFromRole,

    addPermissionToRoleDraft,
    removePermissionFromRoleDraft,

    confirmAddRoles,
    confirmAddPermissions,

    updateRoles,
    updatePermissions,

    canConfirmRole,
    canConfirmPermission,

    deleteRole,
    deletePermission,
    editRole,
    editPermission,
  }
}
