using System.ComponentModel.DataAnnotations.Schema;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class VariantMap : IEntityTypeConfiguration<DalVariant>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DalVariant> builder)
        {
            builder.ToTable("tblABVariant");

            builder.HasKey(hk => hk.Id);

            builder.Property(m => m.ItemId)
                .IsRequired();

            builder.Property(m => m.ItemVersion)
                .IsRequired();

            builder.Property(m => m.IsWinner)
                .IsRequired();

            builder.Property(m => m.Views)
                .IsRequired();

            builder.Property(m => m.Conversions)
                .IsRequired();

            builder.Property(m => m.IsPublished)
                .IsRequired();

            builder.HasOne(m => m.DalABTest)
                .WithMany(m => m.Variants)
                .HasForeignKey(m => m.TestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
