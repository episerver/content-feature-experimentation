using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Commerce.ViewModels
{
    public class SettingsViewModel
    {
        #region Selection options
        public IEnumerable<MarketOption> MarketList { get; set; }
        #endregion

        public string PreferredMarket { get; set; }
    }
}
