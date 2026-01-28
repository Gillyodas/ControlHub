# UoW Handler Review Report

> **Date**: 2026-01-25  
> **Branch**: `refactor/uow-handler-improvements`  
> **Scope**: 17 CommandHandlers in Application layer

---

## Summary

| Severity | Count |
|----------|-------|
| 🔴 Critical | 3 |
| 🟠 Important | 3 |
| 🟡 Suggestion | 3 |
| 💡 Nitpick | 2 |

---

## 🔴 Critical Issues - Missing CancellationToken

| # | File | Line | Current Code | Fix |
|---|------|------|--------------|-----|
| 1 | `ResetPasswordCommandHandler.cs` | 91 | `await _uow.CommitAsync();` | `await _uow.CommitAsync(cancellationToken);` |
| 2 | `ToggleIdentifierActiveCommandHandler.cs` | 55 | `await _unitOfWork.CommitAsync();` | `await _unitOfWork.CommitAsync(cancellationToken);` |
| 3 | `UpdateIdentifierConfigCommandHandler.cs` | 70 | `await _unitOfWork.CommitAsync();` | `await _unitOfWork.CommitAsync(cancellationToken);` |

**Why Critical**: Không truyền `CancellationToken` có thể dẫn đến:
- Request đã bị cancel nhưng transaction vẫn tiếp tục
- Không graceful shutdown được
- Resource leak potential

---

## 🟠 Important Issues - Log Success Before Commit

| # | File | Log Line | Commit Line | Issue |
|---|------|----------|-------------|-------|
| 4 | `RegisterAdminCommandHandler.cs` | 84-87 | 89 | Log success trước commit |
| 5 | `RegisterSupperAdminCommandHandler.cs` | 104-107 | 109 | Log success trước commit |
| 6 | `RegisterUserCommandHandler.cs` | 84-87 | 89 | Log success trước commit |

**Why Important**: Nếu `CommitAsync()` fail sau khi log success → misleading logs, gây khó debug.

**Fix**: Di chuyển log success xuống sau `CommitAsync()`.

---

## 🟡 Suggestions - Code Quality

| # | File | Line | Issue | Fix |
|---|------|------|-------|-----|
| 7 | `RegisterSupperAdminCommandHandler.cs` | 20, 28 | Wrong logger type: `ILogger<RegisterUserCommandHandler>` | Change to `ILogger<RegisterSupperAdminCommandHandler>` |
| 8 | `ToggleIdentifierActiveCommandHandler.cs` | 5 | Duplicate using: `using ControlHub.SharedKernel.Results;` appears twice | Remove duplicate |
| 9 | `CreatePermissionsCommandHandler.cs` | 32 | Debug log: `"--- DEBUG: CreatePermissionsCommandHandler.Handle HIT ---"` | Remove debug log |

---

## 💡 Nitpicks - Naming Consistency

| # | File | Current | Suggested |
|---|------|---------|-----------|
| 10 | `ToggleIdentifierActiveCommandHandler.cs` | `_unitOfWork` | `_uow` |
| 11 | `UpdateIdentifierConfigCommandHandler.cs` | `_unitOfWork` | `_uow` |

**Rationale**: Hầu hết handlers khác đều dùng `_uow` → nên thống nhất.

---

## ✅ Completed Fixes

- [x] Remove `DateTime.UtcNow` from `UnitOfWork.cs` log messages (5 occurrences)

---

## 📋 Action Checklist

- [X] Fix Critical #1: `ResetPasswordCommandHandler.cs`
- [X] Fix Critical #2: `ToggleIdentifierActiveCommandHandler.cs`
- [X] Fix Critical #3: `UpdateIdentifierConfigCommandHandler.cs`
- [x] Fix Important #4: `RegisterAdminCommandHandler.cs`
- [x] Fix Important #5: `RegisterSupperAdminCommandHandler.cs`
- [x] Fix Important #6: `RegisterUserCommandHandler.cs`
- [x] Fix Suggestion #7: Wrong logger type
- [x] Fix Suggestion #8: Duplicate using
- [x] Fix Suggestion #9: Remove debug log
- [ ] Fix Nitpick #10-11: Rename `_unitOfWork` → `_uow`
