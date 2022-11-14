using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.KPI.DataAccess
{
    [ServiceConfiguration(ServiceType = typeof(IKpiDataAccess), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiDataAccess : IKpiDataAccess
    {
        public readonly Injected<IRepository> _repository;
        internal bool _UseEntityFramework;
        public bool IsDatabaseConfigured;

        [ExcludeFromCodeCoverage]
        public KpiDataAccess()
        {
        }

        public KpiDataAccess(IRepository repository)
        {
            _repository.Service = repository;
        }

        /// <summary>
        /// Deletes KPI object from the DB.
        /// </summary>
        /// <param name="kpiId">ID of the KPI to delete.</param>
        public void Delete(Guid kpiId)
        {
            if (!IsDatabaseConfigured)
            {
                throw new DatabaseDoesNotExistException();
            }
            DeleteHelper(_repository.Service, kpiId);
        }

        private void DeleteHelper(IRepository repo, Guid kpiId)
        {
            repo.DeleteKpi(kpiId);
            repo.SaveChanges();
        }

        /// <summary>
        /// Returns a KPI object based on its ID.
        /// </summary>
        /// <param name="kpiId">ID of the KPI to retrieve.</param>
        /// <returns>KPI object.</returns>
        public DalKpi Get(Guid kpiId)
        {
            if (!IsDatabaseConfigured)
            {
                throw new DatabaseDoesNotExistException();
            }
            return GetHelper(_repository.Service, kpiId);
        }

        private DalKpi GetHelper(IRepository repo, Guid kpiId)
        {
            return repo.GetById(kpiId);
        }

        /// <summary>
        /// Gets the whole list of KPI objects.
        /// </summary>
        /// <returns>List of KPI objects.</returns>
        public List<DalKpi> GetKpiList()
        {
            if (!IsDatabaseConfigured)
            {
                throw new DatabaseDoesNotExistException();
            }
            return GetKpiListHelper(_repository.Service);
        }

        private List<DalKpi> GetKpiListHelper(IRepository repo)
        {
            return repo.GetAll().ToList();
        }

        /// <summary>
        /// Adds or updates a KPI object.
        /// </summary>
        /// <param name="kpiObject">ID of the KPI to add/update.</param>
        /// <returns>The ID of the KPI object that was added/updated.</returns>
        public Guid Save(DalKpi kpiObject)
        {
            return Save(new List<DalKpi>() { kpiObject }).First();
        }

        /// <summary>
        /// Adds or updates multiple KPI objects.
        /// </summary>
        /// <param name="kpiObjects">List of KPIs to add/update.</param>
        /// <returns>The IDs of the KPI objects that were added/updated.</returns>
        public IList<Guid> Save(IList<DalKpi> kpiObjects)
        {
            if (!IsDatabaseConfigured)
            {
                throw new DatabaseDoesNotExistException();
            }
            return SaveHelper(_repository.Service, kpiObjects);
        }

        private IList<Guid> SaveHelper(IRepository repo, IList<DalKpi> kpiObjects)
        {
            var ids = new List<Guid>();

            foreach (var kpiObject in kpiObjects)
            {
                var kpi = repo.GetById(kpiObject.Id) as DalKpi;
                Guid id;

                // if a test doesn't exist, add it to the db
                if (kpi == null)
                {
                    repo.Add(kpiObject);
                    id = kpiObject.Id;
                }
                else
                {
                    kpi.ClassName = kpiObject.ClassName;
                    kpi.Properties = kpiObject.Properties;
                    id = kpi.Id;
                }

                ids.Add(id);
            }

            repo.SaveChanges();
            
            return ids;
        }

        public long GetDatabaseVersion(string schema, string contextKey)
        {
            if (HasTableNamed((BaseRepository)_repository.Service, "tblKeyPerformaceIndicator"))
            {
                // the sql scripts need to be run!
                IsDatabaseConfigured = true;
            }

            return IsDatabaseConfigured ? GetDatabaseVersionHelper(_repository.Service, contextKey) : 0 ;
        }

        private long GetDatabaseVersionHelper(IRepository repo, string contextKey)
        {
            var lastMigration = repo.GetDatabaseVersion(contextKey);

            // we are only interested in the numerical part of the key (i.e. 201609091719244_Initial)
            var version = lastMigration.Split('_')[0];

            return Convert.ToInt64(version);
        }

        [ExcludeFromCodeCoverage]
        private static bool HasTableNamed(BaseRepository repository, string table, string schema = "dbo")
        {
            string sql = @"SELECT CASE WHEN EXISTS
            (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA=@p0 AND TABLE_NAME=@p1) THEN 1 ELSE 0 END";

            using (var command = repository.DatabaseContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                command.Parameters.Add(new SqlParameter("@p0", schema));
                command.Parameters.Add(new SqlParameter("@p1", table));
                repository.DatabaseContext.Database.OpenConnection();

                return ((int)command.ExecuteScalar()) == 1;
            }
        }
    }
}
