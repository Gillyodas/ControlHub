import { useState } from "react"
import { Code2, ChevronDown, ChevronRight } from "lucide-react"

interface ApiEndpoint {
  path: string
  method: string
  description: string
  controller: string
  requiresAuth: boolean
}

interface ApiGroup {
  name: string
  endpoints: ApiEndpoint[]
}

// Hardcoded API endpoints from backend
const API_ENDPOINTS: ApiGroup[] = [
  {
    name: "Authentication",
    endpoints: [
      { path: "/api/Auth/login", method: "POST", description: "User login with credentials", controller: "AuthController", requiresAuth: false },
      { path: "/api/Auth/register", method: "POST", description: "Register new user account", controller: "AuthController", requiresAuth: false },
      { path: "/api/Auth/register-admin", method: "POST", description: "Register admin account", controller: "AuthController", requiresAuth: true },
      { path: "/api/Auth/refresh-token", method: "POST", description: "Refresh access token", controller: "AuthController", requiresAuth: false },
      { path: "/api/Auth/forgot-password", method: "POST", description: "Request password reset", controller: "AuthController", requiresAuth: false },
      { path: "/api/Auth/reset-password", method: "POST", description: "Reset password with token", controller: "AuthController", requiresAuth: false },
      { path: "/api/Auth/change-password", method: "POST", description: "Change user password", controller: "AuthController", requiresAuth: true },
    ]
  },
  {
    name: "User Management",
    endpoints: [
      { path: "/api/User/users/{id}/username", method: "PATCH", description: "Update username for a user", controller: "UserController", requiresAuth: true },
    ]
  },
  {
    name: "Role Management",
    endpoints: [
      { path: "/api/Role", method: "GET", description: "Get all roles", controller: "RoleController", requiresAuth: true },
      { path: "/api/Role/{id}", method: "GET", description: "Get role by ID", controller: "RoleController", requiresAuth: true },
      { path: "/api/Role", method: "POST", description: "Create new role", controller: "RoleController", requiresAuth: true },
      { path: "/api/Role/{id}", method: "PUT", description: "Update role", controller: "RoleController", requiresAuth: true },
      { path: "/api/Role/{id}", method: "DELETE", description: "Delete role", controller: "RoleController", requiresAuth: true },
    ]
  },
  {
    name: "Permission Management",
    endpoints: [
      { path: "/api/Permission", method: "GET", description: "Get all permissions", controller: "PermissionController", requiresAuth: true },
      { path: "/api/Permission/{id}", method: "GET", description: "Get permission by ID", controller: "PermissionController", requiresAuth: true },
      { path: "/api/Permission", method: "POST", description: "Create new permission", controller: "PermissionController", requiresAuth: true },
    ]
  },
  {
    name: "Identifier Configuration",
    endpoints: [
      { path: "/api/Identifier", method: "GET", description: "Get all identifier configurations", controller: "IdentifierController", requiresAuth: true },
      { path: "/api/Identifier", method: "POST", description: "Create new identifier configuration", controller: "IdentifierController", requiresAuth: true },
    ]
  },
]

export default function ApiExplorerPage() {
  const [expandedGroups, setExpandedGroups] = useState<Set<string>>(
    new Set(API_ENDPOINTS.map(g => g.name))
  )

  const toggleGroup = (groupName: string) => {
    const newExpanded = new Set(expandedGroups)
    if (newExpanded.has(groupName)) {
      newExpanded.delete(groupName)
    } else {
      newExpanded.add(groupName)
    }
    setExpandedGroups(newExpanded)
  }

  const getMethodColor = (method: string) => {
    switch (method) {
      case "GET": return "bg-blue-900 text-blue-300"
      case "POST": return "bg-green-900 text-green-300"
      case "PUT": return "bg-yellow-900 text-yellow-300"
      case "PATCH": return "bg-orange-900 text-orange-300"
      case "DELETE": return "bg-red-900 text-red-300"
      default: return "bg-gray-700 text-gray-300"
    }
  }

  return (
    <div className="min-h-screen bg-gray-900 text-gray-100 p-6">
      <div className="max-w-7xl mx-auto">
        <div className="flex items-center gap-3 mb-6">
          <Code2 className="w-8 h-8 text-blue-500" />
          <div>
            <h1 className="text-2xl font-bold text-gray-100">API Explorer</h1>
            <p className="text-gray-400 text-sm">Browse and explore available API endpoints</p>
          </div>
        </div>

        <div className="space-y-4">
          {API_ENDPOINTS.map((group) => (
            <div key={group.name} className="bg-gray-800 rounded-lg border border-gray-700 overflow-hidden">
              <button
                onClick={() => toggleGroup(group.name)}
                className="w-full flex items-center justify-between p-4 hover:bg-gray-750 transition-colors"
              >
                <div className="flex items-center gap-3">
                  {expandedGroups.has(group.name) ? (
                    <ChevronDown className="w-5 h-5 text-gray-400" />
                  ) : (
                    <ChevronRight className="w-5 h-5 text-gray-400" />
                  )}
                  <h2 className="text-lg font-semibold text-gray-100">{group.name}</h2>
                  <span className="px-2 py-1 bg-gray-700 text-gray-300 text-xs rounded-full">
                    {group.endpoints.length} endpoint{group.endpoints.length !== 1 ? "s" : ""}
                  </span>
                </div>
              </button>

              {expandedGroups.has(group.name) && (
                <div className="border-t border-gray-700">
                  {group.endpoints.map((endpoint, index) => (
                    <div
                      key={index}
                      className="p-4 border-b border-gray-700 last:border-b-0 hover:bg-gray-750 transition-colors"
                    >
                      <div className="flex items-start gap-3">
                        <span className={`px-2 py-1 rounded text-xs font-mono font-semibold ${getMethodColor(endpoint.method)}`}>
                          {endpoint.method}
                        </span>
                        <div className="flex-1">
                          <code className="text-sm text-blue-400 font-mono">{endpoint.path}</code>
                          <p className="text-gray-400 text-sm mt-1">{endpoint.description}</p>
                          <div className="flex items-center gap-3 mt-2">
                            <span className="text-xs text-gray-500">
                              Controller: <span className="text-gray-400">{endpoint.controller}</span>
                            </span>
                            {endpoint.requiresAuth && (
                              <span className="px-2 py-0.5 bg-yellow-900 text-yellow-300 text-xs rounded">
                                🔒 Auth Required
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>

        <div className="mt-6 p-4 bg-gray-800 border border-gray-700 rounded-lg">
          <h3 className="text-sm font-semibold text-gray-100 mb-2">API Information</h3>
          <div className="space-y-1 text-xs text-gray-400">
            <p>• Base URL: <code className="text-blue-400">https://localhost:7110</code></p>
            <p>• Authentication: Bearer Token (JWT)</p>
            <p>• Content-Type: application/json</p>
            <p>• Total Endpoints: {API_ENDPOINTS.reduce((sum, g) => sum + g.endpoints.length, 0)}</p>
          </div>
        </div>
      </div>
    </div>
  )
}
