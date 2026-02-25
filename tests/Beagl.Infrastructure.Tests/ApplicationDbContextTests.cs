// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity;

namespace Beagl.Infrastructure.Tests;

/// <summary>
/// Verifies that the ApplicationDbContext model maps ASP.NET Core Identity entities to the expected table names.
/// </summary>
public class ApplicationDbContextTests
{
    [Fact]
    public void Model_ShouldConfigureIdentityTableNames()
    {
        // Arrange
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
        DbContextOptions<ApplicationDbContext> options = optionsBuilder.UseInMemoryDatabase("TestDb").Options;
        using ApplicationDbContext context = new(options);
        // Act
        IModel model = context.Model;

        // Assert
        AssertEntityTypeTableName<ApplicationUser>(model, "Users");
        AssertEntityTypeTableName<ApplicationRole>(model, "Roles");
        AssertEntityTypeTableName<IdentityUserClaim<string>>(model, "UserClaims");
        AssertEntityTypeTableName<IdentityUserRole<string>>(model, "UserRoles");
        AssertEntityTypeTableName<IdentityUserLogin<string>>(model, "UserLogins");
        AssertEntityTypeTableName<IdentityRoleClaim<string>>(model, "RoleClaims");
        AssertEntityTypeTableName<IdentityUserToken<string>>(model, "UserTokens");
    }

    private static void AssertEntityTypeTableName<TEntity>(IModel model, string expectedTableName)
    {
        System.Type entityType = typeof(TEntity);
        IEntityType? efEntityType = model.FindEntityType(entityType);
        efEntityType.Should().NotBeNull($"the {entityType.Name} entity type must be configured in the model");
        efEntityType!.GetTableName().Should().Be(expectedTableName);
    }
}
