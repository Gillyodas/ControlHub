using ControlHub.Domain.Accounts.Enums;
using ControlHub.Domain.Accounts.ValueObjects;
using ControlHub.Domain.Roles;
using ControlHub.Domain.Users;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Users;

namespace ControlHub.Domain.Accounts
{
    public class Account
    {
        public Guid Id { get; private set; }
        public Password Password { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }

        // Quan hệ 1-nhiều (Account - Role)
        public Guid RoleId { get; private set; }
        public Maybe<Role> Role { get; private set; } = Maybe<Role>.None;

        private readonly List<Identifier> _identifiers = new();
        public IReadOnlyCollection<Identifier> Identifiers => _identifiers.AsReadOnly();

        public Maybe<User> User { get; private set; }

        private Account() { }

        private Account(Guid id, Password pass, Guid roleId, bool isActive, bool isDeleted, Maybe<User> user, Maybe<Role> role)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id is required", nameof(id));
            if (roleId == Guid.Empty) throw new ArgumentException("RoleId is required", nameof(roleId));

            Id = id;
            Password = pass;
            RoleId = roleId;
            IsActive = isActive;
            IsDeleted = isDeleted;
            User = user;
            Role = role;
        }

        public static Account Create(Guid id, Password pass, Guid roleId)
            => new Account(id, pass, roleId, true, false, Maybe<User>.None, Maybe<Role>.None);

        public static Account Rehydrate(
            Guid id,
            Password pass,
            Guid roleId,
            bool isActive,
            bool isDeleted,
            Maybe<User> user,
            Maybe<Role> role,
            IEnumerable<Identifier> identifiers)
        {
            var acc = new Account(id, pass, roleId, isActive, isDeleted, user, role);
            acc._identifiers.AddRange(identifiers);
            return acc;
        }

        // Behaviors
        public Result AddIdentifier(Identifier identifier)
        {
            if (_identifiers.Any(i => i.Type == identifier.Type && i.NormalizedValue == identifier.NormalizedValue))
                return Result.Failure(AccountErrors.IdentifierAlreadyExists);

            _identifiers.Add(identifier);
            return Result.Success();
        }

        public Result RemoveIdentifier(IdentifierType type, string normalized)
        {
            var found = _identifiers.FirstOrDefault(i => i.Type == type && i.NormalizedValue == normalized);
            if (found == null) return Result.Failure(AccountErrors.IdentifierNotFound);

            _identifiers.Remove(found);
            return Result.Success();
        }

        public Result AttachUser(User user)
        {
            if (user == null)
                return Result.Failure(UserErrors.Required);

            if (User.HasValue)
                return Result.Failure(UserErrors.AlreadyAtached);

            User = Maybe<User>.From(user);
            return Result.Success();
        }

        public Result AttachRole(Role role)
        {
            if (role == null)
                return Result.Failure(AccountErrors.RoleRequired);

            Role = Maybe<Role>.From(role);
            RoleId = role.Id;
            return Result.Success();
        }

        public void Deactivate() => IsActive = false;

        public void Delete()
        {
            IsDeleted = true;
            User.Match(
                some: u => u.Delete(),
                none: () => { }
            );
        }

        public Result UpdatePassword(Password newPass)
        {
            if (newPass is null)
                return Result.Failure(AccountErrors.PasswordRequired);

            if (!newPass.IsValid())
                return Result.Failure(AccountErrors.PasswordIsNotValid);

            Password = newPass;
            return Result.Success();
        }
    }
}