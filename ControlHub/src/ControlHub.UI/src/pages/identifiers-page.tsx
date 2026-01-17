import React, { useState, useEffect, useCallback, useMemo } from "react"
import { Plus, Edit, Eye, EyeOff, Power, Save, X, Fingerprint, Loader2 } from "lucide-react"
import { cn } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  getActiveIdentifierConfigs,
  createIdentifierConfig,
  toggleIdentifierActive,
  updateIdentifierConfig,
  ValidationRuleType,
  type IdentifierConfigDto,
  type CreateIdentifierConfigCommand,
  type ValidationRuleDto
} from "@/services/api/identifiers"
import { useAuth } from "@/auth/use-auth"
import { useTranslation } from "react-i18next"

export default function IdentifiersPage() {
  const { t } = useTranslation()
  const { auth } = useAuth()
  const [configs, setConfigs] = useState<IdentifierConfigDto[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [includeDeactivated, setIncludeDeactivated] = useState(false)

  const validationRuleTypes = useMemo(() => [
    { value: ValidationRuleType.Required, label: t('identifiers.validationRuleTypes.required') },
    { value: ValidationRuleType.MinLength, label: t('identifiers.validationRuleTypes.minLength') },
    { value: ValidationRuleType.MaxLength, label: t('identifiers.validationRuleTypes.maxLength') },
    { value: ValidationRuleType.Pattern, label: t('identifiers.validationRuleTypes.pattern') },
    { value: ValidationRuleType.Email, label: t('identifiers.validationRuleTypes.email') },
    { value: ValidationRuleType.Phone, label: t('identifiers.validationRuleTypes.phone') },
    { value: ValidationRuleType.Range, label: t('identifiers.validationRuleTypes.range') },
    { value: ValidationRuleType.Custom, label: t('identifiers.validationRuleTypes.custom') },
  ], [t])

  const loadConfigs = useCallback(async () => {
    try {
      setLoading(true)
      const data = await getActiveIdentifierConfigs(includeDeactivated)
      setConfigs(data)
      setError(null)
    } catch (err) {
      setError(t('identifiers.failedToLoad'))
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [includeDeactivated, t])

  useEffect(() => {
    loadConfigs()
  }, [loadConfigs])

  const handleCreateConfig = async (data: CreateIdentifierConfigCommand) => {
    try {
      setError(null)
      await createIdentifierConfig(data, auth!.accessToken)
      setShowCreateModal(false)
      loadConfigs()
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : t('identifiers.failedToCreate')
      setError(errorMessage)
      throw err
    }
  }

  return (
    <div className="space-y-8 animate-in fade-in duration-500">
      <div className="flex justify-between items-end">
        <div>
          <h1 className="text-4xl font-extrabold tracking-tight bg-[var(--vibrant-gradient)] bg-clip-text text-transparent italic">
            {t('identifiers.title')}
          </h1>
          <p className="text-muted-foreground mt-2 text-lg">
            {t('identifiers.description')}
          </p>
        </div>
        <div className="flex items-center gap-4">
          <label className="flex items-center gap-2 text-sm text-muted-foreground cursor-pointer bg-sidebar-accent/50 px-4 h-12 rounded-xl border border-sidebar-border hover:bg-sidebar-accent transition-colors">
            <input
              type="checkbox"
              checked={includeDeactivated}
              onChange={(e) => setIncludeDeactivated(e.target.checked)}
              className="w-4 h-4 rounded border-sidebar-border text-sidebar-primary focus:ring-sidebar-primary bg-background"
            />
            {t('identifiers.showOffline')}
          </label>
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-2 bg-[var(--vibrant-gradient)] text-white px-6 h-12 rounded-xl font-bold shadow-xl shadow-sidebar-primary/20 hover:opacity-90 transition-all active:scale-95"
          >
            <Plus className="w-5 h-5" />
            {t('identifiers.newProtocol')}
          </button>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-destructive/10 border border-destructive/20 rounded-xl text-destructive text-sm font-medium animate-in slide-in-from-top-2">
          {error}
        </div>
      )}

      {loading ? (
        <div className="flex flex-col items-center justify-center py-20 gap-4">
          <div className="h-10 w-10 border-4 border-sidebar-primary/20 border-t-sidebar-primary rounded-full animate-spin" />
          <p className="text-muted-foreground font-bold tracking-widest text-xs uppercase animate-pulse">{t('identifiers.syncingMatrix')}</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {configs.map((config) => (
            <IdentifierConfigCard
              key={config.id}
              config={config}
              onUpdate={loadConfigs}
              validationRuleTypes={validationRuleTypes}
            />
          ))}
          {configs.length === 0 && (
            <div className="col-span-full py-20 text-center border-2 border-dashed border-sidebar-border rounded-2xl bg-sidebar/20 backdrop-blur-sm">
              <h3 className="text-xl font-bold text-muted-foreground italic">{t('identifiers.noProtocols')}</h3>
              <p className="text-muted-foreground/60 mt-1">{t('identifiers.noProtocolsDescription')}</p>
            </div>
          )}
        </div>
      )}

      {showCreateModal && (
        <CreateConfigModal
          onClose={() => setShowCreateModal(false)}
          onSubmit={handleCreateConfig}
          validationRuleTypes={validationRuleTypes}
        />
      )}
    </div>
  )
}

function IdentifierConfigCard({ config, onUpdate, validationRuleTypes }: {
  config: IdentifierConfigDto
  onUpdate: () => void
  validationRuleTypes: { value: number; label: string }[]
}) {
  const { t } = useTranslation()
  const { auth } = useAuth()
  const [expanded, setExpanded] = useState(false)
  const [showEditModal, setShowEditModal] = useState(false)
  const [isActive, setIsActive] = useState(config.isActive)
  const [isToggling, setIsToggling] = useState(false)

  const handleToggleActive = async () => {
    try {
      setIsToggling(true)
      const newActiveState = !isActive
      await toggleIdentifierActive(config.id, newActiveState, auth!.accessToken)
      setIsActive(newActiveState)
      onUpdate()
    } catch (error) {
      console.error("Failed to toggle active state:", error)
    } finally {
      setIsToggling(false)
    }
  }

  const handleUpdateSuccess = () => {
    setShowEditModal(false)
    onUpdate()
  }

  return (
    <>
      <div className={cn(
        "group relative bg-sidebar/40 backdrop-blur-md rounded-2xl border transition-all duration-300 hover:shadow-2xl hover:shadow-sidebar-primary/10",
        config.isActive
          ? "border-sidebar-border"
          : "border-sidebar-border/50 opacity-60 grayscale-[0.5]"
      )}>
        {isActive && (
          <div className="absolute inset-0 bg-gradient-to-br from-sidebar-primary/5 to-transparent rounded-2xl pointer-events-none" />
        )}

        <div className="p-6">
          <div className="flex justify-between items-start">
            <div className="flex-1">
              <div className="flex items-center gap-2">
                <h3 className={cn(
                  "text-xl font-bold tracking-tight transition-colors",
                  config.isActive ? "text-foreground" : "text-muted-foreground"
                )}>
                  {config.name}
                </h3>
                {config.isActive ? (
                  <Badge variant="success" className="h-5 px-1.5 font-bold uppercase text-[9px]">{t('identifiers.live')}</Badge>
                ) : (
                  <Badge variant="outline" className="h-5 px-1.5 font-bold uppercase text-[9px]">{t('identifiers.offline')}</Badge>
                )}
              </div>
              <p className="text-muted-foreground text-sm mt-1.5 leading-relaxed line-clamp-2">
                {config.description || t('identifiers.noProtocolsDescription')}
              </p>

              <div className="flex items-center gap-4 mt-4">
                <div className="flex items-center gap-1.5">
                  <Fingerprint className="w-3.5 h-3.5 text-sidebar-primary" />
                  <span className="text-muted-foreground font-bold text-[10px] uppercase tracking-wider">
                    {config.rules.length} {config.rules.length !== 1 ? t('identifiers.logicGates') : t('identifiers.logicGate')}
                  </span>
                </div>
              </div>
            </div>

            <div className="flex flex-col gap-2">
              <button
                onClick={handleToggleActive}
                disabled={isToggling}
                className={cn(
                  "p-2.5 rounded-xl transition-all duration-300 border shadow-sm",
                  isActive
                    ? "bg-emerald-500/10 text-emerald-400 border-emerald-500/20 hover:bg-emerald-500/20"
                    : "bg-sidebar-accent/50 text-muted-foreground border-sidebar-border hover:bg-sidebar-accent"
                )}
              >
                {isToggling ? (
                  <Loader2 className="w-5 h-5 animate-spin" />
                ) : (
                  <Power className="w-5 h-5" />
                )}
              </button>
            </div>
          </div>

          <div className="flex items-center gap-2 mt-6 pt-6 border-t border-sidebar-border/30">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setExpanded(!expanded)}
              className="flex-1 rounded-lg font-bold text-xs h-9 border-sidebar-border"
            >
              {expanded ? (
                <><EyeOff className="w-3.5 h-3.5 mr-2" /> {t('identifiers.hideMatrix')}</>
              ) : (
                <><Eye className="w-3.5 h-3.5 mr-2" /> {t('identifiers.viewMatrix')}</>
              )}
            </Button>
            <Button
              variant="secondary"
              size="icon"
              onClick={() => setShowEditModal(true)}
              className="rounded-lg h-9 w-9 border border-sidebar-border"
            >
              <Edit className="w-3.5 h-3.5" />
            </Button>
          </div>

          {expanded && (
            <div className="mt-4 space-y-2 animate-in slide-in-from-top-2 duration-300">
              {config.rules.map((rule, index) => (
                <div key={index} className="flex items-center justify-between p-3 bg-sidebar-accent/50 rounded-xl border border-sidebar-border/50">
                  <div className="flex-1">
                    <div className="font-bold text-[11px] uppercase tracking-wider text-foreground">
                      {validationRuleTypes.find(t => t.value === rule.type)?.label || rule.type}
                    </div>
                    <div className="text-[11px] text-muted-foreground mt-0.5 italic">
                      {rule.errorMessage || t('identifiers.standardValidation')}
                    </div>
                  </div>
                  <div className="text-[10px] font-mono text-sidebar-primary/60 bg-sidebar-primary/5 px-2 py-0.5 rounded-full border border-sidebar-primary/10">
                    O#{rule.order}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {showEditModal && (
        <EditConfigModal
          config={config}
          onClose={() => setShowEditModal(false)}
          onSubmit={handleUpdateSuccess}
          validationRuleTypes={validationRuleTypes}
        />
      )}
    </>
  )
}

function CreateConfigModal({ onClose, onSubmit, validationRuleTypes }: {
  onClose: () => void
  onSubmit: (data: CreateIdentifierConfigCommand) => Promise<void>
  validationRuleTypes: { value: number; label: string }[]
}) {
  const { t } = useTranslation()
  const [formData, setFormData] = useState({
    name: "",
    description: "",
  })
  const [rules, setRules] = useState<ValidationRuleDto[]>([
    {
      type: ValidationRuleType.Required,
      parameters: {},
      order: 0,
    }
  ])
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!formData.name || !formData.description) {
      setError(t('identifiers.requiredFields'))
      return
    }

    if (rules.length === 0) {
      setError(t('identifiers.addAtLeastOneRule'))
      return
    }

    try {
      setSubmitting(true)
      setError(null)
      await onSubmit({
        ...formData,
        rules
      })
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : t('identifiers.failedToCreate')
      setError(errorMessage)
    } finally {
      setSubmitting(false)
    }
  }

  const addRule = () => {
    const newRule: ValidationRuleDto = {
      type: ValidationRuleType.Required,
      parameters: {},
      order: rules.length,
    }
    setRules([...rules, newRule])
  }

  const updateRule = (index: number, rule: ValidationRuleDto) => {
    const updatedRules = [...rules]
    updatedRules[index] = rule
    setRules(updatedRules)
  }

  const removeRule = (index: number) => {
    if (rules.length > 1) {
      const updatedRules = rules.filter((_, i) => i !== index)
      setRules(updatedRules.map((rule, i) => ({ ...rule, order: i })))
    }
  }

  return (
    <div className="fixed inset-0 bg-background/80 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-in fade-in duration-300">
      <div className="bg-sidebar/95 backdrop-blur-xl rounded-3xl max-w-2xl w-full max-h-[90vh] overflow-hidden border border-sidebar-border shadow-2xl flex flex-col">
        <div className="p-8 border-b border-sidebar-border/50 bg-gradient-to-br from-sidebar-primary/10 to-transparent">
          <h2 className="text-2xl font-black tracking-tight text-foreground bg-[var(--vibrant-gradient)] bg-clip-text text-transparent italic">
            {t('identifiers.initializeNewProtocol')}
          </h2>
          <p className="text-muted-foreground text-sm mt-1">{t('identifiers.protocolBriefing')}</p>
        </div>

        <div className="p-8 overflow-y-auto custom-scrollbar flex-1">
          {error && (
            <div className="mb-6 p-4 bg-destructive/10 border border-destructive/20 rounded-xl text-destructive text-sm font-bold flex items-center gap-2">
              <span className="h-2 w-2 bg-destructive rounded-full animate-pulse" />
              {error}
            </div>
          )}

          <form id="create-config-form" onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <label className="text-xs font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.protocolName')}</label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="w-full px-4 py-3 bg-sidebar-accent/50 border border-sidebar-border rounded-xl text-foreground placeholder:text-muted-foreground/30 focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 focus:border-sidebar-primary transition-all font-medium"
                placeholder={t('identifiers.protocolNamePlaceholder')}
              />
            </div>

            <div className="space-y-2">
              <label className="text-xs font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.briefingDescription')}</label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="w-full px-4 py-3 bg-sidebar-accent/50 border border-sidebar-border rounded-xl text-foreground placeholder:text-muted-foreground/30 focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 focus:border-sidebar-primary transition-all font-medium"
                rows={3}
                placeholder={t('identifiers.briefingPlaceholder')}
              />
            </div>

            <div className="pt-4">
              <div className="flex justify-between items-center mb-4">
                <label className="text-xs font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.logicMatrixRules')}</label>
                <Button
                  type="button"
                  onClick={addRule}
                  variant="outline"
                  size="sm"
                  className="rounded-lg h-8 px-3 border-sidebar-border font-bold text-[10px] uppercase"
                >
                  <Plus className="w-3 h-3 mr-1.5" /> {t('identifiers.appendGate')}
                </Button>
              </div>
              <div className="space-y-4">
                {rules.map((rule, index) => (
                  <ValidationRuleEditor
                    key={index}
                    rule={rule}
                    onChange={(updatedRule) => updateRule(index, updatedRule)}
                    onRemove={() => removeRule(index)}
                    canRemove={rules.length > 1}
                    validationRuleTypes={validationRuleTypes}
                  />
                ))}
              </div>
            </div>
          </form>
        </div>

        <div className="p-8 bg-sidebar-accent/20 border-t border-sidebar-border/50 flex gap-4">
          <Button
            type="button"
            onClick={onClose}
            disabled={submitting}
            variant="ghost"
            className="flex-1 rounded-xl h-12 font-bold text-muted-foreground hover:bg-sidebar-accent"
          >
            {t('identifiers.abort')}
          </Button>
          <Button
            type="submit"
            form="create-config-form"
            disabled={submitting}
            variant="vibrant"
            className="flex-[2] rounded-xl h-12 font-black uppercase tracking-widest shadow-xl shadow-sidebar-primary/20"
          >
            {submitting ? (
              <><Loader2 className="w-4 h-4 mr-2 animate-spin" /> {t('identifiers.sequencing')}</>
            ) : (
              t('identifiers.deployProtocol')
            )}
          </Button>
        </div>
      </div>
    </div>
  )
}

function EditConfigModal({ config, onClose, onSubmit, validationRuleTypes }: {
  config: IdentifierConfigDto
  onClose: () => void
  onSubmit: () => void
  validationRuleTypes: { value: number; label: string }[]
}) {
  const { t } = useTranslation()
  const { auth } = useAuth()
  const [formData, setFormData] = useState({
    name: config.name,
    description: config.description,
  })
  const [rules, setRules] = useState<ValidationRuleDto[]>(config.rules)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [hasChanges, setHasChanges] = useState(false)

  useEffect(() => {
    const nameChanged = formData.name !== config.name
    const descChanged = formData.description !== config.description
    const rulesChanged = JSON.stringify(rules) !== JSON.stringify(config.rules)
    setHasChanges(nameChanged || descChanged || rulesChanged)
  }, [formData, rules, config])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!formData.name || !formData.description) {
      setError(t('identifiers.requiredFields'))
      return
    }

    if (rules.length === 0) {
      setError(t('identifiers.addAtLeastOneRule'))
      return
    }

    try {
      setSubmitting(true)
      setError(null)
      await updateIdentifierConfig(config.id, {
        ...formData,
        rules
      }, auth!.accessToken)
      onSubmit()
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : t('identifiers.failedToUpdate')
      setError(errorMessage)
    } finally {
      setSubmitting(false)
    }
  }

  const addRule = () => {
    const newRule: ValidationRuleDto = {
      type: ValidationRuleType.Required,
      parameters: {},
      order: rules.length,
    }
    setRules([...rules, newRule])
  }

  const updateRule = (index: number, rule: ValidationRuleDto) => {
    const updatedRules = [...rules]
    updatedRules[index] = rule
    setRules(updatedRules)
  }

  const removeRule = (index: number) => {
    if (rules.length > 1) {
      const updatedRules = rules.filter((_, i) => i !== index)
      setRules(updatedRules.map((rule, i) => ({ ...rule, order: i })))
    }
  }

  return (
    <div className="fixed inset-0 bg-background/80 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-in fade-in duration-300">
      <div className="bg-sidebar/95 backdrop-blur-xl rounded-3xl max-w-2xl w-full max-h-[90vh] overflow-hidden border border-sidebar-border shadow-2xl flex flex-col">
        <div className="p-8 border-b border-sidebar-border/50 bg-gradient-to-br from-sidebar-primary/10 to-transparent flex justify-between items-center">
          <div>
            <h2 className="text-2xl font-black tracking-tight text-foreground bg-[var(--vibrant-gradient)] bg-clip-text text-transparent italic">
              {t('identifiers.recodeProtocol')}
            </h2>
            <p className="text-muted-foreground text-sm mt-1">{t('identifiers.modifyLogicGates')}</p>
          </div>
          {hasChanges && (
            <Badge variant="warning" className="animate-pulse bg-amber-500/20 text-amber-400 border-amber-500/30">
              {t('identifiers.unsyncedChanges')}
            </Badge>
          )}
        </div>

        <div className="p-8 overflow-y-auto custom-scrollbar flex-1">
          {error && (
            <div className="mb-6 p-4 bg-destructive/10 border border-destructive/20 rounded-xl text-destructive text-sm font-bold flex items-center gap-2">
              <span className="h-2 w-2 bg-destructive rounded-full animate-pulse" />
              {error}
            </div>
          )}

          <form id="edit-config-form" onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <label className="text-xs font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.protocolName')}</label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="w-full px-4 py-3 bg-sidebar-accent/50 border border-sidebar-border rounded-xl text-foreground placeholder:text-muted-foreground/30 focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 focus:border-sidebar-primary transition-all font-medium"
              />
            </div>

            <div className="space-y-2">
              <label className="text-xs font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.briefingDescription')}</label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="w-full px-4 py-3 bg-sidebar-accent/50 border border-sidebar-border rounded-xl text-foreground placeholder:text-muted-foreground/30 focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 focus:border-sidebar-primary transition-all font-medium"
                rows={3}
              />
            </div>

            <div className="pt-4">
              <div className="flex justify-between items-center mb-4">
                <label className="text-xs font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.logicMatrixRules')}</label>
                <Button
                  type="button"
                  onClick={addRule}
                  variant="outline"
                  size="sm"
                  className="rounded-lg h-8 px-3 border-sidebar-border font-bold text-[10px] uppercase"
                >
                  <Plus className="w-3 h-3 mr-1.5" /> {t('identifiers.appendGate')}
                </Button>
              </div>
              <div className="space-y-4">
                {rules.map((rule, index) => (
                  <ValidationRuleEditor
                    key={index}
                    rule={rule}
                    onChange={(updatedRule) => updateRule(index, updatedRule)}
                    onRemove={() => removeRule(index)}
                    canRemove={rules.length > 1}
                    validationRuleTypes={validationRuleTypes}
                  />
                ))}
              </div>
            </div>
          </form>
        </div>

        <div className="p-8 bg-sidebar-accent/20 border-t border-sidebar-border/50 flex gap-4">
          <Button
            type="button"
            onClick={onClose}
            disabled={submitting}
            variant="ghost"
            className="flex-1 rounded-xl h-12 font-bold text-muted-foreground hover:bg-sidebar-accent"
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            form="edit-config-form"
            disabled={submitting || !hasChanges}
            variant="vibrant"
            className="flex-[2] rounded-xl h-12 font-black uppercase tracking-widest shadow-xl shadow-sidebar-primary/20 flex items-center justify-center gap-3"
          >
            {submitting ? (
              <><Loader2 className="w-5 h-5 animate-spin" /> {t('identifiers.rewritingMatrix')}</>
            ) : (
              <>
                <Save className="w-5 h-5" />
                {t('identifiers.commitProtocol')}
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  )
}

function PatternTester({ pattern }: { pattern: string }) {
  const { t } = useTranslation()
  const [testValue, setTestValue] = useState("")
  const [testResult, setTestResult] = useState<{
    isValid: boolean
    error?: string
  } | null>(null)

  const patternExamples = useMemo(() => [
    { name: t('identifiers.examples.email'), pattern: "^[\\w.-]+@[\\w.-]+\\.\\w+$", test: "test@example.com" },
    { name: t('identifiers.examples.phone'), pattern: "^[\\d\\s\\-\\(\\)]+$", test: "123-456-7890" },
    { name: t('identifiers.examples.username'), pattern: "^[a-zA-Z0-9_]{3,20}$", test: "user123" },
    { name: t('identifiers.examples.password'), pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d@$!%*?&]{8,}$", test: "Password123" },
    { name: t('identifiers.examples.url'), pattern: "^https?://[\\w\\.-]+\\.[a-zA-Z]{2,}(/.*)?$", test: "https://example.com" },
    { name: t('identifiers.examples.numbers'), pattern: "^\\d+$", test: "12345" },
    { name: t('identifiers.examples.letters'), pattern: "^[a-zA-Z]+$", test: "abc" },
    { name: t('identifiers.examples.alphanumeric'), pattern: "^[a-zA-Z0-9]+$", test: "abc123" }
  ], [t])

  const testPattern = () => {
    if (!pattern) {
      setTestResult({ isValid: false, error: t('identifiers.patternRequired') })
      return
    }

    try {
      const regex = new RegExp(pattern)
      const isValid = regex.test(testValue)
      setTestResult({ isValid })
    } catch (error) {
      setTestResult({
        isValid: false,
        error: error instanceof Error ? error.message : t('identifiers.invalidRegex')
      })
    }
  }

  const applyExample = (example: typeof patternExamples[0]) => {
    setTestValue(example.test)
  }

  return (
    <div className="mt-3 p-3 bg-gray-800 rounded-lg border border-gray-600">
      <div className="text-sm font-medium text-gray-300 mb-2">{t('identifiers.patternTester')}</div>

      <div className="mb-3">
        <div className="text-xs text-gray-400 mb-1">{t('identifiers.quickExamples')}:</div>
        <div className="flex flex-wrap gap-1">
          {patternExamples.map((example) => (
            <button
              key={example.name}
              type="button"
              onClick={() => applyExample(example)}
              className="px-2 py-1 bg-gray-700 text-gray-300 text-xs rounded hover:bg-gray-600 transition-colors"
              title={`Pattern: ${example.pattern}\nTest: ${example.test}`}
            >
              {example.name}
            </button>
          ))}
        </div>
      </div>

      <div className="space-y-2">
        <input
          type="text"
          value={testValue}
          onChange={(e) => setTestValue(e.target.value)}
          placeholder={t('identifiers.testPlaceholder')}
          className="w-full px-2 py-1 bg-gray-600 border border-gray-500 rounded text-sm text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
        <button
          type="button"
          onClick={testPattern}
          className="px-3 py-1 bg-blue-600 text-white text-xs rounded hover:bg-blue-700 transition-colors"
        >
          {t('identifiers.testPattern')}
        </button>

        {testResult && (
          <div className={`p-2 rounded text-xs ${testResult.isValid
            ? "bg-green-900 text-green-300 border border-green-700"
            : "bg-red-900 text-red-300 border border-red-700"
            }`}>
            {testResult.isValid ? `✅ ${t('identifiers.patternMatches')}` : `❌ ${testResult.error || t('identifiers.patternRejection')}`}
          </div>
        )}

        {pattern && (
          <div className="mt-2 p-2 bg-gray-700 rounded text-xs">
            <div className="text-gray-400">{t('identifiers.currentPattern')}:</div>
            <code className="text-blue-400 break-all">{pattern}</code>
          </div>
        )}
      </div>
    </div>
  )
}

function ValidationRuleEditor({
  rule,
  onChange,
  onRemove,
  canRemove,
  validationRuleTypes
}: {
  rule: ValidationRuleDto
  onChange: (rule: ValidationRuleDto) => void
  onRemove: () => void
  canRemove: boolean
  validationRuleTypes: { value: number; label: string }[]
}) {
  const { t } = useTranslation()
  const updateRule = (updates: Partial<ValidationRuleDto>) => {
    onChange({ ...rule, ...updates })
  }

  const updateParameter = (key: string, value: string | number | boolean) => {
    updateRule({
      parameters: { ...rule.parameters, [key]: value }
    })
  }

  return (
    <div className="group relative border border-sidebar-border bg-sidebar-accent/30 rounded-2xl p-6 transition-all hover:bg-sidebar-accent/50">
      <div className="flex justify-between items-start mb-6 gap-4">
        <div className="flex-1">
          <label className="text-[10px] font-black uppercase tracking-widest text-muted-foreground ml-1 mb-1.5 block">{t('identifiers.gateMechanism')}</label>
          <select
            value={rule.type}
            onChange={(e) => updateRule({ type: Number(e.target.value) as ValidationRuleType, parameters: {} })}
            className="w-full px-4 py-2.5 bg-background border border-sidebar-border rounded-xl text-sm font-bold text-foreground focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 transition-all appearance-none cursor-pointer"
          >
            {validationRuleTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>
        <button
          type="button"
          onClick={onRemove}
          disabled={!canRemove}
          className={cn(
            "mt-5 p-2.5 rounded-xl transition-all border",
            canRemove
              ? "text-destructive border-destructive/20 bg-destructive/5 hover:bg-destructive/10"
              : "text-muted-foreground/30 border-sidebar-border bg-transparent opacity-50 cursor-not-allowed"
          )}
        >
          <X className="w-4 h-4" />
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="space-y-1.5">
          <label className="text-[10px] font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.sequenceOrder')}</label>
          <div className="relative">
            <input
              type="number"
              value={rule.order}
              onChange={(e) => updateRule({ order: parseInt(e.target.value) || 0 })}
              className="w-full px-4 py-2 bg-background border border-sidebar-border rounded-xl text-sm font-mono text-sidebar-primary focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 transition-all"
              min="0"
            />
          </div>
        </div>

        <div className="space-y-1.5">
          <label className="text-[10px] font-black uppercase tracking-widest text-muted-foreground ml-1">{t('identifiers.neuralErrorMessage')}</label>
          <input
            type="text"
            value={rule.errorMessage || ""}
            onChange={(e) => updateRule({ errorMessage: e.target.value || null })}
            className="w-full px-4 py-2 bg-background border border-sidebar-border rounded-xl text-sm italic text-foreground placeholder:text-muted-foreground/30 focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 transition-all"
            placeholder={t('identifiers.neuralPlaceholder')}
          />
        </div>

        <div className="md:col-span-2 pt-2">
          <label className="text-[10px] font-black uppercase tracking-widest text-muted-foreground ml-1 mb-2 block">{t('identifiers.gateParameters')}</label>

          <div className="p-4 bg-background/50 rounded-xl border border-sidebar-border/50">
            {rule.type === ValidationRuleType.Required && (
              <div className="text-[10px] text-muted-foreground font-bold flex items-center gap-2">
                <div className="h-1.5 w-1.5 bg-sidebar-primary rounded-full" />
                {t('identifiers.noEncryptionKeys')}
              </div>
            )}

            {(rule.type === ValidationRuleType.MinLength || rule.type === ValidationRuleType.MaxLength) && (
              <div className="flex items-center gap-4">
                <span className="text-xs font-bold text-muted-foreground uppercase">{rule.type === ValidationRuleType.MinLength ? t('identifiers.minimum') : t('identifiers.maximum')} {t('identifiers.bounds')}:</span>
                <input
                  type="number"
                  value={(rule.parameters.length as number) || 0}
                  onChange={(e) => updateParameter("length", parseInt(e.target.value) || 0)}
                  className="w-24 px-3 py-1.5 bg-background border border-sidebar-border rounded-lg text-sm font-mono text-sidebar-primary focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 transition-all"
                  min="1"
                />
              </div>
            )}

            {rule.type === ValidationRuleType.Pattern && (
              <div className="space-y-4">
                <div className="flex flex-col gap-2">
                  <span className="text-xs font-bold text-muted-foreground uppercase">{t('identifiers.regexMatrix')}:</span>
                  <input
                    type="text"
                    value={(rule.parameters.pattern as string) || ""}
                    onChange={(e) => updateParameter("pattern", e.target.value)}
                    className="w-full px-4 py-2.5 bg-background border border-sidebar-border rounded-xl text-sm font-mono text-sidebar-primary focus:outline-none focus:ring-2 focus:ring-sidebar-primary/50 transition-all"
                    placeholder="^[A-Z0-9]+$"
                  />
                </div>
                <PatternTester pattern={(rule.parameters.pattern as string) || ""} />
              </div>
            )}

            {rule.type === ValidationRuleType.Range && (
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <span className="text-[10px] font-bold text-muted-foreground uppercase">{t('identifiers.lowBound')}</span>
                  <input
                    type="number"
                    value={(rule.parameters.min as number) || 0}
                    onChange={(e) => updateParameter("min", parseFloat(e.target.value) || 0)}
                    className="w-full px-3 py-1.5 bg-background border border-sidebar-border rounded-lg text-sm font-mono text-sidebar-primary"
                  />
                </div>
                <div className="space-y-1.5">
                  <span className="text-[10px] font-bold text-muted-foreground uppercase">{t('identifiers.highBound')}</span>
                  <input
                    type="number"
                    value={(rule.parameters.max as number) || 0}
                    onChange={(e) => updateParameter("max", parseFloat(e.target.value) || 0)}
                    className="w-full px-3 py-1.5 bg-background border border-sidebar-border rounded-lg text-sm font-mono text-sidebar-primary"
                  />
                </div>
              </div>
            )}

            {rule.type === ValidationRuleType.Custom && (
              <div className="space-y-2">
                <span className="text-xs font-bold text-muted-foreground uppercase block">{t('identifiers.systemHeuristics')}:</span>
                <select
                  value={(rule.parameters.customLogic as string) || ""}
                  onChange={(e) => updateParameter("customLogic", e.target.value)}
                  className="w-full px-4 py-2 bg-background border border-sidebar-border rounded-xl text-sm font-bold text-foreground focus:outline-none"
                >
                  <option value="">{t('identifiers.selectHeuristic')}</option>
                  <option value="uppercase">{t('identifiers.uppercaseMatrix')}</option>
                  <option value="lowercase">{t('identifiers.lowercaseMatrix')}</option>
                  <option value="alphanumeric">{t('identifiers.alphanumericSequence')}</option>
                  <option value="numeric">{t('identifiers.pureNumeric')}</option>
                  <option value="letters">{t('identifiers.pureAlpha')}</option>
                </select>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
