using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmeErp.Domain.Entities;

namespace SmeErp.Infrastructure.Persistence.Configurations;

public class QuotationLineConfiguration : IEntityTypeConfiguration<QuotationLine>
{
    public void Configure(EntityTypeBuilder<QuotationLine> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(l => l.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(l => l.GstPercent)
            .HasPrecision(5, 2);

        builder.Property(l => l.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(l => l.TotalAmount)
            .HasPrecision(18, 2);

        builder.HasOne(l => l.Quotation)
            .WithMany(q => q.Lines)
            .HasForeignKey(l => l.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
            .WithMany(p => p.QuotationLines)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
