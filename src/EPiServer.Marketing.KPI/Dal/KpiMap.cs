using Microsoft.EntityFrameworkCore;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.Dal
{
    internal class KpiMap : IEntityTypeConfiguration<DalKpi>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DalKpi> builder)
        {
            builder.ToTable("tblKeyPerformaceIndicator");

            builder.HasKey(hk => hk.Id);

            builder.Property(m => m.ClassName)
                .IsRequired();

            builder.Property(m => m.Properties)
                .IsRequired();

            builder.Property(m => m.CreatedDate)
                .IsRequired();

            builder.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
