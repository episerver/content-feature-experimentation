﻿using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    /// <summary>
    /// The default site context will resolve the currency to be the default currency for the current market.
    /// This site context makes sure the the Commerce API's uses the same currency that the user selects in the currency selector.
    /// This replaces the ordinary DefaultSiteContext by registering it in the container in <see cref="SiteInitialization"/>.
    /// </summary>
    public class CustomCurrencySiteContext : DefaultSiteContext
    {
        private Lazy<Currency> _lazyCurrency;
        private Lazy<string> _lazyLanguageName;

        public CustomCurrencySiteContext(ICurrencyService currencyService, ICurrentMarket currentMarket, LanguageService languageService, IWebHostEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor) 
            : base(currentMarket, hostEnvironment, httpContextAccessor)
        {
            _lazyCurrency = new Lazy<Currency>(() => currencyService.GetCurrentCurrency());
            _lazyLanguageName = new Lazy<string>(() => languageService.GetCurrentLanguage().TwoLetterISOLanguageName);
        }

        public override Currency Currency
        {
            get { return _lazyCurrency.Value; }
            set { _lazyCurrency = new Lazy<Currency>(() => value); }
        }

        public override string LanguageName
        {
            get { return _lazyLanguageName.Value; }
            set { _lazyLanguageName = new Lazy<string>(() => value); }
        }
    }
}