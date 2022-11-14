using EPiServer.Authorization;
using EPiServer.Framework.Localization;
using EPiServer.Shell;
using EPiServer.Shell.Navigation;
using System.Collections.Generic;

namespace EPiServer.Marketing.KPI.Commerce
{
    /// <summary>
    /// Adds navigation for components in this package.
    /// </summary>
    [MenuProvider]
    public class MenuProvider : IMenuProvider
    {
        private readonly LocalizationService _localizationService;

        private const string MarketingToolSettingsPath = MenuPaths.Global + "/addons";

        /// <summary>
        /// Initializes a new instance of <see cref="MenuProvider"/>
        /// </summary>
        /// <param name="localizationService"></param>
        public MenuProvider(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets the mene items provided by this provider.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MenuItem> GetMenuItems()
        {
            return new List<MenuItem>()
            {
                new UrlMenuItem(_localizationService.GetString("", "AB Testing Commerce Settings"),
                    MarketingToolSettingsPath + "/marketingtools/commercesettings",
                    Paths.ToResource(GetType(), "Settings"))
                {
                    SortIndex = 100,
                    Alignment = MenuItemAlignment.Left,
                    IsAvailable = (context) => true,
                    AuthorizationPolicy = CmsPolicyNames.CmsAdmin
                }
            };
        }
    }
}