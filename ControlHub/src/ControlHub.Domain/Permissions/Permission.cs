

using System.Text.RegularExpressions;
using ControlHub.SharedKernel.Permissions;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Domain.Permissions
{
    public class Permission
    {
        public Guid Id { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }

        // Constructor private vẫn giữ nguyên, rất tốt!
        private Permission() { }

        private Permission(Guid id, string code, string description)
        {
            // Các validation cơ bản này có thể giữ ở đây
            // Hoặc chuyển vào hàm Create, tùy bạn
            Id = id;
            Code = code;
            Description = description;
        }

        // Factory methods
        // THAY ĐỔI 1: Chuyển từ "Permission" sang "Result<Permission>"
        public static Result<Permission> Create(Guid id, string code, string description)
        {
            if (id == Guid.Empty)
            {
                // THAY ĐỔI 2: Chuyển từ "throw" sang "return Result.Failure"
                return Result<Permission>.Failure(PermissionErrors.IdRequired);
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                // THAY ĐỔI 3:
                return Result<Permission>.Failure(PermissionErrors.PermissionCodeRequired);
            }

            var normalizedCode = code.Trim().ToLowerInvariant();

            var regexPattern = @"^[a-z0-9]+\.[a-z0-9]+$";

            if (!Regex.IsMatch(normalizedCode, regexPattern))
            {
                // THAY ĐỔI 4:
                return Result<Permission>.Failure(PermissionErrors.InvalidPermissionFormat);
            }

            var normalizedDescription = description?.Trim() ?? string.Empty;

            // THAY ĐỔI 5: Trả về Success
            return Result<Permission>.Success(new Permission(id, normalizedCode, normalizedDescription));
        }

        public static Permission Rehydrate(Guid id, string code, string description)
            => new Permission(id, code, description);

        // Behavior (Hàm Update của bạn đã làm đúng)
        public Result Update(string code, string description)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(PermissionErrors.PermissionCodeRequired);

            var regexPattern = @"^[a-z0-9]+\.[a-z0-9]+$";
            if (!Regex.IsMatch(code.Trim().ToLowerInvariant(), regexPattern))
            {
                return Result.Failure(PermissionErrors.InvalidPermissionFormat);
            }

            Code = code.Trim().ToLowerInvariant();
            Description = description?.Trim() ?? string.Empty;
            return Result.Success();
        }
    }
}