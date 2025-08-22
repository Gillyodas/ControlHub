namespace ControlHub.Infrastructure.Persistence.Models
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public bool IsDeleted { get; set; }

        public Guid AccId { get; set; }

        // Navigation
        public AccountEntity Account { get; set; } = null!;
    }
}