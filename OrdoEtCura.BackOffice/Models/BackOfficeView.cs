using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OrdoEtCura.Web.Models;

public sealed class ManagedUserInput
{
    public string? Id { get; set; }
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Company { get; set; } = "";
    [Required] public string Role { get; set; } = DemoRoles.Collaborator;
    [DataType(DataType.Password)] public string? Password { get; set; }
}
public sealed record UserRow(string Id, string Email, string Company, string Role, bool Active);
public sealed record BackOfficeView(IReadOnlyList<UserRow> Accounts, IReadOnlyList<OnboardingProject> Projects);
public sealed record ProjectDetails(OnboardingProject Project, IReadOnlyList<DemoUser> Approvers,
    IReadOnlyList<ProjectEvent> History);
