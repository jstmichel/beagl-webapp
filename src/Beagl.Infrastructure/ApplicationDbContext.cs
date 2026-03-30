// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.EmailProviders.Entities;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure;

/// <summary>
/// Represents the Entity Framework database context for the application.
/// </summary>
/// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    /// <summary>
    /// Gets the email provider configuration table.
    /// </summary>
    public DbSet<EmailProviderConfigEntity> EmailProviderConfigurations => Set<EmailProviderConfigEntity>();

    /// <summary>
    /// Gets the citizen profiles table.
    /// </summary>
    public DbSet<CitizenProfileEntity> CitizenProfiles => Set<CitizenProfileEntity>();

    /// <summary>
    /// Configures the model for all entities.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        RenameIdentityTables(builder);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique()
            .HasFilter("\"NormalizedEmail\" IS NOT NULL");

        builder.Entity<EmailProviderConfigEntity>(entity =>
        {
            entity.ToTable("EmailProviderConfigurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(512);
            entity.Property(e => e.SenderEmail).IsRequired().HasMaxLength(256);
            entity.Property(e => e.SenderName).IsRequired().HasMaxLength(256);
        });

        builder.Entity<CitizenProfileEntity>(entity =>
        {
            entity.ToTable("CitizenProfiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(256);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<CitizenProfileEntity>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });
    }

    private static void RenameIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }
}
