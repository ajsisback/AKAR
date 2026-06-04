using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using MediatR;

namespace Akar.Application.Features.OwnerProfile;

public record GetOwnerProfileQuery(Guid OwnerId) : IRequest<OwnerProfileDto>;

public class GetOwnerProfileQueryHandler : IRequestHandler<GetOwnerProfileQuery, OwnerProfileDto>
{
    private readonly IOwnerRepository _ownerRepository;

    public GetOwnerProfileQueryHandler(IOwnerRepository ownerRepository)
    {
        _ownerRepository = ownerRepository;
    }

    public async Task<OwnerProfileDto> Handle(GetOwnerProfileQuery request, CancellationToken cancellationToken)
    {
        var owner = await _ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner == null)
        {
            throw new InvalidOperationException("OWNER_NOT_FOUND");
        }

        return new OwnerProfileDto(
            owner.Id,
            owner.FullName,
            owner.Email,
            owner.Phone,
            owner.CreatedAtUtc,
            owner.UpdatedAtUtc);
    }
}
