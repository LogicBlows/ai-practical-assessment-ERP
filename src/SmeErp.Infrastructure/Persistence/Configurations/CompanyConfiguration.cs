using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmeErp.Domain.Entities;
using SmeErp.Infrastructure.Persistence.Seed;

namespace SmeErp.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Tagline)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PinCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.GstNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.PanNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Mobile)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Website)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.LogoPath)
            .HasMaxLength(500);

        builder.HasData(CompanySeedData.Companies);
    }
}
