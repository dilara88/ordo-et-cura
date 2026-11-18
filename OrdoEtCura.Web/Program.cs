using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrdoEtCura.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
var connection = builder.Configuration.GetConnectionString("OrdoEtCura")
    ?? throw new InvalidOperationException("Configure the OrdoEtCura SQL Server connection.");
builder.Services.AddDbContext<DemoDb>(options => options.UseSqlServer(connection));
builder.Services.AddIdentity<DemoUser, IdentityRole>()
    .AddEntityFrameworkStores<DemoDb>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OrdoEtCura.Customer";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else
            context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        else
            context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


if (args.Contains("--initialize-demo", StringComparer.Ordinal))
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException("Demo initialization requires the Development environment.");
    await DemoSetup.InitializeAsync(app.Services, app.Configuration);
    return;
}

app.Run();
