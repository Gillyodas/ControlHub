# Implementation Plan: Application Layer Gaps

**Author:** Agent "Antigravity"  
**Date:** 2026-01-29  
**Based on:** `ControlHub_Feature_Gap_Analysis.md` and `.agent/skills/dotnet-backend-patterns`

## 1. Architectural Strategy

We will follow the existing **Clean Architecture** patterns observed in `ControlHub.Application`:

-   **Pattern**: CQRS (Command Query Responsibility Segregation) using **MediatR**.
-   **Folder Structure**: Feature-based (`Users`, `Roles`, `Permissions`, `Identifiers`, `Profile`).
    -   `Commands/{CommandName}/`: Contains `Command`, `Handler`, `Validator`.
    -   `Queries/{QueryName}/`: Contains `Query`, `Handler`, `Validator`, `DTO`.
-   **Result Pattern**: All handlers must return `Result<T>` or `Result` from `ControlHub.SharedKernel.Results`.
-   **Data Access**:
    -   **Commands**: Use `IRepository` interfaces (e.g., `IUserRepository`, `IRoleRepository`) and `IUnitOfWork`.
    -   **Queries**: Use `I{Feature}Queries` interfaces (e.g., `IUserQueries`) which may use Dapper or EF Core AsNoTracking. (Ref: `RoleQueries`, `IRoleQueries`).
-   **Validation**: Use `FluentValidation` with naming convention `{CommandName}Validator`.
-   **Mapping**: Use `Mapster` or Manual Mapping (as seen in `CreateRolesCommandHandler` utilizing Domain Services or manual construction). *Note: Project seems to use manual mapping or hidden mapper, we will stick to manual or consistent existing pattern.*

## 2. Detailed Task Breakdown

### 2.1 User Management (Priority: High)
*(Location: `ControlHub.Application/Users`)*

#### Task 2.1.1: Implement Update User Command
- [ ] Create folder `Users/Commands/UpdateUser`
- [ ] Create `UpdateUserCommand.cs` (Properties: `Id`, `Email`, `FirstName`, `LastName`, `PhoneNumber`, `IsActive`)
- [ ] Create `UpdateUserValidator.cs` (Validate: `Id` required, `Email` format)
- [ ] Create `UpdateUserCommandHandler.cs`
    -   [ ] Inject `IUserRepository`, `IUnitOfWork`
    -   [ ] Logic: Fetch user, update properties, `_uow.CommitAsync`
    -   [ ] Return `Result<UserDto>` or `Result<Unit>`

#### Task 2.1.2: Implement Delete User Command (Soft Delete)
- [ ] Create folder `Users/Commands/DeleteUser`
- [ ] Create `DeleteUserCommand.cs` (Properties: `Id`)
- [ ] Create `DeleteUserValidator.cs`
- [ ] Create `DeleteUserCommandHandler.cs`
    -   [ ] Inject `IUserRepository`, `IUnitOfWork`
    -   [ ] Logic: Fetch user, set `IsDeleted = true` (or call `repo.DeleteAsync` if soft delete is handled inside), `_uow.CommitAsync`
    -   [ ] Return `Result<Unit>`

#### Task 2.1.3: Implement Get User By ID Query
- [ ] Create folder `Users/Queries/GetUserById`
- [ ] Create `GetUserByIdQuery.cs` (Properties: `Id`)
- [ ] Create `GetUserByIdQueryHandler.cs`
    -   [ ] Inject `IUserQueries` (Need to verify/create interface if missing)
    -   [ ] Logic: Call `_userQueries.GetByIdAsync(request.Id)`
    -   [ ] Return `Result<UserDto>`

#### Task 2.1.4: Implement Get Users Query (Paginated)
- [ ] Create folder `Users/Queries/GetUsers`
- [ ] Create `GetUsersQuery.cs` (Properties: `Page`, `PageSize`, `SearchTerm`)
- [ ] Create `GetUsersQueryHandler.cs`
    -   [ ] Inject `IUserQueries`
    -   [ ] Logic: Call `_userQueries.GetPaginatedAsync(...)`
    -   [ ] Return `Result<PaginatedResult<UserDto>>`

---

### 2.2 Role Assignment (Priority: High)
*(Location: `ControlHub.Application/Roles`)*

#### Task 2.2.1: Implement Assign Role To User Command
- [ ] Create folder `Roles/Commands/AssignRoleToUser`
- [ ] Create `AssignRoleToUserCommand.cs` (Properties: `UserId`, `RoleId`)
- [ ] Create `AssignRoleToUserValidator.cs`
- [ ] Create `AssignRoleToUserCommandHandler.cs`
    -   [ ] Inject `IUserRepository`, `IRoleRepository`, `IUnitOfWork`
    -   [ ] Logic: Load User, Load Role, `user.AddRole(role)`, `_uow.CommitAsync`
    -   [ ] Return `Result<Unit>`

#### Task 2.2.2: Implement Remove Role From User Command
- [ ] Create folder `Roles/Commands/RemoveRoleFromUser`
- [ ] Create `RemoveRoleFromUserCommand.cs` (Properties: `UserId`, `RoleId`)
- [ ] Create `RemoveRoleFromUserCommandHandler.cs`
    -   [ ] Logic: Load User, `user.RemoveRole(roleId)`, `_uow.CommitAsync`

#### Task 2.2.3: Implement Get User Roles Query
- [ ] Create folder `Roles/Queries/GetUserRoles`
- [ ] Create `GetUserRolesQuery.cs` (Properties: `UserId`)
- [ ] Create `GetUserRolesQueryHandler.cs`
    -   [ ] Inject `IRoleQueries`
    -   [ ] Logic: `_roleQueries.GetRolesByUserIdAsync(request.UserId)`
    -   [ ] Return `Result<List<RoleDto>>`

---

### 2.3 Role Management Operations (Priority: Medium)
*(Location: `ControlHub.Application/Roles`)*

#### Task 2.3.1: Implement Update Role Command
- [ ] Create folder `Roles/Commands/UpdateRole`
- [ ] Create `UpdateRoleCommand.cs` (Properties: `Id`, `Name`, `Description`)
- [ ] Create `UpdateRoleCommandHandler.cs`
    -   [ ] Update Role entity, handle duplicate name check if name changes.

#### Task 2.3.2: Implement Delete Role Command
- [ ] Create folder `Roles/Commands/DeleteRole`
- [ ] Create `DeleteRoleCommand.cs` (Properties: `Id`)
- [ ] Create `DeleteRoleCommandHandler.cs`
    -   [ ] Validate Role is not assigned to users (optional or cascade), Soft delete.

---

### 2.4 Permission Management Operations (Priority: Low)
*(Location: `ControlHub.Application/Permissions`)*

#### Task 2.4.1: Implement Update Permission Command
- [ ] Create folder `Permissions/Commands/UpdatePermission`
- [ ] Create `UpdatePermissionCommand.cs`
- [ ] Create `UpdatePermissionCommandHandler.cs`

#### Task 2.4.2: Implement Delete Permission Command
- [ ] Create folder `Permissions/Commands/DeletePermission`
- [ ] Create `DeletePermissionCommand.cs`
- [ ] Create `DeletePermissionCommandHandler.cs`

---

### 2.5 Profile Management (Priority: Medium)
*(Location: `ControlHub.Application/Users` or `ControlHub.Application/Profile`)*

#### Task 2.5.1: Implement Get My Profile Query
- [ ] Create folder `Users/Queries/GetMyProfile`
- [ ] Create `GetMyProfileQuery.cs` (Uses `IUserContext` or similar to get current ID)
- [ ] Create `GetMyProfileQueryHandler.cs`

#### Task 2.5.2: Implement Update My Profile Command
- [ ] Create folder `Users/Commands/UpdateMyProfile`
- [ ] Create `UpdateMyProfileCommand.cs`
- [ ] Create `UpdateMyProfileCommandHandler.cs`

---

### 2.6 Identifier Configuration (Priority: Low)
*(Location: `ControlHub.Application/Identifiers`)*

#### Task 2.6.1: Implement Delete Identifier Config Command
- [ ] Create folder `Identifiers/Commands/DeleteIdentifierConfig`
- [ ] Create `DeleteIdentifierConfigCommand.cs`
- [ ] Create `DeleteIdentifierConfigCommandHandler.cs`

## 3. Infrastructure Updates (Repositories)
*Note: Ensure these methods exist in Repositories/Queries interfaces/implementations. If not, add them.*

#### Task 3.1: Update IUserRepository / UserRepository
- [ ] Verify `Update` method exists (EF Core `Update` or `SetModified`).
- [ ] Verify `Delete` method exists (Soft delete).

#### Task 3.2: Update IUserQueries / UserQueries (Dapper/EF)
- [ ] Add `GetByIdAsync(Guid id)`
- [ ] Add `GetPaginatedAsync(int page, int pageSize, string searchTerm)`

#### Task 3.3: Update IRoleRepository
- [ ] Ensure methods for `Update`, `Delete`.

#### Task 3.4: Update IRoleQueries
- [ ] Add `GetRolesByUserIdAsync(Guid userId)`
