# ControlHub Frontend - Quick Start Guide

Get up and running with the ControlHub frontend in minutes.

## 🚀 Installation

```bash
# Navigate to the UI directory
cd src/ControlHub.UI

# Install dependencies
npm install

# Copy environment file
cp .env.example .env

# Start development server
npm run dev
```

The application will be available at `http://localhost:3000/control-hub`

## 🔧 Configuration

Edit `.env` file:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_BASE_URL=/control-hub
```

Make sure your backend API is running at the configured URL.

## 📋 First Steps

### 1. Register a SuperAdmin Account

Navigate to the login page and click "Register SuperAdmin" (or use the API directly):

```bash
POST http://localhost:5000/api/Auth/register-superadmin
Content-Type: application/json

{
  "value": "admin@example.com",
  "password": "YourSecurePassword123!",
  "type": 1,
  "masterKey": "your-master-key"
}
```

### 2. Sign In

Use your credentials to sign in at `/login`

### 3. Explore Features

Once logged in, you have access to:

- **Dashboard** - Overview of your system
- **Users** - Manage user profiles
- **Roles** - Create and manage roles
- **Permissions** - Create and manage permissions
- **Settings** - Update your account

## 🎯 Common Tasks

### Create Permissions

1. Navigate to **Permissions** page
2. Click **Create Permission**
3. Add permission names (e.g., `user.view`, `user.edit`)
4. Click **Create Permissions**

### Create Roles

1. Navigate to **Roles** page
2. Click **Create Role**
3. Add role names (e.g., `Admin`, `Moderator`)
4. Click **Create Roles**

### Assign Permissions to Roles

1. Navigate to **Roles** page
2. Find the role you want to update
3. Click **Manage Permissions**
4. Select/deselect permissions
5. Click **Save Permissions**

### Change Password

1. Navigate to **Settings**
2. Under "Account Information"
3. Click **Change Password**
4. Enter current and new password
5. Click **Change Password**

### Update Username

1. Navigate to **Settings** or **Users**
2. Click **Update Username**
3. Enter new username
4. Click **Update Username**

## 🔑 API Authentication

All protected API calls require authentication:

```typescript
import { permissionsApi } from '@/services/api'

// The access token is automatically added from auth context
const { auth } = useAuth()

await permissionsApi.getPermissions(
  { pageIndex: 1, pageSize: 10 },
  auth.accessToken  // Required for authenticated endpoints
)
```

## 📦 Project Structure Overview

```
src/
├── services/api/     # All API calls
├── components/       # React components
├── pages/           # Page components
├── hooks/           # Custom hooks
├── auth/            # Authentication
└── lib/             # Utilities
```

## 🎨 UI Components

All UI components are in `src/components/ui/`:

```typescript
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Dialog } from '@/components/ui/dialog'
import { Pagination } from '@/components/ui/pagination'
import { LoadingState } from '@/components/ui/loading-state'
import { EmptyState } from '@/components/ui/empty-state'
```

## 🪝 Available Hooks

```typescript
// Authentication
import { useAuth } from '@/auth/use-auth'
const { auth, signIn, signOut, updateAuth } = useAuth()

// Token refresh (auto-refresh every 14 mins)
import { useTokenRefresh } from '@/hooks/use-token-refresh'
useTokenRefresh()

// Debounce values
import { useDebounce } from '@/hooks/use-debounce'
const debouncedValue = useDebounce(value, 500)

// Async state management
import { useAsync } from '@/hooks/use-async'
const { data, error, isLoading, execute } = useAsync(asyncFn)
```

## 🛠️ Development

### Build for Production

```bash
npm run build
```

Output will be in `dist/` directory.

### Preview Production Build

```bash
npm run preview
```

### Linting

```bash
npm run lint
```

## 🐛 Troubleshooting

### Port Already in Use

If port 3000 is already in use:

```bash
# Use a different port
npm run dev -- --port 3001
```

### API Connection Issues

1. Check if backend is running
2. Verify `VITE_API_BASE_URL` in `.env`
3. Check browser console for CORS errors
4. Ensure backend allows your frontend origin

### Token Expired

Tokens automatically refresh every 14 minutes. If you see unauthorized errors:

1. Sign out and sign in again
2. Check browser console for refresh errors
3. Verify refresh token is valid

### Build Errors

```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install

# Clear Vite cache
rm -rf .vite
```

## 📚 Learn More

- [Full Documentation](README_FRONTEND.md)
- [API Documentation](FRONTEND_API_DOCUMENTATION.md)
- [Implementation Guide](IMPLEMENTATION_GUIDE.md)
- [Changelog](CHANGELOG.md)

## 💡 Tips

1. **Use TypeScript** - All APIs are fully typed
2. **Check Console** - Development errors appear in browser console
3. **Use Network Tab** - Monitor API calls in browser DevTools
4. **Toast Notifications** - Success/error messages appear as toasts
5. **Auto-save** - Changes are saved immediately, no need to refresh

## 🎓 Example: Complete Flow

```typescript
// 1. Get auth context
const { auth } = useAuth()

// 2. Create permissions
await permissionsApi.createPermissions(
  { permissions: ['post.create', 'post.edit', 'post.delete'] },
  auth.accessToken
)

// 3. Create role
await rolesApi.createRoles(
  { roles: ['Content Manager'] },
  auth.accessToken
)

// 4. Get roles
const rolesResult = await rolesApi.getRoles(
  { pageIndex: 1, pageSize: 10 },
  auth.accessToken
)

// 5. Assign permissions to role
const contentManagerRole = rolesResult.items[0]
await rolesApi.addPermissionsForRole(
  {
    roleId: contentManagerRole.id,
    permissionIds: [/* permission IDs */]
  },
  auth.accessToken
)
```

## 🔐 Security Notes

- Never commit `.env` files with real credentials
- Use strong passwords (min 8 chars, uppercase, lowercase, number, special char)
- Tokens are stored in localStorage (cleared on logout)
- Always use HTTPS in production
- Keep dependencies updated

## 🎯 Next Steps

1. ✅ Set up your environment
2. ✅ Create a SuperAdmin account
3. ✅ Sign in
4. ✅ Create permissions
5. ✅ Create roles
6. ✅ Assign permissions to roles
7. 🎨 Customize the UI
8. 🚀 Deploy to production

---

**Need Help?** Check the [full documentation](README_FRONTEND.md) or review the [implementation guide](IMPLEMENTATION_GUIDE.md).

**Happy Coding! 🚀**
