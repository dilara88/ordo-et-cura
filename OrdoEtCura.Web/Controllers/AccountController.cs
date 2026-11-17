using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrdoEtCura.Web.Models;

namespace OrdoEtCura.Web.Controllers;

public sealed class AccountController(SignInManager<DemoUser> signIn, UserManager<DemoUser> users) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new LoginInput { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInput input)
    {
        if (!ModelState.IsValid)
            return View(input);

        var user = await users.FindByNameAsync(input.Email);
        var canSignIn = user is not null &&
            (await users.IsInRoleAsync(user, DemoRoles.Collaborator) ||
             await users.IsInRoleAsync(user, DemoRoles.Approver));
        var result = canSignIn
            ? await signIn.PasswordSignInAsync(user!, input.Password, false, lockoutOnFailure: true)
            : Microsoft.AspNetCore.Identity.SignInResult.Failed;
        if (result.Succeeded)
            return Url.IsLocalUrl(input.ReturnUrl) ? LocalRedirect(input.ReturnUrl!) : RedirectToAction("Index", "Home");

        ModelState.AddModelError("", "Sign-in failed. Check your email and password or try again later.");
        return View(input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denied() => StatusCode(StatusCodes.Status403Forbidden, "You do not have access to this area.");
}

