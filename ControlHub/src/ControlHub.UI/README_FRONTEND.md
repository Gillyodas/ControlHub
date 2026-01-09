# ControlHub Frontend

A modern, type-safe React frontend for the ControlHub API management system.

## 🚀 Features

### ✅ Complete API Coverage
- **Authentication** - Sign in, register (User/Admin/SuperAdmin), sign out, token refresh
- **Account Management** - Change password, forgot/reset password
- **Permissions** - Create, list, search with pagination
- **Roles** - Create, list, assign permissions, search with pagination
- **Users** - Update username, view profile

### 🎨 Modern UI/UX
- Clean, professional design with Tailwind CSS
- Responsive layout for all screen sizes
- Dark theme optimized
- Smooth animations and transitions
- Accessible components following WCAG guidelines

### 🔐 Security
- JWT-based authentication
- Automatic token refresh (every 14 minutes)
- Secure session management
- Protected routes with authorization checks

### ⚡ Performance
- Debounced search (500ms delay)
- Pagination for large datasets
- Lazy loading of routes
- Optimized re-renders with React hooks

## 📁 Project Structure

```
src/
├── auth/                      # Authentication context & utilities
│   ├── context.tsx           # Auth provider with token refresh
│   ├── api.ts                # Legacy auth API (being phased out)
│   └── types.ts              # Auth type definitions
│
├── components/
│   ├── account/              # Account management
│   │   └── change-password-dialog.tsx
│   ├── permissions/          # Permission management
│   │   ├── create-permissions-dialog.tsx
│   │   └── permissions-table.tsx
│   ├── roles/                # Role management
│   │   ├── create-roles-dialog.tsx
│   │   ├── roles-table.tsx
│   │   └── assign-permissions-dialog.tsx
│   ├── users/                # User management
│   │   └── update-username-dialog.tsx
│   ├── ui/                   # Reusable UI components
│   │   ├── button.tsx
│   │   ├── input.tsx
│   │   ├── dialog.tsx
│   │   ├── table.tsx
│   │   ├── pagination.tsx
│   │   ├── loading-state.tsx
│   │   ├── empty-state.tsx
│   │   ├── error-boundary.tsx
│   │   └── card.tsx
│   └── dashboard/            # Layout components
│       ├── sidebar.tsx
│       └── header.tsx
│
├── hooks/                    # Custom React hooks
│   ├── use-token-refresh.ts # Auto token refresh
│   ├── use-debounce.ts      # Debounce hook
│   └── use-async.ts         # Async state management
│
├── pages/                    # Page components
│   ├── permissions-page.tsx
│   ├── roles-management-page.tsx
│   ├── users-page.tsx
│   ├── settings-page.tsx
│   ├── login-page.tsx
│   ├── forgot-password-page.tsx
│   └── reset-password-page.tsx
│
├── services/
│   └── api/                  # API service layer
│       ├── types.ts          # API type definitions
│       ├── client.ts         # Base HTTP client
│       ├── auth.ts           # Auth endpoints
│       ├── account.ts        # Account endpoints
│       ├── permissions.ts    # Permission endpoints
│       ├── roles.ts          # Role endpoints
│       └── users.ts          # User endpoints
│
└── lib/                      # Utilities
    └── utils.ts              # Helper functions
```

## 🛠️ Tech Stack

- **Framework:** React 19 + TypeScript
- **Routing:** React Router DOM v6
- **Styling:** Tailwind CSS v4
- **UI Components:** shadcn/ui (Radix UI primitives)
- **Icons:** Lucide React
- **Notifications:** Sonner
- **Build Tool:** Vite
- **HTTP Client:** Native Fetch API

## 📦 Installation

```bash
cd src/ControlHub.UI
npm install
```

## 🚀 Development

```bash
# Start dev server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## 🔧 Configuration

Create a `.env` file in the root:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_BASE_URL=/control-hub
```

## 📖 API Integration

### Quick Start

```typescript
import { permissionsApi } from '@/services/api'

// Create permissions
await permissionsApi.createPermissions(
  { permissions: ['user.view', 'user.edit'] },
  accessToken
)

// Get permissions with pagination
const result = await permissionsApi.getPermissions(
  { pageIndex: 1, pageSize: 10, searchTerm: 'user' },
  accessToken
)
```

### Available Services

- `authApi` - Authentication operations
- `accountApi` - Account management
- `permissionsApi` - Permission CRUD
- `rolesApi` - Role CRUD and permission assignment
- `usersApi` - User profile updates

## 🎯 Key Features

### 1. Automatic Token Refresh
Tokens are automatically refreshed every 14 minutes to maintain user sessions:

```typescript
// In MainLayout component
useTokenRefresh() // Handles refresh automatically
```

### 2. Debounced Search
Reduces API calls during search:

```typescript
const debouncedSearch = useDebounce(searchTerm, 500)

useEffect(() => {
  fetchData(debouncedSearch)
}, [debouncedSearch])
```

### 3. Type-Safe API Calls
Full TypeScript support for all API interactions:

```typescript
import type { Permission, PagedResult } from '@/services/api'

const [permissions, setPermissions] = useState<PagedResult<Permission>>({
  items: [],
  pageIndex: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
})
```

### 4. Reusable Components

**Pagination:**
```tsx
<Pagination
  currentPage={page}
  totalPages={total}
  totalCount={count}
  onPageChange={handlePageChange}
  hasPreviousPage={hasPrev}
  hasNextPage={hasNext}
/>
```

**Loading State:**
```tsx
<LoadingState message="Loading data..." />
```

**Empty State:**
```tsx
<EmptyState
  icon={FileX}
  title="No results"
  description="Try adjusting your search"
  action={{ label: "Clear", onClick: clear }}
/>
```

## 🔐 Authentication Flow

1. **Sign In** → Get access token + refresh token
2. **Auto Refresh** → Token refreshed every 14 minutes
3. **API Calls** → Access token sent in Authorization header
4. **Sign Out** → Clear tokens + notify backend

## 📱 Responsive Design

The application is fully responsive:
- **Desktop:** Full sidebar navigation
- **Tablet:** Collapsible sidebar
- **Mobile:** Drawer navigation (future enhancement)

## 🎨 Theme

Currently using a dark theme optimized for:
- Reduced eye strain
- Professional appearance
- Better focus on content

## 🧪 Testing

Run tests:
```bash
npm run test
```

Run linter:
```bash
npm run lint
```

## 📚 Documentation

- **[API Documentation](FRONTEND_API_DOCUMENTATION.md)** - Complete API reference
- **[Implementation Guide](IMPLEMENTATION_GUIDE.md)** - Detailed implementation guide

## 🛣️ Routing

| Route | Component | Description |
|-------|-----------|-------------|
| `/` | DashboardPage | Main dashboard |
| `/login` | LoginPage | Sign in |
| `/forgot-password` | ForgotPasswordPage | Password recovery |
| `/reset-password` | ResetPasswordPage | Reset password |
| `/users` | UsersPage | User profile |
| `/roles` | RolesManagementPage | Role management |
| `/permissions` | PermissionsPage | Permission management |
| `/identify` | IdentifyPage | Identity management |
| `/apis` | ApisPage | API documentation |
| `/settings` | SettingsPage | Account settings |

## 🔄 State Management

- **Authentication:** React Context (`AuthProvider`)
- **Component State:** `useState` for local state
- **Async State:** Custom `useAsync` hook
- **Form State:** Controlled components

## 🚨 Error Handling

Comprehensive error handling at multiple levels:

1. **API Level:** Centralized error parsing
2. **Component Level:** Try-catch with user feedback
3. **Boundary Level:** ErrorBoundary component
4. **Global Level:** Toast notifications

## 🎁 Components Catalog

### Dialogs
- ChangePasswordDialog
- CreatePermissionsDialog
- CreateRolesDialog
- AssignPermissionsDialog
- UpdateUsernameDialog

### Tables
- PermissionsTable
- RolesTable

### UI Primitives
- Button, Input, Label, Textarea
- Dialog, Avatar, Badge, Separator
- Dropdown Menu, Tooltip
- Table, Card, Pagination

## 💡 Best Practices

1. ✅ Always use TypeScript types
2. ✅ Handle loading states
3. ✅ Show error messages
4. ✅ Provide empty states
5. ✅ Use debounce for search
6. ✅ Implement pagination
7. ✅ Validate user input
8. ✅ Keep components small
9. ✅ Extract reusable logic to hooks
10. ✅ Follow accessibility guidelines

## 🔮 Future Enhancements

- [ ] Real-time updates via WebSockets
- [ ] Bulk operations (multi-select)
- [ ] Advanced filtering
- [ ] Data export (CSV/PDF)
- [ ] Audit log viewer
- [ ] Two-factor authentication
- [ ] Theme switcher (light/dark)
- [ ] Mobile drawer navigation
- [ ] Offline support
- [ ] PWA capabilities

## 🤝 Contributing

1. Follow TypeScript best practices
2. Use existing UI components
3. Add proper error handling
4. Include loading states
5. Write meaningful commit messages
6. Update documentation

## 📄 License

This project is part of ControlHub API management system.

---

**Built with ❤️ using React + TypeScript + Tailwind CSS**
