using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrdoEtCura.Web.Models;

namespace OrdoEtCura.Web.Controllers;

public sealed class AccountController(SignInManager<DemoUser> signIn, UserManager<DemoUser> users) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new LoginInput { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginInput input)
    {
        if (!ModelState.IsValid)
            return View(input);

        var user = await users.FindByNameAsync(input.Email);
        var canSignIn = user is not null && await users.IsInRoleAsync(user, DemoRoles.Admin);
        var result = canSignIn
            ? await signIn.PasswordSignInAsync(user!, input.Password, false, lockoutOnFailure: true)
            : Microsoft.AspNetCore.Identity.SignInResult.Failed;
        if (result.Succeeded)
            return Url.IsLocalUrl(input.ReturnUrl) ? LocalRedirect(input.ReturnUrl!) : RedirectToAction("Index", "BackOffice");

        ModelState.AddModelError("", "Sign-in failed. Check your email and password or try again later.");
        return View(input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "BackOffice");
    }

    public IActionResult Denied() => StatusCode(StatusCodes.Status403Forbidden, "You do not have access to this area.");
}

