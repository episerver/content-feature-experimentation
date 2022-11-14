using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Config;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack.Core.Impl.Models;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OptimizelySDK.Logger;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI
{
    //[ServiceConfiguration(ServiceType = typeof(IExperimentationClient), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ExperimentationClient : IExperimentationClient
    {
        private readonly ExperimentationRestApiOptions _restOptions;
        //private readonly ILogger _logger;
        private string APIURL = FullStackConstants.APIURL;

        public ExperimentationClient(ExperimentationRestApiOptions restOptions)
        {
            _restOptions = restOptions;

            //ServiceLocator.Current.GetInstance(out ILogger epiErrorLogger);
            //_logger = epiErrorLogger;
        }

        public ExperimentationClient()
        {
            var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();
            _restOptions = new ExperimentationRestApiOptions();
            _restOptions.RestAuthToken = options.Value.RestAuthToken; // "2:Eak6r97y47wUuJWa3ULSHcAWCqLM4OiT0gPe1PswoYKD5QZ0XwoY";
            _restOptions.ProjectId = options.Value.ProjectId; // "21972070188";
            _restOptions.VersionId = options.Value.APIVersion;
            _restOptions.Environment = options.Value.EnviromentKey;
        }

        private RestClient GetRestClient()
        {
            string version = "v2";
            if (_restOptions.VersionId < 0 || _restOptions.VersionId > 2)
                version = "v2";
            else if (_restOptions.VersionId == 1)
                version = "flags/v1";

            var client = new RestClient(APIURL + version);
            client.AddDefaultHeader("Authorization", _restOptions.RestAuthToken);
            return client;
        }

        public bool CreateOrUpdateAttribute(string key, string description = null)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes
                var request = new RestRequest($"/attributes?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var existingAttributesResponse = client.Get(request);
                if (!existingAttributesResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingAttributesResponse.ResponseStatus}");
                    return false;
                }

                var existingAttributes = JsonConvert.DeserializeObject<List<OptiAttribute>>(existingAttributesResponse.Content);
                var item = existingAttributes.FirstOrDefault(x => x.Key == key);
                if (item == null) // Create new attribute in Optimizely
                {
                    var data = new { project_id = ulong.Parse(_restOptions.ProjectId), archived = false, key, description = description ?? "" };
                    request = new RestRequest($"/attributes", Method.Post);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = client.Post(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }
                else // Update attribute in Optimizely
                {
                    if (key != item.Key || description != item.Description)
                    {
                        var data = new { project_id = ulong.Parse(_restOptions.ProjectId), archived = false, key, description = description ?? "" };
                        request = new RestRequest($"/attributes/{item.Id}", Method.Patch);//DataFormat.Json);
                        request.AddJsonBody(data);
                        var response = client.Patch(request);
                        if (!response.IsSuccessful)
                        {
                            //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                            return false;
                        }
                    }
                }

                var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                projectConfig.PollNow();

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public bool CreateEventIfNotExists(OptiEvent opEvent, out long EventID)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                throw new ArgumentNullException(nameof(_restOptions.RestAuthToken));
            }
            if (opEvent.Key == null)
                throw new ArgumentNullException(nameof(opEvent.Key));

            try
            {
                var client = GetRestClient();

                // Get a list of existing events
                var request = new RestRequest($"/events?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var existingEventsResponse = client.Get(request);
                if (!existingEventsResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingEventsResponse.ResponseStatus}");
                    EventID = 0;
                    return false;
                }

                var existingEvents = JsonConvert.DeserializeObject<List<OptiEvent>>(existingEventsResponse.Content);
                var item = existingEvents.FirstOrDefault(x => x.Key == opEvent.Key);
                if (item == null) // Create new event in Optimizely
                {
                    var data = JsonConvert.SerializeObject(opEvent);
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/custom_events", Method.Post);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = client.Post(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        EventID = 0;
                        return false;
                    }

                    var event1 = JsonConvert.DeserializeObject<OptiEvent>(response.Content);
                    EventID = Convert.ToInt64( event1.Id);
                    return true;


                }
                else
                {
                    EventID = item.Id;
                    return true;
                }
                EventID = 0;
                return false;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse event data from Optimizely", e);
                EventID = 0;
                return false;
            }
        }

        public bool GetExperimentResult(out OptiExperimentResults opResults, long ExperimentID)
        {
            opResults = new OptiExperimentResults();
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (ExperimentID <= 0 )
                throw new ArgumentNullException(nameof(ExperimentID));

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes
                var request = new RestRequest($"/experiments/{ExperimentID}/results", Method.Get);//DataFormat.Json);
                var existingAttributesResponse = client.Get(request);

                opResults = JsonConvert.DeserializeObject<OptiExperimentResults>(existingAttributesResponse.Content);
                
                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateFlag(OptiFlag optiFlag)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (optiFlag.Key== null)
                throw new ArgumentNullException(nameof(optiFlag.Key));

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{optiFlag.Key}", Method.Get);//DataFormat.Json);
                var existingAttributesResponse = client.Get(request);

                var existingFlag = JsonConvert.DeserializeObject<OptiFlag>(existingAttributesResponse.Content);
                //var item = existingFlag.FirstOrDefault(x => x.Key == optiFlag.Key);
                if (existingFlag.Key == null) // Create new attribute in Optimizely
                {
                    var data = JsonConvert.SerializeObject(optiFlag);
                    //var data = new { key = optiFlag.Key, name = optiFlag.Name, description = optiFlag.Description ?? "" , outlier_filtering_enabled = false, variable_definitions = optiFlag.VariableDefinition };
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags", Method.Post);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = await client.PostAsync(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }
                else // Update attribute in Optimizely
                {
                    OptiFlagUpdate opFlagUpdate = new OptiFlagUpdate();
                    opFlagUpdate.Op = "replace";
                    opFlagUpdate.Path = "/" + existingFlag.Key + "/description";
                    opFlagUpdate.Value = optiFlag.Description;

                    List<OptiFlagUpdate> flagsToUpdate = new List<OptiFlagUpdate>();
                    flagsToUpdate.Add(opFlagUpdate);

                    var data = JsonConvert.SerializeObject(flagsToUpdate.ToArray());
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags", Method.Patch);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = await client.PatchAsync(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }


        public async Task<bool> EnableExperiment()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            
            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset/enabled", Method.Post);//DataFormat.Json);
                    
                    var response = await client.PostAsync(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public async Task<bool> DisableExperiment(string FlagKey)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{FlagKey}/environments/{_restOptions.Environment}/ruleset/disabled", Method.Post);//DataFormat.Json);

                var response = await client.PostAsync(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public bool GetExperimentID(out OptiFetchFlagRuleSet fetchedFlagRuleSet, string flagKey, string experimentKey)
        {
            fetchedFlagRuleSet = new OptiFetchFlagRuleSet();
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }


            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{flagKey}/environments/{_restOptions.Environment}/ruleset", Method.Get);//DataFormat.Json);
                var existingAttributesResponse = client.Get(request);
                if (!existingAttributesResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingAttributesResponse.ResponseStatus}");
                    return false;
                }
                var jsonContent = existingAttributesResponse.Content;
                jsonContent = jsonContent.Replace(experimentKey, FullStackConstants.ReplaceABTestExperiment);
                if (!jsonContent.Contains(experimentKey))
                    fetchedFlagRuleSet = JsonConvert.DeserializeObject<OptiFetchFlagRuleSet>(jsonContent);

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }
        public async Task<bool> CreateFlagRuleSet(List<OptiFlagRulesSet> optiFlagRuleSet)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (optiFlagRuleSet.Count < 1)
                throw new ArgumentNullException(nameof(optiFlagRuleSet));

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset", Method.Get);//DataFormat.Json);
                //sleep for 3 seconds before checking if flag is created asynchronously...
                System.Threading.Thread.Sleep(1500);
                var existingAttributesResponse = await client.GetAsync(request);
                if (!existingAttributesResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingAttributesResponse.ResponseStatus}");
                    return false;
                }

                var existingFlag = JsonConvert.DeserializeObject<OptiFetchFlagRuleSet>(existingAttributesResponse.Content);
                //if data is found in ruleset, don't do anything.
                if (existingFlag.rule_priorities.Count == 0)
                {
                    var data = JsonConvert.SerializeObject(optiFlagRuleSet.ToArray());
                    data = data.ToString().Replace("ValueClass", "value");
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset", Method.Patch);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = await client.PatchAsync(request);
                    //sleep for 3 second just to make sure Flag is created asynchronously
                    System.Threading.Thread.Sleep(2500);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }
                else
                {
                    //Flag Ruleset already exists, set op to replace and run patch command

                    OptiUpdateFlagRuleSet frs1 = new OptiUpdateFlagRuleSet();
                    frs1.Op = "replace";
                    frs1.Path = "/rules/" + existingFlag.rule_priorities[0] + "/percentage_included";
                    frs1.Value = optiFlagRuleSet[0].ValueClass.PercentageIncluded.ToString();

                    OptiUpdateFlagRuleSet frs2 = new OptiUpdateFlagRuleSet();
                    frs2.Op = "replace";
                    frs2.Path = "/rules/" + existingFlag.rule_priorities[0] + "/description";
                    frs2.Value = optiFlagRuleSet[0].ValueClass.Description;

                    List<OptiUpdateFlagRuleSet> ruleSetLists = new List<OptiUpdateFlagRuleSet>();
                    ruleSetLists.Add(frs1);
                    ruleSetLists.Add(frs2);

                    var data = JsonConvert.SerializeObject(ruleSetLists.ToArray());
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset", Method.Patch);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = await client.PatchAsync(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }


                EnableExperiment();
                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public List<OptiFeature> GetFeatureList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/features?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiFeature>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }

        public List<OptiAttribute> GetAttributeList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/attributes?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiAttribute>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
            }

            return null;
        }

        public List<OptiEvent> GetEventList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/events?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiEvent>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse event data from Optimizely", e);
            }

            return null;
        }

        public List<OptiEnvironment> GetEnvironmentList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/environments?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiEnvironment>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse environment data from Optimizely", e);
            }

            return null;
        }

        public List<OptiExperiment> GetExperimentList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/experiments?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiExperiment>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }

        public OptiExperiment GetExperiment(long experimentId)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/experiments/{experimentId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var item = JsonConvert.DeserializeObject<OptiExperiment>(response.Content);
                return item;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }

        public OptiExperiment GetExperiment(string experimentKey)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var allExperiments = GetExperimentList();
                var experiment = allExperiments.Where(x => x.Key == experimentKey).ToList();
                if (experiment.Count() == 1)
                    return experiment.First();
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }
    }
}
