using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyConversionResultMap : IEntityTypeConfiguration<DalKeyConversionResult>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DalKeyConversionResult> builder)
        {
            builder.ToTable("tblABKeyConversionResult");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.KpiId)
                .IsRequired();

            builder.Property(m => m.Conversions)
                .IsRequired();

            builder.Property(m => m.Weight)
                .IsRequired();

            builder.Property(m => m.SelectedWeight)
                .IsRequired();

            builder.Property(m => m.Performance)
                .IsRequired();

            builder.HasOne(m => m.DalVariant)
                .WithMany(m => m.DalKeyConversionResults)
                .HasForeignKey(m => m.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
