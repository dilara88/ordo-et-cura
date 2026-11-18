using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdoEtCura.Web.Models;

namespace OrdoEtCura.Web.Controllers;

[Authorize(Roles = DemoRoles.Admin)]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class BackOfficeController(DemoDb db, UserManager<DemoUser> users, SignInManager<DemoUser> signIn) : Controller
{
    public async Task<IActionResult> Index()
    {
        var accounts = await (from user in db.Users
                              join membership in db.UserRoles on user.Id equals membership.UserId
                              join role in db.Roles on membership.RoleId equals role.Id
                              orderby user.Email
                              select new UserRow(user.Id, user.Email!, user.Company, role.Name!, user.IsActive)).ToListAsync();
        return View(new BackOfficeView(accounts, await db.Projects.OrderBy(p => p.Name).ToListAsync()));
    }

    public async Task<IActionResult> UserDetails(string? id)
    {
        if (id is null) return View(new ManagedUserInput());
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        return View(new ManagedUserInput { Id = user.Id, Email = user.Email!, Company = user.Company,
            Role = (await users.GetRolesAsync(user)).Single() });
    }

    [HttpPost]
    public async Task<IActionResult> SaveUser(ManagedUserInput input)
    {
        // A blank field binds as null, so trim safely and let the checks below report it.
        input.Company = input.Company?.Trim() ?? "";
        if (input.Role is not (DemoRoles.Admin or DemoRoles.Approver or DemoRoles.Collaborator))
            ModelState.AddModelError(nameof(input.Role), "Choose a supported role.");
        if (string.IsNullOrWhiteSpace(input.Company))
        {
            ModelState.Remove(nameof(input.Company)); // replace the generic "field is required" message
            ModelState.AddModelError(nameof(input.Company), "Enter a company.");
        }
        if (input.Id is null && string.IsNullOrWhiteSpace(input.Password))
            ModelState.AddModelError(nameof(input.Password), "Enter an initial password.");
        if (!ModelState.IsValid) return View("UserDetails", input);

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = input.Id is null ? new DemoUser() : await users.FindByIdAsync(input.Id);
        if (user is null) return NotFound();
        if (user.Id == users.GetUserId(User) && (user.Company != input.Company || input.Role != DemoRoles.Admin))
            ModelState.AddModelError("", "Use another administrator to change your own membership or role.");
        if (input.Id is not null && (user.Company != input.Company || input.Role != DemoRoles.Approver) &&
            await db.Projects.AnyAsync(p => p.ApproverId == user.Id))
            ModelState.AddModelError("", "Reassign this user's projects before changing their company or approver role.");
        if (!ModelState.IsValid) return View("UserDetails", input);

        // Only a change to sign-in name, company or role should end the user's existing sessions.
        var isSelf = input.Id is not null && user.Id == users.GetUserId(User);
        var accessChanged = input.Id is not null &&
            (user.Email != input.Email || user.Company != input.Company ||
             !(await users.GetRolesAsync(user)).SequenceEqual([input.Role]));

        user.Company = input.Company;
        var result = IdentityResult.Success;
        if (input.Id is null)
        {
            user.UserName = input.Email;
            user.Email = input.Email;
            result = await users.CreateAsync(user, input.Password!);
        }
        else
        {
            // Email is the login identifier. Keep it consistent with UserName.
            user.Email = input.Email;
            user.UserName = input.Email;
            result = await users.UpdateAsync(user);
        }
        if (result.Succeeded)
        {
            var oldRoles = await users.GetRolesAsync(user);
            result = await users.RemoveFromRolesAsync(user, oldRoles);
            if (result.Succeeded) result = await users.AddToRoleAsync(user, input.Role);
            if (result.Succeeded && accessChanged) result = await users.UpdateSecurityStampAsync(user);
        }
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View("UserDetails", input);
        }
        await transaction.CommitAsync();
        // Keep administrators signed in after changing their own sign-in email.
        if (isSelf && accessChanged) await signIn.RefreshSignInAsync(user);
        TempData["Notice"] = "User saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(string id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (user.Id == users.GetUserId(User))
        {
            TempData["Notice"] = "Use another administrator to deactivate your account.";
            return RedirectToAction(nameof(Index));
        }
        if (user.IsActive && await db.Projects.AnyAsync(p => p.ApproverId == id))
        {
            TempData["Notice"] = "Reassign this user's projects before deactivating them.";
            return RedirectToAction(nameof(Index));
        }
        user.IsActive = !user.IsActive;
        var result = await users.UpdateAsync(user);
        if (result.Succeeded) result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded)
        {
            TempData["Notice"] = "The account changed. Review it and try again.";
            return RedirectToAction(nameof(Index));
        }
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Project(string id)
    {
        var project = await db.Projects.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
        if (project is null) return NotFound();
        var approvers = await users.GetUsersInRoleAsync(DemoRoles.Approver);
        return View(new ProjectDetails(project,
            approvers.Where(u => u.IsActive && u.Company == project.Company).OrderBy(u => u.Email).ToList(),
            await db.ProjectEvents.Where(e => e.ProjectId == id).OrderByDescending(e => e.Id).ToListAsync()));
    }

    [HttpPost]
    public async Task<IActionResult> AssignApprover(string id, string approverId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == id);
        var approver = await users.FindByIdAsync(approverId);
        if (project is null) return NotFound();
        if (approver is null || !approver.IsActive || approver.Company != project.Company ||
            !await users.IsInRoleAsync(approver, DemoRoles.Approver))
        {
            TempData["Notice"] = "Select an active approver from this project's company.";
            return RedirectToAction(nameof(Project), new { id });
        }
        if (project.ApproverId != approverId)
        {
            var previous = await users.FindByIdAsync(project.ApproverId);
            project.ApproverId = approverId;
            project.ApprovedAt = null;
            db.ProjectEvents.Add(new ProjectEvent { ProjectId = id, Actor = User.Identity!.Name!,
                Description = $"Approver changed from {previous?.Email ?? project.ApproverId} to {approver.Email}. Approval is pending." });
            await db.SaveChangesAsync();
            TempData["Notice"] = "Approver saved. Approval is pending.";
        }
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Project), new { id });
    }
}
