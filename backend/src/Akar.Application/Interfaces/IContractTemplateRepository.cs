using Akar.Domain.Entities;

namespace Akar.Application.Interfaces;

public interface IContractTemplateRepository
{
    Task<List<ContractTemplate>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ContractTemplate?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default);
}
