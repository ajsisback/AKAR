using Akar.Application.DTOs;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Admin;

/// <summary>
/// Admin read-only query definitions.
/// Handlers are in Akar.Infrastructure where DbContext access is available.
/// </summary>

public record ListOwnersForAdminQuery() : IRequest<Result<List<AdminOwnerListItemDto>>>;

public record GetOwnerForAdminQuery(Guid OwnerId) : IRequest<Result<AdminOwnerDetailDto>>;

public record ListProjectsForAdminQuery() : IRequest<Result<List<AdminProjectListItemDto>>>;

public record GetProjectForAdminQuery(Guid ProjectId) : IRequest<Result<AdminProjectDetailDto>>;
