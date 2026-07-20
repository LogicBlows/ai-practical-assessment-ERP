using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Services;

public interface ISigningKeyService
{
    Task<SigningKey> GetActiveKeyAsync(CancellationToken cancellationToken = default);

    Task<SigningKey> RotateKeyAsync(CancellationToken cancellationToken = default);
}
