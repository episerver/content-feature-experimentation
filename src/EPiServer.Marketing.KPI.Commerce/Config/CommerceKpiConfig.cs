using EPiServer.ServiceLocation;
using Mediachase.Commerce.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Commerce.Config
{
    [ServiceConfiguration(typeof(ICommerceKpiConfig))]
    public class CommerceKpiConfig : ICommerceKpiConfig
    {
        private readonly Injected<IMarketService> _marketService;
        private static object _lock = new object();

        public string PreferredMarket
        {
            get
            {
                return CommerceKpiSettings.Current.PreferredMarket.MarketId.Value;
            }
            set
            {
                lock (_lock)
                {
                    CommerceKpiSettings.Current.PreferredMarket = _marketService.Service.GetMarket(value);
                    CommerceKpiSettings.Current.Save();
                }
            }
        }
    }
}
