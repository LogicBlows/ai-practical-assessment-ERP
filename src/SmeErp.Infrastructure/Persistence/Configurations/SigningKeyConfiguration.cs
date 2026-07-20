using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Configurations;

public class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(k => k.KeyValue)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(k => k.IsActive);
    }
}
