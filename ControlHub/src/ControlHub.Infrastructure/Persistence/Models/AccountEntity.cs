using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlHub.Infrastructure.Persistence.Models
{
    public class AccountEntity
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public byte[] HashPassword { get; set; } = null!;
        public byte[] Salt { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation
        public UserEntity? User { get; set; }
    }
}
