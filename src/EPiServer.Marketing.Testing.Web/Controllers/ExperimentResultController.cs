using EPiServer.Logging;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Config;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack.Core.Impl.Models;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    public class ExperimentResultController : Controller
    {
        private IMarketingTestingWebRepository _webRepo;
        private ILogger _logger;
        private IEpiserverHelper _episerverHelper;
        private Injected<IExperimentationClient> _experimentationClient;
        [ExcludeFromCodeCoverage]
        public ExperimentResultController()
        {
            
        _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _logger = LogManager.GetLogger();
        }
        public IActionResult Index(string fs_FlagKey, string fs_ExperimentKey)
        {
            try
            {
                
                var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();
                ExperimentationRestApiOptions _restOptions = new ExperimentationRestApiOptions();
                _restOptions.RestAuthToken = options.Value.RestAuthToken; // "2:Eak6r97y47wUuJWa3ULSHcAWCqLM4OiT0gPe1PswoYKD5QZ0XwoY";
                _restOptions.ProjectId = options.Value.ProjectId; // "21972070188";
                _restOptions.VersionId = options.Value.APIVersion; //1
                _restOptions.Environment = options.Value.EnviromentKey; // "production";
                IExperimentationClient _expClient = new ExperimentationClient(_restOptions);
                OptiFetchFlagRuleSet newfetchedRuleSet = new OptiFetchFlagRuleSet();
                bool foundExperimentID = _expClient.GetExperimentID(out newfetchedRuleSet, fs_FlagKey, fs_ExperimentKey);
                OptiExperimentResults opResults = new OptiExperimentResults();
                if (newfetchedRuleSet.rules != null)
                {
                    long ExperimentID = GetExeperimentIDFromURL(newfetchedRuleSet.rules.AB_Test_Experiment?.fetch_results_ui_url);
                    _restOptions.ExperimentID = ExperimentID;
                    _restOptions.VersionId = 2;
                    _expClient = new ExperimentationClient(_restOptions);
                    var exResult = _expClient.GetExperimentResult(out opResults, ExperimentID);
                }
                

                return View("~/Views/ExperimentResult/Index.cshtml", opResults);
            }
            catch (Exception e)
            {
                _logger.Error("Internal error getting test using content Guid : "
                    + fs_FlagKey, e);
            }

            return new ContentResult();
        }

        private long GetExeperimentIDFromURL(string experimentURL)
        {
            string experimentID = experimentURL.Substring(experimentURL.LastIndexOf("/") + 1, experimentURL.Length - experimentURL.LastIndexOf("/") - 1);

            if (!string.IsNullOrEmpty(experimentID))
                return Convert.ToInt64( experimentID);

            return 0;
        }
    }
}
