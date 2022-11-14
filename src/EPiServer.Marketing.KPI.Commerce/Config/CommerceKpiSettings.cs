using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EPiServer.Marketing.KPI.Commerce.Config
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class CommerceKpiSettings : IDynamicData
    {
        private static readonly Injected<IKpiManager> _kpiManager;
        private static readonly Injected<IMarketService> _marketService;
        public Identity Id { get; set; }      

        public IMarket PreferredMarket { get; set; }

        internal static CommerceKpiSettings _currentSettings;

        [ExcludeFromCodeCoverage]
        public static CommerceKpiSettings Current
        {
            get
            {
                if (_currentSettings == null)
                {
                    _currentSettings = new CommerceKpiSettings();

                    var preferredMarket = _kpiManager.Service.GetCommerceSettings();

                    _currentSettings.PreferredMarket = !string.IsNullOrEmpty(preferredMarket.CommerceCulture) ? 
                        _currentSettings.PreferredMarket = _marketService.Service.GetMarket(preferredMarket.CommerceCulture)
                        :
                        _currentSettings.PreferredMarket = _marketService.Service.GetMarket(MarketId.Default.Value);
                    
                }

                return _currentSettings;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommerceKpiSettings"/> class.
        /// </summary>
        public CommerceKpiSettings()
        {
            PreferredMarket = _marketService.Service.GetMarket(MarketId.Default.Value); 
        }

        [ExcludeFromCodeCoverage]
        public void Save()
        {
            var settingsToSave = new CommerceData();
            settingsToSave.CommerceCulture = PreferredMarket.MarketId.Value;
            settingsToSave.preferredFormat = PreferredMarket.DefaultCurrency.Format;
            _kpiManager.Service.SaveCommerceSettings(settingsToSave);
            
            _currentSettings = this;
        }

        /// <summary>
        /// Clears setting values that being stored in memory.
        /// </summary>
        public void Reset()
        {
            _currentSettings = null;
        }      
    }   
}
