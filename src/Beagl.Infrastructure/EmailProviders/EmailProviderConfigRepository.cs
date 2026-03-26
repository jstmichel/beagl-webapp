// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.EmailProviders;
using Beagl.Infrastructure.EmailProviders.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.EmailProviders;

/// <summary>
/// Persists the email provider configuration through Entity Framework Core.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class EmailProviderConfigRepository(ApplicationDbContext dbContext) : IEmailProviderConfigRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <inheritdoc />
    public async Task<EmailProviderConfig?> GetActiveAsync(CancellationToken cancellationToken)
    {
        EmailProviderConfigEntity? entity = await _dbContext.EmailProviderConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<EmailProviderConfig> SaveAsync(EmailProviderConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        EmailProviderConfigEntity? existing = await _dbContext.EmailProviderConfigurations
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            EmailProviderConfigEntity newEntity = new()
            {
                Id = config.Id,
                ApiKey = config.ApiKey,
                SenderEmail = config.SenderEmail,
                SenderName = config.SenderName,
            };

            _dbContext.EmailProviderConfigurations.Add(newEntity);
        }
        else
        {
            existing.Id = config.Id;
            existing.ApiKey = config.ApiKey;
            existing.SenderEmail = config.SenderEmail;
            existing.SenderName = config.SenderName;
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return config;
    }

    private static EmailProviderConfig MapToDomain(EmailProviderConfigEntity entity)
    {
        return new EmailProviderConfig(entity.Id, entity.ApiKey, entity.SenderEmail, entity.SenderName);
    }
}
