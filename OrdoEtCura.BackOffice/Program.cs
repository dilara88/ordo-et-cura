using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrdoEtCura.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
var connection = builder.Configuration.GetConnectionString("OrdoEtCura")
    ?? throw new InvalidOperationException("Configure the OrdoEtCura SQL Server connection.");
builder.Services.AddDbContext<DemoDb>(options => options.UseSqlServer(connection));
builder.Services.AddIdentity<DemoUser, IdentityRole>()
    .AddEntityFrameworkStores<DemoDb>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OrdoEtCura.Staff";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
});
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireRole(DemoRoles.Admin).Build());

var app = builder.Build();
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();
app.MapControllerRoute("default", "{controller=BackOffice}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
