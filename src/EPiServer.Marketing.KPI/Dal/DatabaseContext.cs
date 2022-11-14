using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.Dal
{
    using System;    
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public class KpiDatabaseContext : DbContext
    {
        [ExcludeFromCodeCoverage]
        public KpiDatabaseContext(DbContextOptions<KpiDatabaseContext> options) : base(options)
        {
        }

        public DbSet<DalKpi> Kpis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                this._modelBuilder = modelBuilder;

                _modelBuilder.ApplyConfiguration(new KpiMap());
            }
        }

        private ModelBuilder _modelBuilder;
    }
}
