using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Users;

namespace ControlHub.Domain.Users
{
    public class User
    {
        // Properties
        public Guid Id { get; private set; }
        public string? Username { get; private set; }
        public bool IsDeleted { get; private set; }

        // Foreign Key & Navigation
        public Guid AccId { get; private set; }

        // Navigation property về Account (Aggregate Root cha nếu User nằm trong Account Aggregate)
        // Hoặc chỉ là reference nếu User là Aggregate riêng biệt (tùy thiết kế của bạn)
        // Ở đây tôi khai báo nó, nhưng EF Core sẽ set nó.
        // public Account Account { get; private set; } = null!; 

        // Constructor rỗng cho EF Core
        private User() { }

        public User(Guid id, Guid accId, string? username = null)
        {
            if (id == Guid.Empty) throw new ArgumentException("User Id is required", nameof(id));
            if (accId == Guid.Empty) throw new ArgumentException("Account Id is required", nameof(accId));

            Id = id;
            AccId = accId;
            Username = username;
            IsDeleted = false;
        }

        // Behavior
        public void Delete() => IsDeleted = true;

        public void SetUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));
            Username = username;
        }

        public Result UpdateUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Result.Failure(UserErrors.Required);

            Username = username;

            return Result.Success();
        }
    }
}