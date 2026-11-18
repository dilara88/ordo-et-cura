using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OrdoEtCura.Web.Models;

public static class DemoSetup
{
    public const string ProjectId = "P-B01";
    private const string Customer = "Concordia Systems";

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DemoDb>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<DemoUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await db.Database.MigrateAsync();

        var accounts = new[]
        {
            (Name: "collaborator", Company: Customer, Role: DemoRoles.Collaborator),
            (Name: "approver", Company: Customer, Role: DemoRoles.Approver),
            (Name: "outsider", Company: "Aster", Role: DemoRoles.Collaborator),
            (Name: "aster-approver", Company: "Aster", Role: DemoRoles.Approver),
            (Name: "admin", Company: "Ordo et Cura", Role: DemoRoles.Admin)
        };

        foreach (var role in accounts.Select(account => account.Role).Distinct())
        {
            if (!await roles.RoleExistsAsync(role))
                RequireSuccess(await roles.CreateAsync(new IdentityRole(role)));
        }

        foreach (var account in accounts)
        {
            var email = $"{account.Name}@demo.example";
            var user = await users.FindByNameAsync(email);
            if (user is null)
            {
                var password = configuration[$"Demo:Passwords:{account.Name}"];
                if (string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException($"Set Demo:Passwords:{account.Name} before seeding accounts.");

                user = new DemoUser { UserName = email, Email = email, Company = account.Company };
                RequireSuccess(await users.CreateAsync(user, password));
                RequireSuccess(await users.AddToRoleAsync(user, account.Role));
            }


        }


        if (!await db.Projects.AnyAsync(project => project.Id == "P-A01"))
        {
            db.Projects.Add(new OnboardingProject
            {
                Id = "P-A01", Name = "Aster Launch", Company = "Aster", Plan = "Core",
                ApproverId = (await users.FindByNameAsync("aster-approver@demo.example"))!.Id, ApprovedAt = null
            });
            db.ProjectEvents.Add(new ProjectEvent { ProjectId = "P-A01", Actor = "System", Description = "Project created with configuration complete and approval pending." });
            await db.SaveChangesAsync();
        }

        if (!await db.Projects.AnyAsync(project => project.Id == ProjectId))
        {
            var approver = await users.FindByNameAsync("approver@demo.example");
            db.Projects.Add(new OnboardingProject
            {
                Id = ProjectId, Name = "Transitus", Company = Customer, Plan = "Growth",
                ApproverId = approver!.Id
            });
            db.ProjectEvents.Add(new ProjectEvent { ProjectId = ProjectId, Actor = "System", Description = "Project created with configuration complete and approval pending." });
            await db.SaveChangesAsync();
        }
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(error => error.Code)));
    }
}
