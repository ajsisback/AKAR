using Akar.Infrastructure.Seed;
using Microsoft.AspNetCore.Mvc;

namespace Akar.Api.Controllers;

/// <summary>
/// Development-only seed endpoint.
/// Returns 404 in non-Development environments.
/// WARNING: This endpoint is for local development and pilot testing only.
/// </summary>
[ApiController]
[Route("api/dev/seed")]
public class DevSeedController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IServiceProvider _serviceProvider;

    public DevSeedController(IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        _env = env;
        _serviceProvider = serviceProvider;
    }

    [HttpPost("pilot")]
    public async Task<IActionResult> SeedPilotData(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        var seeder = _serviceProvider.GetRequiredService<PilotDataSeeder>();
        await seeder.SeedAsync(cancellationToken);

        return Ok(new { message = "Pilot data seeded successfully." });
    }
}
