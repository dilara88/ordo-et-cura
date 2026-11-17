using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdoEtCura.Web.Models;

namespace OrdoEtCura.Web.Controllers;

[Authorize(Roles = DemoRoles.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class BackOfficeController(DemoDb db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var accounts = await (from user in db.Users
                              join membership in db.UserRoles on user.Id equals membership.UserId
                              join role in db.Roles on membership.RoleId equals role.Id
                              orderby user.Email, role.Name
                              select new DemoAccountRow(user.Email!, user.Company, role.Name!)).ToListAsync();

        var projects = await (from project in db.Projects
                              join approver in db.Users on project.ApproverId equals approver.Id
                              orderby project.Name
                              select new DemoProjectRow(project.Name, project.Company, project.Plan,
                                  approver.Email!, project.ApprovedAt)).ToListAsync();
        return View(new BackOfficeView(accounts, projects));
    }
}
