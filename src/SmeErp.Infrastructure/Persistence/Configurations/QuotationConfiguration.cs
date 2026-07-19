using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Configurations;

public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.QuotationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(q => q.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(q => q.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(q => q.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(q => q.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(q => q.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(q => q.CompanyId);

        builder.HasOne(q => q.Company)
            .WithMany(c => c.Quotations)
            .HasForeignKey(q => q.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Customer)
            .WithMany(c => c.Quotations)
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
