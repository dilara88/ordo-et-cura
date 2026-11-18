using System.Data;
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
    public async Task<IActionResult> Index(string? id)
    {
        var user = await users.GetUserAsync(User);
        if (user is null || !user.IsActive) return Forbid();
        var projects = await db.Projects.AsNoTracking().Where(p => p.Company == user.Company).OrderBy(p => p.Name).ToListAsync();
        var project = id is null ? projects.FirstOrDefault() : projects.SingleOrDefault(p => p.Id == id);
        if (project is null) return id is null ? View("Empty") : NotFound();
        return View(new WorkspaceView(project, User.IsInRole(DemoRoles.Approver) && project.ApproverId == user.Id,
            projects, await db.ProjectEvents.Where(e => e.ProjectId == project.Id).OrderByDescending(e => e.Id).ToListAsync()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = DemoRoles.Approver)]
    public async Task<IActionResult> Approve(string id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await users.GetUserAsync(User);
        if (user is null || !user.IsActive) return Forbid();
        var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == id && p.Company == user.Company && p.ApproverId == user.Id);
        if (project is null) return NotFound();
        if (project.ApprovedAt is null)
        {
            project.ApprovedAt = DateTimeOffset.UtcNow;
            db.ProjectEvents.Add(new ProjectEvent { ProjectId = id, Actor = user.Email!,
                Description = "Configuration approved. Team training is ready to start." });
            await db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Index), new { id });
    }

    [HttpGet("/api/projects/{id}/approval")]
    public async Task<IActionResult> ApprovalStatus(string id)
    {
        var user = await users.GetUserAsync(User);
        if (user is null || !user.IsActive) return Forbid();
        var project = await db.Projects.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id && p.Company == user.Company);
        if (project is null) return NotFound();
        return Ok(new { projectId = project.Id, workspace = project.Name,
            approval = project.ApprovedAt is null ? "pending" : "approved",
            training = project.ApprovedAt is null ? "blocked" : "ready" });
    }
}
