// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
using Beagl.Infrastructure.Breeds;
using Beagl.Infrastructure.Breeds.Entities;
using Beagl.Infrastructure.Colors;
using Beagl.Infrastructure.Colors.Entities;
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
[ExcludeFromCodeCoverage]
public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    /// <summary>
    /// Gets the breeds table.
    /// </summary>
    public DbSet<BreedEntity> Breeds => Set<BreedEntity>();

    /// <summary>
    /// Gets the colors table.
    /// </summary>
    public DbSet<ColorEntity> Colors => Set<ColorEntity>();

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

        builder.Entity<BreedEntity>(entity =>
        {
            entity.ToTable("Breeds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnimalType).IsRequired().HasConversion<int>();
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameFr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.HasData(BreedSeedData.GetAll());
        });

        builder.Entity<ColorEntity>(entity =>
        {
            entity.ToTable("Colors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameFr).IsRequired().HasMaxLength(100);
            entity.HasData(ColorSeedData.GetAll());
        });

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
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(256).HasColumnName("Address_Street");
                address.Property(a => a.City).HasMaxLength(100).HasColumnName("Address_City");
                address.Property(a => a.Province).HasMaxLength(100).HasColumnName("Address_Province");
                address.Property(a => a.PostalCode).HasMaxLength(20).HasColumnName("Address_PostalCode");
            });
            entity.Property(e => e.CommunicationPreference)
                .HasConversion<int>()
                .HasDefaultValue(Domain.Users.CommunicationPreference.None);
            entity.Property(e => e.LanguagePreference)
                .HasConversion<int>()
                .HasDefaultValue(Domain.Users.LanguagePreference.None);
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

        builder.Entity<ApplicationUser>()
            .Property(u => u.RecoveryCode)
            .HasMaxLength(6);

        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }
}
