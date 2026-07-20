using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SmeErp.Domain.Entities;
using SmeErp.Infrastructure.Persistence;

namespace SmeErp.Infrastructure.Services;

public class SigningKeyService : ISigningKeyService
{
    private const int KeyExpirationDays = 30;
    private const int KeyByteLength = 64;

    private readonly AppDbContext _dbContext;

    public SigningKeyService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SigningKey> GetActiveKeyAsync(CancellationToken cancellationToken = default)
    {
        var activeKey = await FindActiveKeyAsync(cancellationToken);
        if (activeKey is not null)
        {
            return activeKey;
        }

        return await CreateKeyAsync(cancellationToken);
    }

    public async Task<SigningKey> RotateKeyAsync(CancellationToken cancellationToken = default)
    {
        var activeKeys = await _dbContext.SigningKeys
            .Where(k => k.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var key in activeKeys)
        {
            key.IsActive = false;
        }

        return await CreateKeyAsync(cancellationToken);
    }

    private async Task<SigningKey?> FindActiveKeyAsync(CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        return await _dbContext.SigningKeys
            .Where(k => k.IsActive && k.ExpiresAt > utcNow)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<SigningKey> CreateKeyAsync(CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var signingKey = new SigningKey
        {
            KeyValue = GenerateKeyValue(),
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddDays(KeyExpirationDays),
            IsActive = true
        };

        _dbContext.SigningKeys.Add(signingKey);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return signingKey;
    }

    private static string GenerateKeyValue()
    {
        var keyBytes = new byte[KeyByteLength];
        RandomNumberGenerator.Fill(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
