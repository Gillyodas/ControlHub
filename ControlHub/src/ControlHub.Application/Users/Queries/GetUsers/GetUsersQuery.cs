using ControlHub.Application.Users.DTOs;
using ControlHub.SharedKernel.Common;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Users.Queries.GetUsers
{
    public record GetUsersQuery(int Page, int PageSize, string? SearchTerm) : IRequest<Result<PaginatedResult<UserDto>>>;
}
