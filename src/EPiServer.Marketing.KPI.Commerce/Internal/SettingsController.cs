using EPiServer.Authorization;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Commerce.Config;
using EPiServer.Marketing.KPI.Commerce.ViewModels;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Web.Mvc;
using Mediachase.Commerce.Markets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.KPI.Commerce.Internal
{
    public class SettingsController : Controller
    {
        private readonly Injected<IMarketService> _marketService;
        private readonly Injected<LocalizationService> _localizationService;
        private readonly Injected<ICommerceKpiConfig> _commerceKpiConfig;

        public SettingsController()
        {
        }

        public ActionResult Index()
        {
            if (!User.IsInRole(Roles.CmsAdmins))
            {
                throw new AccessDeniedException();
            }

            return View(new SettingsViewModel
            {
                MarketList = GetMarketOptions(),
                PreferredMarket = _marketService.Service.GetMarket(_commerceKpiConfig.Service.PreferredMarket).MarketId.Value,
            });
        }

        [HttpPost]
        public ActionResult Save([FromBody] SettingsRequest request)
        {
            try
            {
                CommerceKpiSettings.Current.PreferredMarket = _marketService.Service.GetMarket(request.PreferredMarket);

                CommerceKpiSettings.Current.Save();

                return Ok(_localizationService.Service.GetString("/abtesting/admin/success","Saved Successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IEnumerable<MarketOption> GetMarketOptions()
        {
            var MarketList = new List<MarketOption>();

            var availableMarkets = _marketService.Service.GetAllMarkets();

            foreach (var market in availableMarkets)
            {
                MarketList.Add(new MarketOption 
                { 
                    Name = market.MarketName, 
                    IdValue = market.MarketId.Value
                });
            }
            return MarketList;
        }

        public class SettingsRequest
        {
            public string PreferredMarket { get; set; }
        }
}
}
