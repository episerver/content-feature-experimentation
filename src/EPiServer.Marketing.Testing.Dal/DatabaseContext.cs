using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal
{
    public class DatabaseContext : DbContext
    {
        [ExcludeFromCodeCoverage]
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<DalABTest> ABTests { get; set; }

        public DbSet<DalVariant> Variants { get; set; }

        public DbSet<DalKeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }

        public DbSet<DalKeyFinancialResult> DalKeyFinancialResults { get; set; }

        public DbSet<DalKeyValueResult> DalKeyValueResults { get; set; }

        public DbSet<DalKeyConversionResult> DalKeyConversionResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                this._modelBuilder = modelBuilder;

                _modelBuilder.ApplyConfiguration(new Mappings.ABTestMap());
                _modelBuilder.ApplyConfiguration(new Mappings.VariantMap());
                _modelBuilder.ApplyConfiguration(new Mappings.KeyPerformanceIndicatorMap());
                _modelBuilder.ApplyConfiguration(new Mappings.KeyFinancialResultMap());
                _modelBuilder.ApplyConfiguration(new Mappings.KeyValueResultMap());
                _modelBuilder.ApplyConfiguration(new Mappings.KeyConversionResultMap());
            }
        }


        

        private ModelBuilder _modelBuilder;
    }
}
