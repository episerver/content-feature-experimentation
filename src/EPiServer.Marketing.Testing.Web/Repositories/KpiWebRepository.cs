using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using System.Text.Json;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IKpiWebRepository), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiWebRepository : IKpiWebRepository
    {
        private IServiceProvider _locator;
        private IKpiManager kpiManager;
        private readonly string kpiTypeKey = "kpiType";      

        public KpiWebRepository()
        {
            _locator = ServiceLocator.Current;
            kpiManager = _locator.GetInstance<IKpiManager>();
        }

        public KpiWebRepository(IServiceProvider sl)
        {
            _locator = sl;
            kpiManager = _locator.GetInstance<IKpiManager>();
        }
        
        /// <summary>
        /// Retrieves all KPIs available to the system.
        /// </summary>
        /// <returns></returns>
        public List<KpiTypeModel> GetKpiTypes()
        {
            IKpiManager kpiManager = _locator.GetInstance<IKpiManager>();
            List<KpiTypeModel> kpiData = new List<KpiTypeModel>();

            var KpiTypes = kpiManager.GetKpiTypes();
            foreach (Type t in KpiTypes)
            {
                if (t.Name == "ContentComparatorKPI") // Ignore other two kpi's for now
                    kpiData.Add(new KpiTypeModel() {kpi = Activator.CreateInstance(t) as IKpi, kpiType = t.AssemblyQualifiedName});
            }
            return kpiData;
        }

        public Guid SaveKpi(IKpi kpiInstance)
        {
            return kpiManager.Save(kpiInstance);
        }

        public IList<Guid> SaveKpis(IList<IKpi> kpiInstances)
        {
            return kpiManager.Save(kpiInstances);
        }      

        public List<Dictionary<string,string>> DeserializeJsonKpiFormCollection(string jsonFormDataCollection)
        {
            List<Dictionary<string, string>> kpiFormData = new List<Dictionary<string, string>>();
            List<string> values = JsonSerializer.Deserialize<List<string>>(jsonFormDataCollection);
            values.ForEach(value =>
            {
                if (value.Contains(kpiTypeKey))
                    kpiFormData.Add(JsonSerializer.Deserialize<Dictionary<string, string>>(value));
            });
            return kpiFormData;
        }

        public IKpi ActivateKpiInstance(Dictionary<string, string> kpiFormData)
        {
            //Create a kpi instance based on the incomming type
            IKpi kpiInstance;
            var kpi = Activator.CreateInstance(Type.GetType(kpiFormData[kpiTypeKey]));
            if (kpi is IFinancialKpi)
            {
                var financialKpi = kpi as IFinancialKpi;
                financialKpi.PreferredFinancialFormat = kpiManager.GetCommerceSettings();
                kpiInstance = financialKpi as IKpi;
            }
            else
            {
                kpiInstance = kpi as IKpi;
            }

            return kpiInstance;
        }

        public IKpi GetKpiInstance(Guid kpiId)
        {
           return kpiManager.Get(kpiId);
        }
    }
}
