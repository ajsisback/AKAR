using Akar.Application.Interfaces;
using Akar.Infrastructure.Persistence;
using Akar.Infrastructure.Repositories;
using Akar.Infrastructure.Services;
using Akar.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Akar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<AkarDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectFolderRepository, ProjectFolderRepository>();
        services.AddScoped<IProjectFileRepository, ProjectFileRepository>();
        services.AddScoped<IProjectFollowerRepository, ProjectFollowerRepository>();
        services.AddScoped<IFollowerUploadLinkRepository, FollowerUploadLinkRepository>();

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
