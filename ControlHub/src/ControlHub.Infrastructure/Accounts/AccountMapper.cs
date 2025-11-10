using ControlHub.Domain.Accounts;
using ControlHub.Domain.Accounts.ValueObjects;
using ControlHub.Domain.Roles;
using ControlHub.Domain.Users;
using ControlHub.Infrastructure.Roles;
using ControlHub.Infrastructure.Users;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Infrastructure.Accounts
{
    public static class AccountMapper
    {
        public static Account ToDomain(AccountEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Map User (nếu có)
            var user = entity.User != null
                ? Maybe<User>.From(new User(entity.User.Id, entity.User.AccId, entity.User.Username))
                : Maybe<User>.None;

            // Map Password
            var password = Password.From(entity.HashPassword, entity.Salt);

            // Map Role (nếu có)
            var role = entity.Role != null
                ? Maybe<Role>.From(RoleMapper.ToDomain(entity.Role))
                : Maybe<Role>.None;

            // Map Identifiers
            var identifiers = entity.Identifiers
                .Select(IdentifierMapper.ToDomain)
                .ToList();

            return Account.Rehydrate(
                entity.Id,
                password,
                entity.RoleId,
                entity.IsActive,
                entity.IsDeleted,
                user,
                role,
                identifiers
            );
        }

        public static AccountEntity ToEntity(Account domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            return new AccountEntity
            {
                Id = domain.Id,
                HashPassword = domain.Password.Hash,
                Salt = domain.Password.Salt,
                IsActive = domain.IsActive,
                IsDeleted = domain.IsDeleted,
                RoleId = domain.RoleId,

                // Nếu Role có giá trị, ánh xạ về RoleEntity
                Role = domain.Role.Match(
                    some: r => RoleMapper.ToEntity(r),
                    none: () => null!
                ),

                Identifiers = domain.Identifiers
                    .Select(i => IdentifierMapper.ToEntity(i, domain.Id))
                    .ToList(),

                // Nếu User có giá trị, ánh xạ về UserEntity
                User = domain.User.Match(
                    some: u => new UserEntity
                    {
                        Id = u.Id,
                        AccId = u.AccId,
                        Username = u.Username
                    },
                    none: () => null!
                )
            };
        }
    }
}