﻿using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Services
{
    [ServiceConfiguration]
    public class LanguageService : IUpdateCurrentLanguage
    {
        private const string LanguageCookie = "Language";
        private readonly ICurrentMarket _currentMarket;
        private readonly CookieService _cookieService;
        private readonly IUpdateCurrentLanguage _defaultUpdateCurrentLanguage;

        public LanguageService(ICurrentMarket currentMarket, CookieService cookieService, IUpdateCurrentLanguage defaultUpdateCurrentLanguage)
        {
            _currentMarket = currentMarket;
            _cookieService = cookieService;
            _defaultUpdateCurrentLanguage = defaultUpdateCurrentLanguage;
        }

        public virtual IEnumerable<CultureInfo> GetAvailableLanguages()
        {
            return CurrentMarket.Languages;
        }

        public virtual CultureInfo GetCurrentLanguage()
        {
            return TryGetLanguage(_cookieService.Get(LanguageCookie), out var cultureInfo)
                ? cultureInfo
                : CurrentMarket.DefaultLanguage;
        }

        public void SetRoutedContent(IContent currentContent, string requestedLanguage)
        {
            if (currentContent != null)
            {
                _defaultUpdateCurrentLanguage.SetRoutedContent(currentContent, requestedLanguage);
            }
            else
            {
                var chosenLanguage = requestedLanguage;
                var cookieLanguage = _cookieService.Get(LanguageCookie);

                if (string.IsNullOrEmpty(chosenLanguage))
                {
                    if (cookieLanguage != null)
                    {
                        chosenLanguage = cookieLanguage;
                    }
                    else
                    {
                        var currentMarket = _currentMarket.GetCurrentMarket();
                        if (currentMarket?.DefaultLanguage != null)
                        {
                            chosenLanguage = currentMarket.DefaultLanguage.Name;
                        }
                    }
                }

                _defaultUpdateCurrentLanguage.SetRoutedContent(null, chosenLanguage);

                if (cookieLanguage == null || cookieLanguage != chosenLanguage)
                {
                    _cookieService.Set(LanguageCookie, chosenLanguage);
                }
            }
        }

        private bool TryGetLanguage(string language, out CultureInfo cultureInfo)
        {
            cultureInfo = null;

            if (language == null)
            {
                return false;
            }

            try
            {
                var culture = CultureInfo.GetCultureInfo(language);
                cultureInfo = GetAvailableLanguages().FirstOrDefault(c => c.Name == culture.Name);
                return cultureInfo != null;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }

        private IMarket CurrentMarket => _currentMarket.GetCurrentMarket();
    }
}