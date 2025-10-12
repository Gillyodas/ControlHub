using ControlHub.Domain.Permissions;

namespace ControlHub.Infrastructure.Permissions
{
    public static class PermissionMapper
    {
        public static Permission ToDomain(PermissionEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return Permission.Rehydrate(
                entity.Id,
                entity.Code,
                entity.Description
            );
        }

        public static PermissionEntity ToEntity(Permission domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            return new PermissionEntity
            {
                Id = domain.Id,
                Code = domain.Code,
                Description = domain.Description
            };
        }
    }
}