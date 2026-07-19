using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Mobile)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(c => c.CompanyId);

        builder.HasOne(c => c.Company)
            .WithMany(co => co.Customers)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
