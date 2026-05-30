using Akar.Domain.Entities;

namespace Akar.Application.Interfaces;

/// <summary>
/// Generates contract PDF bytes. Implementation in Infrastructure layer.
/// </summary>
public interface IContractPdfGenerator
{
    /// <summary>
    /// Generates a PDF document for the given contract.
    /// Returns the raw PDF bytes.
    /// </summary>
    byte[] Generate(ProjectContract contract, Project project, Owner owner);
}
