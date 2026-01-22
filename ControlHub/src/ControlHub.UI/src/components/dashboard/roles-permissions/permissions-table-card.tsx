import { Button } from "@/components/ui/button"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { cn } from "@/lib/utils"
import { useTranslation } from "react-i18next"
import type { Permission, PermissionDraft } from "./types"
import { X, Search, Plus } from "lucide-react"

type PermissionsTableCardProps = {
  permissions: Permission[]

  searchTerm: string
  onSearchTermChange: (value: string) => void
  pageIndex: number
  onPageIndexChange: (value: number) => void
  pageSize: number
  onPageSizeChange: (value: number) => void
  totalCount: number
  totalPages: number
  loading: boolean

  permissionDrafts: PermissionDraft[]

  onStartAdd: () => void
  onConfirmAdd: () => void
  onUpdate: () => void
  canConfirm: boolean
  canUpdate: boolean
  saving: boolean

  onDraftChange: (index: number, patch: Partial<PermissionDraft>) => void
  onRemoveDraft: (index: number) => void
}

export function PermissionsTableCard({
  permissions,
  searchTerm,
  onSearchTermChange,
  pageIndex,
  onPageIndexChange,
  pageSize,
  onPageSizeChange,
  totalCount,
  totalPages,
  loading,
  permissionDrafts,
  onStartAdd,
  onConfirmAdd,
  onUpdate,
  canConfirm,
  canUpdate,
  saving,
  onDraftChange,
  onRemoveDraft,
}: PermissionsTableCardProps) {
  const { t } = useTranslation()

  return (
    <div className="bg-sidebar/50 backdrop-blur-sm rounded-xl border border-sidebar-border overflow-hidden shadow-xl">
      <div className="p-6 border-b border-sidebar-border flex flex-col gap-4">
        <div className="flex items-center gap-3">
          <h2 className="text-xl font-bold bg-[var(--vibrant-gradient)] bg-clip-text text-transparent italic">{t('permissions.vault')}</h2>
          <div className="ml-auto flex items-center gap-2">
            <div className="h-2 w-2 rounded-full bg-sidebar-primary animate-pulse" />
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              {loading ? t('permissions.syncing') : `${totalCount} ${t('permissions.secrets')}`}
            </span>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <div className="relative">
            <input
              value={searchTerm}
              onChange={(e) => onSearchTermChange(e.target.value)}
              placeholder={t('permissions.searchPlaceholder')}
              className={cn(
                "h-10 w-64 rounded-xl border border-sidebar-border bg-background/50 px-4 pl-10 text-sm text-foreground",
                "placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-primary/50 transition-all",
              )}
            />
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          </div>

          <select
            value={String(pageSize)}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className={cn(
              "h-10 rounded-xl border border-sidebar-border bg-background/50 px-3 text-sm text-foreground",
              "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-primary/50 transition-all",
            )}
          >
            {[10, 20, 50, 100].map((n) => (
              <option key={n} value={String(n)} className="bg-sidebar">
                {n} {t('table.rowsPerPageSuffix')}
              </option>
            ))}
          </select>

          <div className="flex items-center gap-1 ml-auto">
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="rounded-lg h-10 px-4"
              onClick={() => onPageIndexChange(Math.max(1, pageIndex - 1))}
              disabled={loading || pageIndex <= 1}
            >
              {t('table.previous')}
            </Button>
            <div className="px-4 text-sm font-bold bg-sidebar-accent/50 h-10 flex items-center rounded-lg border border-sidebar-border min-w-[100px] justify-center">
              {pageIndex} <span className="text-muted-foreground mx-1">/</span> {Math.max(1, totalPages)}
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="rounded-lg h-10 px-4"
              onClick={() => onPageIndexChange(Math.min(Math.max(1, totalPages), pageIndex + 1))}
              disabled={loading || pageIndex >= Math.max(1, totalPages)}
            >
              {t('table.next')}
            </Button>
          </div>
        </div>
      </div>

      <div className="overflow-auto scrollbar-none max-h-[calc(100vh-260px)]">
        <Table>
          <TableHeader>
            <TableRow className="border-zinc-800 hover:bg-zinc-900">
              <TableHead className="text-zinc-300 w-12">{t('table.stt')}</TableHead>
              <TableHead className="text-zinc-300 w-20">{t('table.id')}</TableHead>
              <TableHead className="text-zinc-300 w-32">{t('table.permission')}</TableHead>
              <TableHead className="text-zinc-300 w-40">{t('table.description')}</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {permissions.map((permission, index) => (
              <TableRow
                key={permission.id}
                className="border-sidebar-border hover:bg-sidebar-accent/30 transition-colors cursor-grab active:cursor-grabbing group/row"
                draggable
                onDragStart={(e) => {
                  e.dataTransfer.setData("application/rbac-permission-id", permission.id)
                  e.dataTransfer.effectAllowed = "copy"
                }}
                title={t('permissions.dragHint')}
              >
                <TableCell className="text-muted-foreground font-mono text-xs">{(pageIndex - 1) * pageSize + index + 1}</TableCell>
                <TableCell className="text-muted-foreground font-mono text-[10px]">{permission.id}</TableCell>
                <TableCell className="font-bold text-sm">
                  <span className="px-2 py-1 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                    {permission.code}
                  </span>
                </TableCell>
                <TableCell className="text-muted-foreground text-xs max-w-[160px] truncate" title={permission.description}>
                  {permission.description}
                </TableCell>
              </TableRow>
            ))}

            {permissionDrafts.map((draft, draftIndex) => (
              <TableRow key={`draft-${draftIndex}`} className="border-sidebar-border bg-sidebar-primary/5">
                <TableCell className="text-muted-foreground">-</TableCell>
                <TableCell className="text-sidebar-primary font-bold text-[10px] italic">
                  {t('permissions.newDraft')}
                  <button
                    type="button"
                    onClick={() => onRemoveDraft(draftIndex)}
                    className="ml-2 inline-flex items-center justify-center rounded-full p-1 hover:bg-red-500/20 hover:text-red-400 transition-all"
                    aria-label={t('permissions.removeDraft')}
                  >
                    <X className="h-3 w-3" />
                  </button>
                </TableCell>
                <TableCell>
                  <input
                    value={draft.code}
                    onChange={(e) => onDraftChange(draftIndex, { code: e.target.value })}
                    placeholder={t('permissions.codePlaceholder')}
                    className={cn(
                      "h-9 w-full rounded-lg border border-sidebar-border bg-background px-3 text-sm text-foreground",
                      "placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-primary transition-all",
                    )}
                  />
                </TableCell>
                <TableCell>
                  <input
                    value={draft.description}
                    onChange={(e) => onDraftChange(draftIndex, { description: e.target.value })}
                    placeholder={t('permissions.descriptionPlaceholder')}
                    className={cn(
                      "h-9 w-full rounded-lg border border-sidebar-border bg-background px-3 text-sm text-foreground",
                      "placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-primary transition-all",
                    )}
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="p-6 border-t border-sidebar-border bg-sidebar/30 flex items-center gap-3">
        <Button onClick={onStartAdd} variant="outline" className="font-bold border-sidebar-primary/30 text-sidebar-primary hover:bg-sidebar-primary/10">
          <Plus className="h-4 w-4 mr-2" />
          {t('permissions.addPermission')}
        </Button>
        <Button
          onClick={onConfirmAdd}
          disabled={!canConfirm}
          variant="secondary"
          className="font-bold border-sidebar-border"
        >
          {t('permissions.confirmDrafts')}
        </Button>
        <Button
          onClick={onUpdate}
          variant="vibrant"
          disabled={!canUpdate || saving}
          className="ml-auto font-bold px-8"
        >
          {saving ? t('permissions.syncing') : t('permissions.updateAll')}
        </Button>
      </div>
    </div>
  )
}
