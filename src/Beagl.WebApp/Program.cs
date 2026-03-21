// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Globalization;
using Beagl.Infrastructure;
using Beagl.Infrastructure.Users;
using Beagl.Infrastructure.Users.Entities;
using Beagl.Application.Users.Services;
using Beagl.Domain.Users;
using Beagl.WebApp.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
        "Database connection string 'DefaultConnection' is missing from configuration.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add ASP.NET Core Identity (without default UI), using EF Core for storage
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserRepository, IdentityUserRepository>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.AddServiceDefaults();

WebApplication app = builder.Build();

// Ensure database is created and migrations are applied at startup
await app.ExecuteMigrationsAsync(builder.Configuration);

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
app.MapRazorComponents<Beagl.WebApp.Components.App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
