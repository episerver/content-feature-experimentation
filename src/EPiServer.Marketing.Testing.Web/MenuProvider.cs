using EPiServer.Authorization;
using EPiServer.Framework.Localization;
using EPiServer.Security;
using EPiServer.Shell;
using EPiServer.Shell.Modules;
using EPiServer.Shell.Navigation;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Marketing.Testing.Web
{
    [MenuProvider]
    public class MenuProvider : IMenuProvider
    {
        private readonly ModuleTable _moduleTable;
        private readonly LocalizationService _localizationService;

        private const string MarketingToolSettingsPath = MenuPaths.Global + "/addons";

        public MenuProvider(LocalizationService localizationService, ModuleTable moduleTable)
        {
            _localizationService = localizationService;
            _moduleTable = moduleTable;
        }

        public IEnumerable<MenuItem> GetMenuItems()
        {
            var moduleFound = _moduleTable.TryGetModule(GetType().Assembly, out var module);

            if (!moduleFound)
            {
                return Enumerable.Empty<MenuItem>();
            }
            return new List<MenuItem>
            {
                new SectionMenuItem(_localizationService.GetString("", "Addons"),
                    MarketingToolSettingsPath)
                {
                    SortIndex = 10,
                    Alignment = MenuItemAlignment.Left,
                    IsAvailable = (context) => true
                },
                new UrlMenuItem(_localizationService.GetString("", "Marketing Tools"),
                    MarketingToolSettingsPath + "/marketingtools",
                    Paths.ToResource(GetType(), "Setting"))
                {
                    SortIndex = 10,
                    Alignment = MenuItemAlignment.Left,
                    IsAvailable = (context) => true,
                    AuthorizationPolicy = CmsPolicyNames.CmsAdmin
                },
                 new UrlMenuItem(_localizationService.GetString("/abtesting/admin/displayname", "AB Testing Configuration"),
                    MarketingToolSettingsPath + "/marketingtools/setting",
                    Paths.ToResource(GetType(), "Setting"))
                {
                    SortIndex = SortIndex.Early + 110,
                    Alignment = MenuItemAlignment.Left,
                    AuthorizationPolicy = CmsPolicyNames.CmsAdmin
                }               
            };
        }
    }

}
