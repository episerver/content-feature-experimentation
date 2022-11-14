using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyFinancialResultMap : IEntityTypeConfiguration<DalKeyFinancialResult>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DalKeyFinancialResult> builder)
        {
            builder.ToTable("tblABKeyFinancialResult");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.KpiId)
                .IsRequired();

            builder.Property(m => m.Total)
                .IsRequired();

            builder.Property(m => m.TotalMarketCulture)
                .IsRequired();

            builder.Property(m => m.ConvertedTotal)
                .IsRequired();

            builder.Property(m => m.ConvertedTotalCulture)
                .IsRequired();

            builder.HasOne(m => m.DalVariant)
                .WithMany(m => m.DalKeyFinancialResults)
                .HasForeignKey(m => m.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
