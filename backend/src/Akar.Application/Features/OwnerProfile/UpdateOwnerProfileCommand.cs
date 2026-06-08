using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using MediatR;

namespace Akar.Application.Features.OwnerProfile;

public record UpdateOwnerProfileCommand(Guid OwnerId, string FullName, string? Phone) : IRequest<OwnerProfileDto>;

public class UpdateOwnerProfileCommandHandler : IRequestHandler<UpdateOwnerProfileCommand, OwnerProfileDto>
{
    private readonly IOwnerRepository _ownerRepository;

    public UpdateOwnerProfileCommandHandler(IOwnerRepository ownerRepository)
    {
        _ownerRepository = ownerRepository;
    }

    public async Task<OwnerProfileDto> Handle(UpdateOwnerProfileCommand request, CancellationToken cancellationToken)
    {
        var owner = await _ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner == null)
        {
            throw new InvalidOperationException("OWNER_NOT_FOUND");
        }

        owner.UpdateProfile(request.FullName, request.Phone);

        await _ownerRepository.SaveChangesAsync(cancellationToken);

        return new OwnerProfileDto(
            owner.Id,
            owner.FullName,
            owner.Email,
            owner.Phone,
            owner.CreatedAtUtc,
            owner.UpdatedAtUtc);
    }
}
