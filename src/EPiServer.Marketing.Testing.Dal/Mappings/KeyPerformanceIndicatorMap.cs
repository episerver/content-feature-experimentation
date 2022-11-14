using System.ComponentModel.DataAnnotations.Schema;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyPerformanceIndicatorMap : IEntityTypeConfiguration<DalKeyPerformanceIndicator>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DalKeyPerformanceIndicator> builder)
        {
            builder.ToTable("tblABKeyPerformanceIndicator");

            builder.HasKey(hk => hk.Id);

            builder.Property(m => m.KeyPerformanceIndicatorId);

            builder.HasOne(m => m.DalABTest)
                .WithMany(m => m.KeyPerformanceIndicators)
                .HasForeignKey(m => m.TestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
