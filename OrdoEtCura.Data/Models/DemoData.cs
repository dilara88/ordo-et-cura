using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrdoEtCura.Web.Models;

public sealed class DemoUser : IdentityUser
{
    public string Company { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class OnboardingProject
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Company { get; set; } = "";
    public string Plan { get; set; } = "";
    public string ApproverId { get; set; } = "";
    public DateTimeOffset? ApprovedAt { get; set; }
}

public sealed class ProjectEvent
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public string Actor { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class DemoDb(DbContextOptions<DemoDb> options) : IdentityDbContext<DemoUser>(options)
{
    public DbSet<OnboardingProject> Projects => Set<OnboardingProject>();
    public DbSet<ProjectEvent> ProjectEvents => Set<ProjectEvent>();
}

public static class DemoRoles
{
    public const string Collaborator = "Collaborator";
    public const string Approver = "Approver";
    public const string Admin = "Admin";
}
