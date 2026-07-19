using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Configurations;

public class CompanySettingConfiguration : IEntityTypeConfiguration<CompanySetting>
{
    public void Configure(EntityTypeBuilder<CompanySetting> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(s => s.CompanyId);

        builder.HasOne(s => s.Company)
            .WithMany(c => c.Settings)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
