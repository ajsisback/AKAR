using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ContractTemplateRepository : IContractTemplateRepository
{
    private readonly AkarDbContext _db;

    public ContractTemplateRepository(AkarDbContext db) => _db = db;

    public async Task<List<ContractTemplate>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        await _db.ContractTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.TemplateCode)
            .ToListAsync(cancellationToken);

    public async Task<ContractTemplate?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default) =>
        await _db.ContractTemplates.FindAsync(new object[] { templateId }, cancellationToken);
}
