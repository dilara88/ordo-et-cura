using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdoEtCura.Web.Models;

namespace OrdoEtCura.Web.Controllers;

[Authorize(Roles = DemoRoles.Collaborator + "," + DemoRoles.Approver)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class WorkspaceController(DemoDb db, UserManager<DemoUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var (project, user) = await FindProjectAsync();
        if (project is null)
            return Forbid();

        return View(new WorkspaceView(project, CanApprove(project, user)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = DemoRoles.Approver)]
    public async Task<IActionResult> Approve()
    {
        var (project, user) = await FindProjectAsync();
        if (project is null || !CanApprove(project, user))
            return Forbid();

        await db.Projects.Where(item => item.Id == project.Id &&
            item.ApproverId == user!.Id && item.Company == user.Company && item.ApprovedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(item => item.ApprovedAt, DateTimeOffset.UtcNow));
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/api/projects/{id}/approval")]
    public async Task<IActionResult> ApprovalStatus(string id)
    {
        if (id != DemoSetup.ProjectId)
            return NotFound();

        var (project, _) = await FindProjectAsync();
        if (project is null)
            return Forbid();

        return Ok(new
        {
            projectId = project.Id,
            workspace = project.Name,
            approval = project.ApprovedAt is null ? "pending" : "approved",
            training = project.ApprovedAt is null ? "blocked" : "ready"
        });
    }

    private async Task<(OnboardingProject? Project, DemoUser? User)> FindProjectAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return (null, null);

        var project = await db.Projects.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == DemoSetup.ProjectId && item.Company == user.Company);
        return (project, user);
    }

    private bool CanApprove(OnboardingProject project, DemoUser? user) =>
        User.IsInRole(DemoRoles.Approver) && project.ApproverId == user?.Id;
}
