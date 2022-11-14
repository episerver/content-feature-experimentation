using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.Marketing.Testing.Web.Models.Settings;
using EPiServer.Shell.Web.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [Authorize(Roles = "Administrators,WebAdmins")]
    public class SettingController : Controller
    {
        private readonly ILogger _logger;
        public SettingController()
        {
            _logger = LogManager.GetLogger();
        }

        [HttpGet]
        public ActionResult Index() => View();

        [HttpGet]
        public IActionResult Get()
        {
            var model = new SettingsViewModel
            {
                AutoPublishWinners = GetAutoPublishWinners(),
                ConfidenceLevels = GetConfidenceLevels(),
                TestDuration = AdminConfigTestSettings.Current.TestDuration,
                ParticipationPercent = AdminConfigTestSettings.Current.ParticipationPercent,
                ConfidenceLevel = AdminConfigTestSettings.Current.ConfidenceLevel,
                AutoPublishWinner = AdminConfigTestSettings.Current.AutoPublishWinner,
                IsEnabled = AdminConfigTestSettings.Current.IsEnabled,

                ABTestingConfigTitle = LocalizationService.Current.GetString("/abtesting/admin/displayname"),
                ABTestingConfigDescription = LocalizationService.Current.GetString("/abtesting/admin/description"),
                TestDurationLabel = LocalizationService.Current.GetString("/abtesting/admin/testduration"),
                ParticipationPercentLabel = LocalizationService.Current.GetString("/abtesting/admin/participationpercent"),
                AutoPublishWinnerLabel = LocalizationService.Current.GetString("/abtesting/admin/autopublishwinner"),
                ConfidenceLevelLabel = LocalizationService.Current.GetString("/abtesting/admin/confidencelevel"),
                IsEnabledLabel = LocalizationService.Current.GetString("/abtesting/admin/isenabled"),
                InValid = LocalizationService.Current.GetString("/abtesting/admin/invalid"),
                ParticipationError = LocalizationService.Current.GetString("/abtesting/admin/participationerror"),
                DurationError = LocalizationService.Current.GetString("/abtesting/admin/durationerror"),
                Success = LocalizationService.Current.GetString("/abtesting/admin/success"),
                SaveButton = LocalizationService.Current.GetString("/abtesting/admin/save"),
                CancelButton= LocalizationService.Current.GetString("/abtesting/admin/cancel")
            };
            return this.JsonData(model);
        }

        [HttpPost]
        public IActionResult Save([FromBody] SettingsRequest request)
        {
            if (request.ParticipationPercent < 1 || request.ParticipationPercent > 100)
            {
                return BadRequest(LocalizationService.Current.GetString("/abtesting/admin/participationerror"));
            }

            if (request.TestDuration < 1 || request.TestDuration > 365)
            {

                return BadRequest(LocalizationService.Current.GetString("/abtesting/admin/durationerror"));
            }

            var newSettings = new AdminConfigTestSettings()
            {
                TestDuration = request.TestDuration,
                ParticipationPercent = request.ParticipationPercent,
                ConfidenceLevel = request.ConfidenceLevel,
                AutoPublishWinner = request.AutoPublishWinner,
                IsEnabled = request.IsEnabled
            };

            newSettings.Save();
            return Ok(LocalizationService.Current.GetString("/abtesting/admin/success"));
        }

        private IEnumerable<ProviderOption> GetAutoPublishWinners()
        {
            var listItemCollection = new List<ProviderOption>()
            {
                new ProviderOption
                {
                    Label = " True ",
                    Value = "true"
                },
                new ProviderOption
                {
                    Label = " False ",
                    Value = "false"
                }
            };

            return listItemCollection;
        }

        private IEnumerable<ProviderOption> GetConfidenceLevels()
        {
            var listItemCollection = new List<ProviderOption>(){
                new ProviderOption
                {
                    Label = " 99% ",
                    Value = "99"
                },
                new ProviderOption
                {
                    Label = " 98% ",
                    Value = "98"
                },
                new ProviderOption
                {
                    Label = " 95% ",
                    Value = "95"
                },
                new ProviderOption
                {
                    Label = " 90% ",
                    Value = "90"
                }
            };

            return listItemCollection;
        }
    }
}
