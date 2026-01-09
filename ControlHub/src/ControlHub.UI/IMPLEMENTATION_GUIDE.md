# ControlHub Frontend Implementation Guide

## Overview

This guide provides comprehensive documentation for the ControlHub frontend implementation, covering all APIs, components, hooks, and best practices.

## Table of Contents

1. [Architecture](#architecture)
2. [API Integration](#api-integration)
3. [Components](#components)
4. [Hooks](#hooks)
5. [Pages](#pages)
6. [Authentication & Authorization](#authentication--authorization)
7. [State Management](#state-management)
8. [Error Handling](#error-handling)
9. [Performance Optimizations](#performance-optimizations)

---

## Architecture

### Directory Structure

```
src/
├── auth/                    # Authentication context and utilities
├── components/
│   ├── account/            # Account management components
│   ├── permissions/        # Permission management components
│   ├── roles/              # Role management components
│   ├── users/              # User management components
│   ├── dashboard/          # Dashboard layout components
│   ├── layout/             # Main layout components
│   └── ui/                 # Reusable UI components
├── hooks/                  # Custom React hooks
├── pages/                  # Page components
├── services/
│   └── api/                # API service layer
└── lib/                    # Utility functions
```

---

## API Integration

### Service Layer (`src/services/api/`)

All API calls are centralized in the service layer with TypeScript type safety.

#### Core Modules

**Client (`client.ts`)**
```typescript
// Generic fetch wrapper with error handling
fetchJson<T>(path, options)
fetchVoid(path, options)
```

**Auth (`auth.ts`)**
```typescript
signIn(req: SignInRequest)
registerUser(req: RegisterRequest)
registerAdmin(req: RegisterRequest, accessToken: string)
registerSuperAdmin(req: RegisterSuperAdminRequest)
refreshAccessToken(req: RefreshTokenRequest)
signOut(req: SignOutRequest, accessToken: string)
```

**Account (`account.ts`)**
```typescript
changePassword(userId: string, req: ChangePasswordRequest, accessToken: string)
forgotPassword(req: ForgotPasswordRequest)
resetPassword(req: ResetPasswordRequest)
```

**Permissions (`permissions.ts`)**
```typescript
createPermissions(req: CreatePermissionsRequest, accessToken: string)
getPermissions(params: PaginationParams, accessToken: string)
```

**Roles (`roles.ts`)**
```typescript
createRoles(req: CreateRolesRequest, accessToken: string)
addPermissionsForRole(req: AddPermissionsForRoleRequest, accessToken: string)
getRoles(params: PaginationParams, accessToken: string)
```

**Users (`users.ts`)**
```typescript
updateUsername(userId: string, req: UpdateUsernameRequest, accessToken: string)
```

### Usage Example

```typescript
import { permissionsApi } from '@/services/api'

// Create permissions
await permissionsApi.createPermissions(
  { permissions: ['user.view', 'user.edit'] },
  accessToken
)

// Get paginated permissions
const result = await permissionsApi.getPermissions(
  { pageIndex: 1, pageSize: 10, searchTerm: 'user' },
  accessToken
)
```

---

## Components

### UI Components (`src/components/ui/`)

#### Pagination
```typescript
<Pagination
  currentPage={1}
  totalPages={5}
  totalCount={50}
  onPageChange={(page) => handlePageChange(page)}
  hasPreviousPage={true}
  hasNextPage={true}
/>
```

#### LoadingState
```typescript
<LoadingState message="Loading data..." />
```

#### EmptyState
```typescript
<EmptyState
  icon={FileX}
  title="No results found"
  description="Try adjusting your search"
  action={{
    label: "Clear Filters",
    onClick: () => clearFilters()
  }}
/>
```

#### ErrorBoundary
```typescript
<ErrorBoundary>
  <YourComponent />
</ErrorBoundary>
```

#### Card Components
```typescript
<Card>
  <CardHeader>
    <CardTitle>Title</CardTitle>
    <CardDescription>Description</CardDescription>
  </CardHeader>
  <CardContent>
    Content here
  </CardContent>
  <CardFooter>
    Footer actions
  </CardFooter>
</Card>
```

### Feature Components

#### Change Password Dialog
```typescript
<ChangePasswordDialog
  userId={userId}
  accessToken={accessToken}
  open={isOpen}
  onOpenChange={setIsOpen}
/>
```

#### Create Permissions Dialog
```typescript
<CreatePermissionsDialog
  accessToken={accessToken}
  open={isOpen}
  onOpenChange={setIsOpen}
  onSuccess={() => refetchData()}
/>
```

#### Assign Permissions Dialog
```typescript
<AssignPermissionsDialog
  role={selectedRole}
  accessToken={accessToken}
  open={isOpen}
  onOpenChange={setIsOpen}
  onSuccess={() => refetchData()}
/>
```

---

## Hooks

### useTokenRefresh

Automatically refreshes access tokens every 14 minutes.

```typescript
import { useTokenRefresh } from '@/hooks/use-token-refresh'

function MyComponent() {
  useTokenRefresh() // Call in top-level component (MainLayout)
}
```

### useDebounce

Debounces a value to reduce API calls.

```typescript
import { useDebounce } from '@/hooks/use-debounce'

const [searchTerm, setSearchTerm] = useState('')
const debouncedSearchTerm = useDebounce(searchTerm, 500)

useEffect(() => {
  // API call with debounced value
  fetchResults(debouncedSearchTerm)
}, [debouncedSearchTerm])
```

### useAsync

Manages async operations with loading and error states.

```typescript
import { useAsync } from '@/hooks/use-async'

const { data, error, isLoading, execute } = useAsync(fetchData)

// Call execute when needed
await execute(params)
```

### useAuth

Access authentication context.

```typescript
import { useAuth } from '@/auth/use-auth'

const { auth, isAuthenticated, signIn, signOut, updateAuth } = useAuth()
```

---

## Pages

### Permissions Page (`/permissions`)
- List permissions with pagination
- Search permissions
- Create new permissions
- View permission details

### Roles Page (`/roles`)
- List roles with pagination
- Search roles
- Create new roles
- Manage role permissions
- View assigned permissions

### Users Page (`/users`)
- View user profile
- Update username
- Display account information

### Settings Page (`/settings`)
- Account information
- Change password
- Update username
- Sign out

---

## Authentication & Authorization

### Token Management

**Access Token:** Short-lived token (15 minutes) for API authentication
**Refresh Token:** Long-lived token for getting new access tokens

### Auto-Refresh Mechanism

Tokens are automatically refreshed every 14 minutes via `useTokenRefresh` hook in the MainLayout.

```typescript
// Automatic refresh in MainLayout
export function MainLayout() {
  useTokenRefresh() // Refreshes tokens every 14 mins
  // ...
}
```

### Manual Refresh

```typescript
import { authApi } from '@/services/api'

const result = await authApi.refreshAccessToken({
  refreshToken: auth.refreshToken,
  accID: auth.accountId,
  accessToken: auth.accessToken,
})

updateAuth({
  accessToken: result.accessToken,
  refreshToken: result.refreshToken,
})
```

### Sign Out

```typescript
const { signOut } = useAuth()

// Calls backend API and clears local storage
await signOut()
```

---

## State Management

### Authentication State

Managed via React Context in `src/auth/context.tsx`:

```typescript
interface AuthContextValue {
  auth: AuthData | null
  isAuthenticated: boolean
  signIn: (req: SignInRequest) => Promise<void>
  register: (role: RegisterRole, req: RegisterRequest) => Promise<void>
  signOut: () => Promise<void>
  updateAuth: (updates: Partial<AuthData>) => void
}
```

### Local State

Component-level state using `useState` for:
- Form inputs
- Dialog visibility
- Pagination state
- Loading states

---

## Error Handling

### API Error Handling

All API calls use a centralized error handler:

```typescript
async function fetchJson<T>(path: string, options?: RequestOptions): Promise<T> {
  try {
    const response = await fetch(url, config)
    if (!response.ok) {
      throw new Error(await readErrorMessage(response))
    }
    return await response.json()
  } catch (error) {
    throw error
  }
}
```

### Error Display

```typescript
try {
  await apiCall()
  toast.success("Operation successful")
} catch (error) {
  toast.error(error instanceof Error ? error.message : "Operation failed")
}
```

### Error Boundary

Wrap components to catch rendering errors:

```typescript
<ErrorBoundary>
  <MyComponent />
</ErrorBoundary>
```

---

## Performance Optimizations

### 1. Debounced Search

Reduces API calls during typing:

```typescript
const debouncedSearch = useDebounce(searchTerm, 500)
```

### 2. Pagination

Loads data in chunks instead of all at once:

```typescript
const params = {
  pageIndex: 1,
  pageSize: 10,
  searchTerm: searchValue
}
```

### 3. Lazy Loading

Components are loaded on-demand via React Router.

### 4. Memoization

Use `useMemo` and `useCallback` for expensive computations:

```typescript
const expensiveValue = useMemo(() => computeExpensive(data), [data])
const memoizedCallback = useCallback(() => doSomething(), [dependency])
```

### 5. Optimistic Updates

Update UI immediately, rollback on error:

```typescript
const optimisticUpdate = (newData) => {
  setData(newData) // Update UI
  try {
    await api.update(newData)
  } catch (error) {
    setData(oldData) // Rollback on error
    toast.error("Update failed")
  }
}
```

---

## Best Practices

### 1. Type Safety

Always use TypeScript types:

```typescript
import type { Permission, Role } from '@/services/api'

const [permissions, setPermissions] = useState<Permission[]>([])
```

### 2. Error Messages

Provide clear, actionable error messages:

```typescript
toast.error("Failed to create permission. Please check the permission name.")
```

### 3. Loading States

Show loading indicators for async operations:

```typescript
{isLoading ? <LoadingState /> : <DataTable data={data} />}
```

### 4. Empty States

Display helpful empty states:

```typescript
{data.length === 0 && (
  <EmptyState
    title="No data found"
    description="Get started by creating your first item"
    action={{ label: "Create Item", onClick: openDialog }}
  />
)}
```

### 5. Accessibility

- Use semantic HTML
- Provide ARIA labels
- Ensure keyboard navigation
- Maintain focus management

### 6. Code Organization

- Keep components small and focused
- Extract reusable logic into hooks
- Use composition over inheritance
- Follow single responsibility principle

---

## Testing Checklist

- [ ] Authentication flow (signin, signout, refresh)
- [ ] Permission CRUD operations
- [ ] Role CRUD operations
- [ ] Permission assignment to roles
- [ ] User profile updates
- [ ] Password change
- [ ] Search and pagination
- [ ] Error handling
- [ ] Loading states
- [ ] Empty states
- [ ] Token refresh mechanism
- [ ] Responsive design
- [ ] Accessibility

---

## Common Issues & Solutions

### Issue: Token Expired
**Solution:** Token refresh hook should handle this automatically. Check browser console for errors.

### Issue: API Call Failed
**Solution:** Check network tab, verify token is valid, ensure API is running.

### Issue: Component Not Re-rendering
**Solution:** Verify state updates, check dependency arrays in useEffect/useMemo.

### Issue: Search Not Working
**Solution:** Ensure debounce is working, check API parameters.

---

## Future Enhancements

1. **Real-time Updates:** WebSocket integration for live data
2. **Bulk Operations:** Multi-select and bulk actions
3. **Advanced Filters:** More filtering options
4. **Export Data:** Download as CSV/PDF
5. **Audit Logs:** View system activity
6. **User Management:** Full CRUD for users
7. **Dark/Light Mode:** Theme switching
8. **Notifications:** In-app notification system
9. **Analytics Dashboard:** Usage statistics
10. **Two-Factor Auth:** Enhanced security

---

## Support

For issues or questions:
1. Check this documentation
2. Review the code examples
3. Check browser console for errors
4. Verify API endpoints are accessible
5. Review backend logs

---

**Last Updated:** January 2026
**Version:** 1.0.0
