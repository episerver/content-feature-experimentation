using System;
using System.Data.Common;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Test.Dal
{
    public class TestContext : DatabaseContext
    {
        public TestContext() : base(new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "EpiserverDB").EnableServiceProviderCaching(false)
                .Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<DalABTest>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<DalVariant>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<DalKeyPerformanceIndicator>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<DalKeyValueResult>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<DalKeyFinancialResult>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<DalKeyConversionResult>().Property(e => e.Id).ValueGeneratedNever();
            }
        }
    }
}
