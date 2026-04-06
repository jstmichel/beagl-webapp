// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Beagl.Infrastructure;
using Beagl.Infrastructure.EmailProviders;
using Beagl.Infrastructure.Users;
using Beagl.Infrastructure.Users.Entities;
using Beagl.Application.CitizenProfiles.Services;
using Beagl.Application.EmailProviders.Services;
using Beagl.Application.Setup.Services;
using Beagl.Application.Users.Services;
using Beagl.Domain.EmailProviders;
using Beagl.Domain.Users;
using Beagl.WebApp.Authentication;
using Beagl.WebApp.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add<MustChangePasswordFilter>());

CultureInfo[] supportedCultures =
[
    new CultureInfo("en"),
    new CultureInfo("fr"),
];

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add EF Core DbContext with PostgreSQL
string? connectionString = builder.Configuration.GetConnectionString("beagl-db");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'beagl-db' is missing from configuration.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add ASP.NET Core Identity (without default UI), using EF Core for storage
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(
        AuthorizationPolicies.EmployeeAccess,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(
                AuthorizationPolicies.EmployeeRole,
                AuthorizationPolicies.AdministratorRole));

builder.Services.AddScoped<IUserRepository, IdentityUserRepository>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddSingleton<SetupStatusCache>();
builder.Services.AddScoped<IInitialSetupService, InitialSetupService>();
builder.Services.AddScoped<IEmailProviderConfigRepository, EmailProviderConfigRepository>();
builder.Services.AddScoped<IEmailProviderConfigService, EmailProviderConfigService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailSender, BrevoEmailSender>();
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<ISharedLoginService, SharedLoginService>();
builder.Services.AddScoped<ICitizenProfileRepository, CitizenProfileRepository>();
builder.Services.AddScoped<ICitizenProfileService, CitizenProfileService>();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.AddServiceDefaults();

WebApplication app = builder.Build();

// Ensure database is created and migrations are applied at startup
await app.ExecuteMigrationsAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages();
app.MapRazorComponents<Beagl.WebApp.Components.App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();

/// <summary>
/// The main entry point for the Beagl web application.
/// </summary>
[ExcludeFromCodeCoverage]
internal partial class Program
{
}
