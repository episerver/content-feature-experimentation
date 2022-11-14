using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class ABTestMap : IEntityTypeConfiguration<DalABTest>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DalABTest> builder)
        {
            builder.ToTable("tblABTest");

            builder.HasKey(hk => hk.Id);

            builder.Property(m => m.Title)
                .IsRequired();

            builder.Property(m => m.Description);

            builder.Property(m => m.Owner)
                .IsRequired();

            builder.Property(m => m.OriginalItemId)
                .IsRequired();

            builder.Property(m => m.State)
                .IsRequired();

            builder.Property(m => m.StartDate)
                .IsRequired();

            builder.Property(m => m.EndDate)
                .IsRequired();

            builder.Property(m => m.ParticipationPercentage)
                .IsRequired();

            builder.Property(m => m.LastModifiedBy)
                .HasMaxLength(100);

            builder.Property(m => m.ExpectedVisitorCount);

            builder.Property(m => m.ActualVisitorCount)
                .IsRequired();

            builder.Property(m => m.ConfidenceLevel)
                .IsRequired();

            builder.Property(m => m.ZScore)
                .IsRequired();

            builder.Property(m => m.IsSignificant)
                .IsRequired();

            builder.Property(m => m.ContentLanguage)
                .IsRequired();

            builder.Property(m => m.CreatedDate)
                .IsRequired();

            builder.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
