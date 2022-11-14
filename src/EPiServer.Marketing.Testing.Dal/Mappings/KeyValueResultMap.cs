using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyValueResultMap : IEntityTypeConfiguration<DalKeyValueResult>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DalKeyValueResult> builder)
        {
            builder.ToTable("tblABKeyValueResult");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.KpiId)
                .IsRequired();

            builder.Property(m => m.Value)
                .IsRequired();

            builder.HasOne(m => m.DalVariant)
                .WithMany(m => m.DalKeyValueResults)
                .HasForeignKey(m => m.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
