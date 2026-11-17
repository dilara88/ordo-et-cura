using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrdoEtCura.Web.Models;

public sealed class DemoUser : IdentityUser
{
    public string Company { get; set; } = "";
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

public sealed class DemoDb(DbContextOptions<DemoDb> options) : IdentityDbContext<DemoUser>(options)
{
    public DbSet<OnboardingProject> Projects => Set<OnboardingProject>();
}

public static class DemoRoles
{
    public const string Collaborator = "Collaborator";
    public const string Approver = "Approver";
    public const string Admin = "Admin";
}
