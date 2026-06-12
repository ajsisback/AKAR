using Akar.Application.DTOs;
using Akar.Application.Features.Admin;
using Akar.Infrastructure.Persistence;
using Akar.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.QueryHandlers;

public class ListOwnersForAdminQueryHandler : IRequestHandler<ListOwnersForAdminQuery, Result<List<AdminOwnerListItemDto>>>
{
    private readonly AkarDbContext _db;

    public ListOwnersForAdminQueryHandler(AkarDbContext db) => _db = db;

    public async Task<Result<List<AdminOwnerListItemDto>>> Handle(ListOwnersForAdminQuery request, CancellationToken cancellationToken)
    {
        var owners = await _db.Owners
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new AdminOwnerListItemDto(
                o.Id,
                o.FullName,
                o.Email,
                o.Phone,
                o.CreatedAtUtc,
                o.UpdatedAtUtc,
                _db.Projects.Count(p => p.OwnerId == o.Id)))
            .ToListAsync(cancellationToken);

        return Result<List<AdminOwnerListItemDto>>.Success(owners);
    }
}

public class GetOwnerForAdminQueryHandler : IRequestHandler<GetOwnerForAdminQuery, Result<AdminOwnerDetailDto>>
{
    private readonly AkarDbContext _db;

    public GetOwnerForAdminQueryHandler(AkarDbContext db) => _db = db;

    public async Task<Result<AdminOwnerDetailDto>> Handle(GetOwnerForAdminQuery request, CancellationToken cancellationToken)
    {
        var owner = await _db.Owners
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OwnerId, cancellationToken);

        if (owner is null)
        {
            return Result<AdminOwnerDetailDto>.Failure("OWNER_NOT_FOUND", "Owner not found");
        }

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == owner.Id)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new AdminOwnerProjectSummaryDto(
                p.Id,
                p.ProjectName,
                p.ProjectType.ToString(),
                p.CurrentStage.ToString(),
                p.City,
                p.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        var dto = new AdminOwnerDetailDto(
            owner.Id,
            owner.FullName,
            owner.Email,
            owner.Phone,
            owner.CreatedAtUtc,
            owner.UpdatedAtUtc,
            projects);

        return Result<AdminOwnerDetailDto>.Success(dto);
    }
}

public class ListProjectsForAdminQueryHandler : IRequestHandler<ListProjectsForAdminQuery, Result<List<AdminProjectListItemDto>>>
{
    private readonly AkarDbContext _db;

    public ListProjectsForAdminQueryHandler(AkarDbContext db) => _db = db;

    public async Task<Result<List<AdminProjectListItemDto>>> Handle(ListProjectsForAdminQuery request, CancellationToken cancellationToken)
    {
        var projects = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new AdminProjectListItemDto(
                p.Id,
                p.OwnerId,
                p.Owner.FullName,
                p.ProjectName,
                p.ProjectType.ToString(),
                p.CurrentStage.ToString(),
                p.City,
                p.CreatedAtUtc,
                p.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<List<AdminProjectListItemDto>>.Success(projects);
    }
}

public class GetProjectForAdminQueryHandler : IRequestHandler<GetProjectForAdminQuery, Result<AdminProjectDetailDto>>
{
    private readonly AkarDbContext _db;

    public GetProjectForAdminQueryHandler(AkarDbContext db) => _db = db;

    public async Task<Result<AdminProjectDetailDto>> Handle(GetProjectForAdminQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result<AdminProjectDetailDto>.Failure("PROJECT_NOT_FOUND", "Project not found");
        }

        var fileCount = await _db.ProjectFiles
            .CountAsync(f => f.ProjectId == project.Id && !f.IsDeleted, cancellationToken);

        var followerCount = await _db.ProjectFollowers
            .CountAsync(f => f.ProjectId == project.Id && !f.IsDeleted, cancellationToken);

        var contractCount = await _db.ProjectContracts
            .CountAsync(c => c.ProjectId == project.Id, cancellationToken);

        var timelineCount = await _db.ProjectTimelineEvents
            .CountAsync(t => t.ProjectId == project.Id && !t.IsDeleted, cancellationToken);

        var dto = new AdminProjectDetailDto(
            project.Id,
            project.OwnerId,
            project.Owner.FullName,
            project.ProjectName,
            project.ProjectType.ToString(),
            project.CurrentStage.ToString(),
            project.City,
            project.LocationText,
            project.CreatedAtUtc,
            project.UpdatedAtUtc,
            fileCount,
            followerCount,
            contractCount,
            timelineCount);

        return Result<AdminProjectDetailDto>.Success(dto);
    }
}
