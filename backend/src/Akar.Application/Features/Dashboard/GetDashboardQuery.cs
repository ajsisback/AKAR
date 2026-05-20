using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Enums;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Dashboard;

public record GetDashboardQuery(Guid OwnerId) : IRequest<Result<DashboardDto>>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetDashboardQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var total = await _projectRepository.CountByOwnerIdAsync(request.OwnerId, cancellationToken);
        var notStarted = await _projectRepository.CountByOwnerIdAndStageAsync(request.OwnerId, CurrentStage.NotStarted, cancellationToken);
        var structural = await _projectRepository.CountByOwnerIdAndStageAsync(request.OwnerId, CurrentStage.Structural, cancellationToken);
        var finishing = await _projectRepository.CountByOwnerIdAndStageAsync(request.OwnerId, CurrentStage.Finishing, cancellationToken);
        var completed = await _projectRepository.CountByOwnerIdAndStageAsync(request.OwnerId, CurrentStage.Completed, cancellationToken);

        return Result<DashboardDto>.Success(new DashboardDto(total, notStarted, structural, finishing, completed));
    }
}
