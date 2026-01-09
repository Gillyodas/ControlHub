import * as React from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Plus, Trash2, Edit, Check, X } from "lucide-react"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Textarea } from "@/components/ui/textarea"
import { Badge } from "@/components/ui/badge"

type IdentityType = {
  id: string
  name: string
  description: string
  regex: string
  example: string
  isActive: boolean
}

export function IdentifyPage() {
  const [identities, setIdentities] = React.useState<IdentityType[]>([])
  const [isDialogOpen, setIsDialogOpen] = React.useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = React.useState(false)
  const [editingId, setEditingId] = React.useState<string | null>(null)
  const [deleteId, setDeleteId] = React.useState<string | null>(null)
  const [testValue, setTestValue] = React.useState("")
  const [formData, setFormData] = React.useState<Omit<IdentityType, 'id'>>({ 
    name: '', 
    description: '', 
    regex: '',
    example: '',
    isActive: true 
  })

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (editingId) {
      setIdentities(prev => 
        prev.map(item => 
          item.id === editingId ? { ...formData, id: editingId } : item
        )
      )
    } else {
      const newIdentity: IdentityType = {
        id: crypto.randomUUID(),
        ...formData
      }
      setIdentities(prev => [...prev, newIdentity])
    }
    setIsDialogOpen(false)
    resetForm()
  }

  const handleEdit = (id: string) => {
    const identity = identities.find(item => item.id === id)
    if (identity) {
      setFormData({
        name: identity.name,
        description: identity.description,
        regex: identity.regex,
        example: identity.example,
        isActive: identity.isActive
      })
      setEditingId(id)
      setIsDialogOpen(true)
    }
  }

  const handleDelete = () => {
    if (deleteId) {
      setIdentities(prev => prev.filter(item => item.id !== deleteId))
      setIsDeleteDialogOpen(false)
      setDeleteId(null)
    }
  }

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      regex: '',
      example: '',
      isActive: true
    })
    setEditingId(null)
    setTestValue("")
  }

  const testRegex = () => {
    try {
      if (!formData.regex) return false
      const regex = new RegExp(formData.regex)
      return regex.test(testValue)
    } catch {
      return false
    }
  }

  const isRegexValid = React.useMemo(() => {
    try {
      if (!formData.regex) return true
      new RegExp(formData.regex)
      return true
    } catch {
      return false
    }
  }, [formData.regex])

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Identity Types</h1>
          <p className="text-muted-foreground">Manage different types of identity verification</p>
        </div>
        <Button onClick={() => {
          resetForm()
          setIsDialogOpen(true)
        }}>
          <Plus className="mr-2 h-4 w-4" />
          Add Identity Type
        </Button>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Description</TableHead>
              <TableHead>Regex Pattern</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {identities.length > 0 ? (
              identities.map((identity) => (
                <TableRow key={identity.id}>
                  <TableCell className="font-medium">{identity.name}</TableCell>
                  <TableCell className="max-w-[200px] truncate">{identity.description}</TableCell>
                  <TableCell className="font-mono text-xs">
                    {identity.regex}
                  </TableCell>
                  <TableCell>
                    <Badge variant={identity.isActive ? "default" : "secondary"}>
                      {identity.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleEdit(identity.id)}
                    >
                      <Edit className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => {
                        setDeleteId(identity.id)
                        setIsDeleteDialogOpen(true)
                      }}
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={5} className="h-24 text-center">
                  No identity types found. Click "Add Identity Type" to create one.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-[600px]">
          <DialogHeader>
            <DialogTitle>
              {editingId ? 'Edit Identity Type' : 'Add New Identity Type'}
            </DialogTitle>
            <DialogDescription>
              {editingId 
                ? 'Update the identity type details below.'
                : 'Fill in the details for the new identity type.'}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <div className="grid gap-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="name">Name *</Label>
                <Input
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  placeholder="e.g., Email, Phone Number"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description</Label>
                <Textarea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleInputChange}
                  placeholder="Enter a description for this identity type"
                  rows={3}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="regex">Regex Pattern *</Label>
                <div className="flex items-center space-x-2">
                  <div className="flex-1 relative">
                    <Input
                      id="regex"
                      name="regex"
                      value={formData.regex}
                      onChange={handleInputChange}
                      placeholder="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                      className={!isRegexValid ? 'border-red-500' : ''}
                      required
                    />
                    {!isRegexValid && (
                      <p className="text-xs text-red-500 mt-1">
                        Invalid regular expression
                      </p>
                    )}
                  </div>
                </div>
                <div className="text-xs text-muted-foreground">
                  Example email pattern: example@domain.com
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="example">Example Value</Label>
                <Input
                  id="example"
                  name="example"
                  value={formData.example}
                  onChange={handleInputChange}
                  placeholder="e.g., example@domain.com"
                />
              </div>
              <div className="space-y-2">
                <Label>Test Regex</Label>
                <div className="flex space-x-2">
                  <Input
                    value={testValue}
                    onChange={(e) => setTestValue(e.target.value)}
                    placeholder="Enter a value to test the regex"
                    className="flex-1"
                  />
                  {testValue && (
                    <div className="flex items-center">
                      {testRegex() ? (
                        <Check className="h-5 w-5 text-green-500" />
                      ) : (
                        <X className="h-5 w-5 text-red-500" />
                      )}
                    </div>
                  )}
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  id="isActive"
                  name="isActive"
                  checked={formData.isActive}
                  onChange={(e) => 
                    setFormData(prev => ({
                      ...prev,
                      isActive: e.target.checked
                    }))
                  }
                  className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                />
                <Label htmlFor="isActive">Active</Label>
              </div>
            </div>
            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  setIsDialogOpen(false)
                  resetForm()
                }}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={!isRegexValid}>
                {editingId ? 'Update' : 'Create'} Identity Type
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Delete Identity Type</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete this identity type? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsDeleteDialogOpen(false)
                setDeleteId(null)
              }}
            >
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
