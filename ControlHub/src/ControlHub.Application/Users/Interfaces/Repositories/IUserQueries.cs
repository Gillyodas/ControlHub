using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlHub.Domain.Users;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Application.Users.Interfaces.Repositories
{
    public interface IUserQueries
    {
        public Task<User> GetByAccountId(Guid id, CancellationToken cancellationToken);
    }
}
