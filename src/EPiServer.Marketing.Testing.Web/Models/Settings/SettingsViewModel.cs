using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Models.Settings
{
    public class SettingsViewModel
    {
        #region Selection options
        public IEnumerable<ProviderOption> AutoPublishWinners { get; set; }        
        public IEnumerable<ProviderOption> ConfidenceLevels { get; set; }
        #endregion

        #region Localization
        public string ABTestingConfigTitle { get; set; }
        public string ABTestingConfigDescription { get; set; }
        public string TestDurationLabel { get; set; }        
        public string ParticipationPercentLabel { get; set; }        
        public string AutoPublishWinnerLabel { get; set; }        
        public string ConfidenceLevelLabel { get; set; }
        public string IsEnabledLabel { get; set; }
        public string InValid { get; set; }
        public string ParticipationError { get; set; }
        public string DurationError { get; set; }
        public string Success { get; set; }
        public string SaveButton { get; set; }
        public string CancelButton { get; set; }

        #endregion

        public int TestDuration { get; set; }
        public int ParticipationPercent { get; set; }
        public int ConfidenceLevel { get; set; }
        public bool AutoPublishWinner { get; set; }
        public bool IsEnabled { get; set; }
    }
}