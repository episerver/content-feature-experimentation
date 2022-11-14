﻿using EPiServer.Framework.Web.Resources;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.ClientKPI
{
    /// <summary>
    /// Handles client side KPI markup.
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IClientKpiInjector), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ClientKpiInjector : IClientKpiInjector
    {
        public const string ClientCookieName = "ClientKpiList";

        private static readonly string _clientKpiWrapperScript;
        private static readonly string _clientKpiScriptTemplate;
        private static readonly string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        private readonly ITestingContextHelper _contextHelper;
        private readonly IMarketingTestingWebRepository _testRepo;
        private readonly IKpiManager _kpiManager;
        private readonly ILogger _logger;
        private readonly IHttpContextHelper _httpContextHelper;
        
        /// <summary>
        /// Constructor
        /// </summary>
        static ClientKpiInjector()
        {
            _clientKpiWrapperScript = ReadScriptFromAssembly(
                "EPiServer.Marketing.Testing.Web.EmbeddedScriptFiles.ClientKpiWrapper.js"
            );

            _clientKpiScriptTemplate = ReadScriptFromAssembly(
                "EPiServer.Marketing.Testing.Web.EmbeddedScriptFiles.ClientKpiSuccessEvent.js"
            );
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ClientKpiInjector()
        {
            _contextHelper = ServiceLocator.Current.GetInstance<ITestingContextHelper>();
            _testRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _logger = LogManager.GetLogger();
            _httpContextHelper = new HttpContextHelper();
            _kpiManager = new KpiManager();

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceLocator">Dependency container</param>
        public ClientKpiInjector(IServiceProvider serviceLocator)
        {
            _contextHelper = serviceLocator.GetInstance<ITestingContextHelper>();
            _testRepo = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _logger = LogManager.GetLogger();
            _httpContextHelper = serviceLocator.GetInstance<IHttpContextHelper>();
            _kpiManager = serviceLocator.GetInstance<IKpiManager>();
        }

        /// <summary>
        /// Checks for any client KPIs which may be assigned to the test and injects the provided
        /// markup via the current response.
        /// </summary>
        /// <param name="kpis">List of KPIs.</param>
        /// <param name="cookieData">Cookie data related to the current test and KPIs.</param>
        public void ActivateClientKpis(List<IKpi> kpis, TestDataCookie cookieData)
        {
            if (ShouldActivateKpis(cookieData))
            {
                var kpisToActivate = kpis.Where(kpi => kpi is IClientKpi).ToList();

                if (kpisToActivate.Any(kpi => !_httpContextHelper.HasItem(kpi.Id.ToString())))
                {
                    kpisToActivate.ForEach(kpi => _httpContextHelper.SetItemValue(kpi.Id.ToString(), true));

                    AppendClientKpiScript(kpisToActivate.ToDictionary(kpi => kpi.Id, kpi => cookieData));
                }
            }
        }

        /// <summary>
        /// Gets the associated script for a client KPI and appends it.
        /// </summary>
        public void AppendClientKpiScript(Dictionary<Guid, TestDataCookie> clientKpis)
        {
            //Check to make sure we have client kpis to inject
            if (ShouldInjectKpiScript(clientKpis))
            {
                var clientKpiScript = new StringBuilder()
                    .Append("//<!-- ABT Script -->")
                    .Append(_clientKpiWrapperScript);

                //Add clients custom evaluation scripts
                foreach (var kpiToTestCookie in clientKpis)
                {
                    var kpiId = kpiToTestCookie.Key;
                    var testCookie = kpiToTestCookie.Value;
                    var test = _testRepo.GetTestById(testCookie.TestId, true);
                    var variant = test?.Variants.FirstOrDefault(v => v.Id.ToString() == testCookie.TestVariantId.ToString());

                    if (variant == null)
                    {
                        _logger.Debug($"Could not find test {testCookie.TestId} or variant {testCookie.TestVariantId} when preparing client script for KPI {kpiId}.");
                    }
                    else
                    {
                        var kpi = _kpiManager.Get(kpiId);
                        var clientKpi = kpi as IClientKpi;
                        var individualKpiScript = BuildClientScript(kpi.Id, test.Id, variant.ItemVersion, clientKpi.ClientEvaluationScript);

                        individualKpiScript = individualKpiScript.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal) ? individualKpiScript.Remove(0, _byteOrderMarkUtf8.Length) : individualKpiScript;
                        clientKpiScript.Append(individualKpiScript);
                    }
                }

                Inject(clientKpiScript.ToString());
            }
        }
        /// <summary>
        /// Determines whether or not client-side KPI scripts need to be injected into the response.
        /// </summary>
        /// <param name="clientKpiList">Collection of client KPIs</param>
        /// <returns>True if the script needs to be injected, false otherwise</returns>
        private bool ShouldInjectKpiScript(Dictionary<Guid, TestDataCookie> clientKpiList)
        {
            return _contextHelper.IsHtmlContentType() && 
                       !_contextHelper.IsInSystemFolder() &&
                       clientKpiList.Any(kpi => _httpContextHelper.HasItem(kpi.Key.ToString()));
        }

        /// <summary>
        /// Determines whether or not client KPIs should be activated for the current request.
        /// </summary>
        /// <param name="cookieData">Test cookie data</param>
        /// <returns>True if client KPIs should be activated, false otherwise</returns>
        private bool ShouldActivateKpis(TestDataCookie cookieData)
        {
            return _contextHelper.IsHtmlContentType() &&
                     !_contextHelper.IsInSystemFolder() && (!cookieData.Converted || cookieData.AlwaysEval);
        }

        /// <summary>
        /// Injects the specified script into the response stream.
        /// </summary>
        /// <param name="script">Script to inject</param>
        private void Inject(string script)
        {
            var requiredResources = ServiceLocator.Current.GetInstance<IRequiredClientResourceList>();
            requiredResources.RequireScriptInline(script).AtFooter();
        }

        /// <summary>
        /// Renders the template script for an individual client KPI with the given parameters.
        /// </summary>
        /// <param name="kpiId">ID of KPI</param>
        /// <param name="testId">ID of test</param>
        /// <param name="versionId">Variant item version</param>
        /// <param name="clientScript">KPI evaluation script</param>
        /// <returns>Script rendered from the template</returns>
        private string BuildClientScript(Guid kpiId, Guid testId, int versionId, string clientScript)
        {
            return _clientKpiScriptTemplate
                .Replace("{KpiGuid}", kpiId.ToString())
                .Replace("{ABTestGuid}", testId.ToString())
                .Replace("{VersionId}", versionId.ToString())
                .Replace("{KpiClientScript}", clientScript);
        }

        /// <summary>
        /// Reads the specified resource from the current assembly.
        /// </summary>
        /// <param name="resourceName">Name of resource</param>
        /// <returns>Resource that was loaded</returns>
        private static string ReadScriptFromAssembly(string resourceName)
        {
            var retString = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            var scriptResource = resourceName;
            var resourceNames = assembly.GetManifestResourceNames();

            using (Stream resourceStream = assembly.GetManifestResourceStream(scriptResource))
            using (StreamReader reader = new StreamReader(resourceStream, Encoding.UTF8, false))
            { 
                retString = reader.ReadToEnd();
            }
            retString = retString.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal) ? 
                        retString.Remove(0, _byteOrderMarkUtf8.Length) : retString;
            return retString;
        }
    }
}
