using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace Akar.Infrastructure.Seed;

/// <summary>
/// Idempotent pilot seed data for Development environment only.
/// Creates admin users, a pilot owner, demo project with followers, timeline events, and a draft contract.
/// WARNING: Seed credentials are for development/pilot only — never use in production.
/// </summary>
public class PilotDataSeeder
{
    private readonly AkarDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<PilotDataSeeder> _logger;

    public PilotDataSeeder(
        AkarDbContext db,
        IPasswordHasher passwordHasher,
        ILogger<PilotDataSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting pilot data seed...");

        // --- Admin Users ---
        var superAdmin = await EnsureAdminAsync(
            "superadmin@akar.local", "Admin@12345", "مدير النظام", AdminRole.SuperAdmin, cancellationToken);
        var supportAdmin = await EnsureAdminAsync(
            "support@akar.local", "Support@12345", "مسؤول الدعم", AdminRole.SupportAdmin, cancellationToken);

        // --- Pilot Owner ---
        var pilotOwner = await EnsurePilotOwnerAsync(cancellationToken);

        // --- Pilot Project ---
        var project = await EnsurePilotProjectAsync(pilotOwner.Id, cancellationToken);

        // --- System Folders ---
        await EnsureSystemFoldersAsync(project.Id, pilotOwner.Id, cancellationToken);

        // --- Followers ---
        var followersInbox = await _db.ProjectFolders
            .FirstOrDefaultAsync(f => f.ProjectId == project.Id && f.FolderType == FolderType.FollowersInbox, cancellationToken);

        if (followersInbox is not null)
        {
            await EnsureFollowerAsync(project.Id, pilotOwner.Id, followersInbox.Id,
                "أحمد المشرف", "0501111111", FollowerType.Supervisor, cancellationToken);
            await EnsureFollowerAsync(project.Id, pilotOwner.Id, followersInbox.Id,
                "مؤسسة البناء الحديث", "0502222222", FollowerType.Contractor, cancellationToken);
            await EnsureFollowerAsync(project.Id, pilotOwner.Id, followersInbox.Id,
                "مكتب التصميم الهندسي", "0503333333", FollowerType.EngineeringOffice, cancellationToken);
        }

        // --- Timeline Events ---
        await EnsureTimelineEventsAsync(project.Id, pilotOwner.Id, project.CurrentStage, cancellationToken);

        // --- Draft Contract ---
        await EnsureDraftContractAsync(project.Id, pilotOwner.Id, cancellationToken);

        // --- Upload Link ---
        await EnsureUploadLinkAsync(project.Id, pilotOwner.Id, cancellationToken);

        _logger.LogInformation("Pilot data seed completed successfully.");
    }

    private async Task<AdminUser> EnsureAdminAsync(
        string email, string password, string fullName, AdminRole role, CancellationToken ct)
    {
        var existing = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Email == email.ToLowerInvariant(), ct);
        if (existing is not null)
        {
            _logger.LogInformation("Admin {Email} already exists, skipping.", email);
            return existing;
        }

        var hash = _passwordHasher.Hash(password);
        var admin = AdminUser.Create(fullName, email, hash, role);
        await _db.AdminUsers.AddAsync(admin, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created admin {Email} with role {Role}.", email, role);
        return admin;
    }

    private async Task<Owner> EnsurePilotOwnerAsync(CancellationToken ct)
    {
        const string email = "pilot.owner@akar.local";
        var existing = await _db.Owners.FirstOrDefaultAsync(o => o.Email == email.ToLowerInvariant(), ct);
        if (existing is not null)
        {
            _logger.LogInformation("Pilot owner {Email} already exists, skipping.", email);
            return existing;
        }

        var hash = _passwordHasher.Hash("Pilot@12345");
        var owner = Owner.Create("مالك تجربة", email, "0500000000", hash);
        await _db.Owners.AddAsync(owner, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created pilot owner {Email}.", email);
        return owner;
    }

    private async Task<Project> EnsurePilotProjectAsync(Guid ownerId, CancellationToken ct)
    {
        const string projectName = "فيلا حي النرجس";
        var existing = await _db.Projects.FirstOrDefaultAsync(
            p => p.OwnerId == ownerId && p.ProjectName == projectName, ct);
        if (existing is not null)
        {
            _logger.LogInformation("Pilot project '{Name}' already exists, skipping.", projectName);
            return existing;
        }

        var project = Project.Create(
            ownerId,
            projectName,
            ProjectType.Villa,
            city: "الرياض",
            locationText: "حي النرجس",
            mapLink: null,
            currentStage: CurrentStage.Structural);

        await _db.Projects.AddAsync(project, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created pilot project '{Name}'.", projectName);
        return project;
    }

    private async Task EnsureSystemFoldersAsync(Guid projectId, Guid ownerId, CancellationToken ct)
    {
        var hasSystemFolders = await _db.ProjectFolders.AnyAsync(
            f => f.ProjectId == projectId && f.IsSystemFolder, ct);
        if (hasSystemFolders)
        {
            _logger.LogInformation("System folders already exist for project, skipping.");
            return;
        }

        var folders = ProjectFolder.DefaultSystemFolders
            .Select(f => ProjectFolder.CreateSystemFolder(projectId, ownerId, f.Type, f.Name))
            .ToList();

        await _db.ProjectFolders.AddRangeAsync(folders, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created {Count} system folders for pilot project.", folders.Count);
    }

    private async Task EnsureFollowerAsync(
        Guid projectId, Guid ownerId, Guid followersInboxId,
        string fullName, string phone, FollowerType type, CancellationToken ct)
    {
        var exists = await _db.ProjectFollowers.AnyAsync(
            f => f.ProjectId == projectId && f.Phone == phone && !f.IsDeleted, ct);
        if (exists)
        {
            _logger.LogInformation("Follower {Phone} already exists, skipping.", phone);
            return;
        }

        // Create follower inbox sub-folder
        var inboxFolder = ProjectFolder.CreateCustomFolder(
            projectId, ownerId, fullName, followersInboxId);
        await _db.ProjectFolders.AddAsync(inboxFolder, ct);
        await _db.SaveChangesAsync(ct);

        var follower = ProjectFollower.Create(
            projectId, ownerId, inboxFolder.Id, fullName, phone, type, notes: null);
        await _db.ProjectFollowers.AddAsync(follower, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created follower {Name} ({Type}).", fullName, type);
    }

    private async Task EnsureTimelineEventsAsync(
        Guid projectId, Guid ownerId, CurrentStage currentStage, CancellationToken ct)
    {
        var hasEvents = await _db.ProjectTimelineEvents.AnyAsync(
            t => t.ProjectId == projectId, ct);
        if (hasEvents)
        {
            _logger.LogInformation("Timeline events already exist, skipping.");
            return;
        }

        var events = new List<ProjectTimelineEvent>
        {
            // Stage changed to Structural
            ProjectTimelineEvent.CreateStageChanged(projectId, ownerId, currentStage, "تم تعيين المرحلة الإنشائية"),

            // Follower added events
            ProjectTimelineEvent.CreateSystemEvent(
                projectId, ownerId, currentStage,
                TimelineEventType.FollowerAdded, TimelineSourceType.None, Guid.Empty,
                "تمت إضافة متابع: أحمد المشرف", "مشرف المشروع"),

            ProjectTimelineEvent.CreateSystemEvent(
                projectId, ownerId, currentStage,
                TimelineEventType.FollowerAdded, TimelineSourceType.None, Guid.Empty,
                "تمت إضافة متابع: مؤسسة البناء الحديث", "مقاول"),

            ProjectTimelineEvent.CreateSystemEvent(
                projectId, ownerId, currentStage,
                TimelineEventType.FollowerAdded, TimelineSourceType.None, Guid.Empty,
                "تمت إضافة متابع: مكتب التصميم الهندسي", "مكتب هندسي"),

            // Manual UAT note
            ProjectTimelineEvent.CreateManualNote(
                projectId, ownerId, currentStage,
                "ملاحظة UAT تجريبية",
                "هذه ملاحظة تجريبية لسيناريو UAT.")
        };

        await _db.ProjectTimelineEvents.AddRangeAsync(events, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created {Count} timeline events.", events.Count);
    }

    private async Task EnsureDraftContractAsync(Guid projectId, Guid ownerId, CancellationToken ct)
    {
        var hasContract = await _db.ProjectContracts.AnyAsync(
            c => c.ProjectId == projectId, ct);
        if (hasContract)
        {
            _logger.LogInformation("Draft contract already exists, skipping.");
            return;
        }

        var template = await _db.ContractTemplates.FirstOrDefaultAsync(ct2 => true, ct);
        if (template is null)
        {
            _logger.LogWarning("No contract templates found. Skipping draft contract seed. Run ContractTemplateSeeder first.");
            return;
        }

        var contractData = new
        {
            scopeOfWork = "أعمال الهيكل الإنشائي",
            paymentTerms = "دفعات حسب مراحل الإنجاز",
            ownerObligations = "توفير المخططات المعتمدة",
            contractorObligations = "تنفيذ الأعمال حسب المواصفات",
            notes = "عقد تجريبي لسيناريو UAT"
        };

        var contract = ProjectContract.Create(
            projectId, ownerId, template.Id,
            contractType: template.ContractType,
            contractTitle: template.TemplateNameAr,
            partyName: "مؤسسة البناء الحديث",
            partyPhone: "0502222222",
            partyNationalId: null,
            contractValue: 150000m,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddMonths(6),
            contractDataJson: JsonSerializer.Serialize(contractData));

        await _db.ProjectContracts.AddAsync(contract, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created draft contract for pilot project.");
    }

    private async Task EnsureUploadLinkAsync(Guid projectId, Guid ownerId, CancellationToken ct)
    {
        var hasLinks = await _db.FollowerUploadLinks.AnyAsync(
            l => l.ProjectId == projectId, ct);
        if (hasLinks)
        {
            _logger.LogInformation("Upload link already exists, skipping.");
            return;
        }

        var follower = await _db.ProjectFollowers.FirstOrDefaultAsync(
            f => f.ProjectId == projectId && !f.IsDeleted, ct);
        if (follower is null)
        {
            _logger.LogWarning("No follower found for upload link seed. Skipping.");
            return;
        }

        // Generate a token and store only the hash (matching existing pattern)
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToBase64String(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));
        var tokenPreview = rawToken[..8] + "...";

        var link = FollowerUploadLink.Create(
            projectId, ownerId, follower.Id,
            tokenHash: tokenHash,
            tokenPreview: tokenPreview,
            expiresAtUtc: DateTime.UtcNow.AddDays(30));

        await _db.FollowerUploadLinks.AddAsync(link, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created upload link for follower {Name}.", follower.FullName);
    }
}
