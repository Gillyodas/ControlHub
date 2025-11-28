using AutoMapper;
using ControlHub.Application.Roles.DTOs;
using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.Domain.Roles;
using ControlHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControlHub.Infrastructure.RolePermissions.Repositories
{
    public class RolePermissionQuery : IRolePermissionQueries
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public RolePermissionQuery(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
    }
}
