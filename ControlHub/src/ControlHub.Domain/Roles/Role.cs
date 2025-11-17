using ControlHub.Domain.Permissions;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Roles;

namespace ControlHub.Domain.Roles
{
    public class Role
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<Permission> _permissions = new();
        public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

        private Role() { }

        private Role(Guid id, string name, string description, bool isActive)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is required", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));

            Id = id;
            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            IsActive = isActive;
        }

        // Factory methods
        public static Role Create(Guid id, string name, string description)
            => new Role(id, name, description, true);

        public static Role Rehydrate(Guid id, string name, string description, bool isActive, IEnumerable<Permission> permissions)
        {
            var role = new Role(id, name, description, isActive);
            role._permissions.AddRange(permissions);
            return role;
        }

        // Behavior
        public Result Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(RoleErrors.RoleNameRequired);

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            return Result.Success();
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        public void Delete()
        {
            IsActive = false;
            _permissions.Clear();
        }

        public Result AddPermission(Permission permission)
        {
            if (_permissions.Any(p => p.Code == permission.Code))
                return Result.Failure(RoleErrors.PermissionAlreadyExists);

            _permissions.Add(permission);
            return Result.Success();
        }

        public Result<PartialResult<Permission, string>> AddRangePermission(IEnumerable<Permission> permissionsToAdd)
        {
            var successes = new List<Permission>();
            var failures = new List<string>();

            var existingCodes = _permissions.Select(p => p.Code).ToHashSet();

            foreach (var per in permissionsToAdd)
            {
                if (existingCodes.Contains(per.Code))
                {
                    failures.Add($"{per.Code}: is already exist in role: {this.Name}");
                }
                else
                {
                    _permissions.Add(per);
                    successes.Add(per);

                    existingCodes.Add(per.Code);
                }
            }

            var partial = PartialResult<Permission, string>.Create(successes, failures);

            if (!partial.Successes.Any() && partial.Failures.Any())
            {
                return Result<PartialResult<Permission, string>>.Failure(RoleErrors.AllPermissionsAlreadyExist);
            }

            return Result<PartialResult<Permission, string>>.Success(partial);
        }

        public Result RemovePermission(string code)
        {
            var found = _permissions.FirstOrDefault(p => p.Code == code);
            if (found == null)
                return Result.Failure(RoleErrors.PermissionNotFound);

            _permissions.Remove(found);
            return Result.Success();
        }
    }
}