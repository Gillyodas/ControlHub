import { RolesPermissionsContent } from "@/components/dashboard/roles-permissions-content"
import { useTranslation } from "react-i18next"

export function RolesPage() {
  const { t } = useTranslation()
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('roles.title')}</h1>
        <p className="text-muted-foreground">{t('roles.description')}</p>
      </div>

      <RolesPermissionsContent />
    </div>
  )
}
