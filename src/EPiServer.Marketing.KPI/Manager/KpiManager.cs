﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.DataAccess;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using EPiServer.Data.Dynamic;
using EPiServer.Logging;
using System.Globalization;
using EPiServer.Globalization;

namespace EPiServer.Marketing.KPI.Manager
{
    /// <inheritdoc />
    [ServiceConfiguration(ServiceType = typeof(IKpiManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiManager : IKpiManager
    {
        private readonly Injected<IKpiDataAccess> _dataAccess;        
        private ILogger _logger;

        /// <summary>
        /// Figures out if the database needs to be configured before setting up the data access layer.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public KpiManager()
        {            
            _logger = LogManager.GetLogger();
        }

        /// <inheritdoc />
        public IKpi Get(Guid kpiId)
        {
            return ConvertToManagerKpi(_dataAccess.Service.Get(kpiId));
        }

        /// <inheritdoc />
        public List<IKpi> GetKpiList()
        {
            return _dataAccess.Service.GetKpiList().Select(dalTest => ConvertToManagerKpi(dalTest)).ToList();
        }

        /// <inheritdoc />
        public Guid Save(IKpi kpi)
        {
            return Save(new List<IKpi>() { kpi }).First();
        }

        /// <inheritdoc />
        public IList<Guid> Save(IList<IKpi> kpis)
        {
            return _dataAccess.Service.Save(ConvertToDalKpis(kpis));
        }

        /// <inheritdoc />
        public void Delete(Guid kpiId)
        {
            _dataAccess.Service.Delete(kpiId);
        }

        /// <inheritdoc />
        public IEnumerable<Type> GetKpiTypes()
        {
            var type = typeof(IKpi);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = new List<Type>();

            foreach (var assembly in assemblies)
            {
                IEnumerable<Type> kpiTypes;
                try
                {
                    kpiTypes = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract && p != typeof(Kpi));
                }
                catch (ReflectionTypeLoadException e)  // This exception gets thrown if any dependencies for an assembly can't be found.
                {
                    // In this case, we just get whatever kpis that we can and ignore any that have missing dependencies
                    kpiTypes = e.Types.Where(t => t != null && type.IsAssignableFrom(t) && t != typeof(Kpi)).ToArray();

                    foreach (var loaderException in e.LoaderExceptions)
                    {
                        _logger.Error("AB Testing: ", loaderException);
                    }
                }

                types.AddRange(kpiTypes);
            }

            return types;
        }

        /// <inheritdoc />
        public long GetDatabaseVersion(string schema, string contextKey, bool setupDataAccess = false)
        {
            return _dataAccess.Service.GetDatabaseVersion(schema, contextKey);
        }

        /// <inheritdoc />
        public void SaveCommerceSettings(CommerceData commerceSettings)
        {            
            var store = GetDataStore(typeof(CommerceData));
            store.Save(commerceSettings);
        }

        /// <inheritdoc />
        public CommerceData GetCommerceSettings()
        {
            var store = GetDataStore(typeof(CommerceData));
            //Add culture for preferred language in case cannot get correct SystemLanguage 
            var settings = store.LoadAll<CommerceData>().OrderByDescending(x => x.Id.StoreId).FirstOrDefault() ?? new CommerceData { CommerceCulture = "DEFAULT", preferredFormat =
                !String.IsNullOrEmpty(CultureInfo.CurrentCulture.Name) ? CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name).NumberFormat : CultureInfo.CreateSpecificCulture(CultureInfo.CurrentUICulture.Name).NumberFormat };
            return settings;
        }

        private DynamicDataStore GetDataStore(Type t)
        {
            return DynamicDataStoreFactory.Instance.GetStore(t) ?? DynamicDataStoreFactory.Instance.CreateStore(t);
        }

        /// <summary>
        /// Serialize the KPI to a JSON string and save it in the properties field.
        /// </summary>
        /// <param name="kpis">List of KPIs to save to the DB (i.e. contentcomparatorkpi, timeonpagekpi, etc.)</param>
        /// <returns>EF KPI object to save in the DB.</returns>
        private List<DalKpi> ConvertToDalKpis(IList<IKpi> kpis)
        {
            var dalKpis = new List<DalKpi>();

            foreach (var kpi in kpis)
            {
                if (Guid.Empty == kpi.Id)
                {
                    // if the kpi.id is null, its because we are creating a new one.
                    kpi.Id = Guid.NewGuid();
                    kpi.CreatedDate = DateTime.UtcNow;
                    kpi.ModifiedDate = DateTime.UtcNow;
                }

                var serializedKpi = JsonConvert.SerializeObject(kpi);

                var dalKpi = new DalKpi()
                {
                    Id = kpi.Id,
                    ClassName = kpi.GetType().AssemblyQualifiedName,
                    Properties = serializedKpi,
                    CreatedDate = kpi.CreatedDate,
                    ModifiedDate = kpi.ModifiedDate
                };

                dalKpis.Add(dalKpi);
            }

            return dalKpis;
        }

        /// <summary>
        /// Takes EF Kpi from the db and deserializes the Json string from properties, then creates/populates the correct kpi class instance.
        /// </summary>
        /// <param name="dalKpi">EF Kpi object.</param>
        /// <returns>User defined Kpi object.</returns>
        private IKpi ConvertToManagerKpi(DalKpi dalKpi)
        {
            // split up saved assembly/class info
            var parts = dalKpi.ClassName.Split(',');
            
            // create class instance of specific type of kpi
            var kpi = Activator.CreateInstance(parts[1], parts[0]);
            
            // unwrap to actually get the kpi class object so we can populate its properties
            var managerKpi = (IKpi)kpi.Unwrap();
            
            // fill in class properties with values saved to the db
            JsonConvert.PopulateObject(dalKpi.Properties, managerKpi);

            return managerKpi;
        }

    }


}
