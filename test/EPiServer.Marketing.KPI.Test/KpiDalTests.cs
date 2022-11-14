using Microsoft.Extensions.Configuration;
using EPiServer.ServiceLocation;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq;
using Xunit;
using EPiServer.Marketing.KPI.Dal;

namespace EPiServer.Marketing.KPI.Test
{
    public class KpiDalTests : KpiTestBase
    {
        private KpiTestContext _context;
        public KpiDalTests()
        {
            var optionsBuilder = new DbContextOptionsBuilder<KpiDatabaseContext>().UseInMemoryDatabase(databaseName: "episerver.testing").EnableServiceProviderCaching(false);
            _context = new KpiTestContext(optionsBuilder.Options);
        }

        [Fact]
        public void AddMultivariateTest()
        {
            var newTests = AddKpis(_context, 2);
            _context.SaveChanges();

            Assert.Equal(_context.Kpis.Count(), 2);
        }

        [Fact]
        public void DeleteMultivariateTest()
        {
            var newTests = AddKpis(_context, 3);
            _context.SaveChanges();

            
            _context.Kpis.Remove(newTests[0]);
            _context.SaveChanges();

            Assert.Equal(_context.Kpis.Count(), 2);
        }

       
    }
}
