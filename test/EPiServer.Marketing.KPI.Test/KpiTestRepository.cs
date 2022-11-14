using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.ServiceLocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPiServer.Marketing.KPI.Test
{
    [ServiceConfiguration(ServiceType = typeof(IRepository), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class KpiTestRepository : IRepository
    {
        public KpiTestContext TestContext { get; set; }
        public KpiDatabaseContext DatabaseContext { get; set; }

        public readonly IHistoryRepository HistoryRepository;

        public KpiTestRepository(KpiTestContext testContext)
        {
            TestContext = testContext;
        }

        public KpiTestRepository(KpiDatabaseContext dbContext)
        {
            HistoryRepository = dbContext.GetService<IHistoryRepository>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    if (TestContext != null)
                    {
                        TestContext.Dispose();
                        TestContext = null;
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Persists all changes made in DatabaseContext to the database
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public int SaveChanges()
        {
            //call save changes with no retries
            return SaveChanges(0);
        }

        /// <summary>
        /// Persists all changes made in DatabaseContext to the database
        /// </summary>
        /// <param name="retryCount">Number of times to retry save operation</param>
        /// <returns>Number of rows affected</returns>
        public int SaveChanges(int retryCount)
        {
            var records = 0;

            if (TestContext != null)
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions()
                    {
                        IsolationLevel = IsolationLevel.ReadCommitted
                    }))
                {
                    records = TestContext.SaveChanges();
                    scope.Complete();
                }
            }

            return records;
        }

        public DalKpi GetById(object id)
        {
            return TestContext.Set<DalKpi>().Find(id);
        }

        public IQueryable<DalKpi> GetAll()
        {
            return TestContext.Set<DalKpi>().AsQueryable();
        }

        public void DeleteKpi(object id)
        {
            var test = TestContext.Set<DalKpi>().Find(id);
            TestContext.Set<DalKpi>().Remove(test);
        }

        /// <summary>
        /// Get a repository object by id from the ORM
        /// </summary>
        /// <typeparam name="T">Type of object to query for</typeparam>
        /// <param name="id">Id of the object to query for</param>
        /// <returns>Object of specified type with matching id. Returns null if there is no match
        /// in the repository.</returns>
        public T GetById<T>(object id) where T : class
        {
            return TestContext.Set<T>().Find(id);
        }

        /// <summary>
        /// Marks the object as modified to the ORM so it will be saved when SaveChanges is called. Use for detached entities.
        /// </summary>
        /// <param name="instance">Instance of the objec to save</param>
        public void Save(object instance)
        {
            TestContext.Entry(instance).State = EntityState.Modified;
        }

        /// <summary>
        /// Get all repository objects of the given type from the ORM.
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve</typeparam>
        /// <returns>IQueryable of all objects matching the given type from the ORM.</returns>
        public IQueryable<T> GetAll<T>() where T : class
        {
            return TestContext.Set<T>().AsQueryable<T>();
        }

        /// <summary>
        /// Get all repository objects of the given type from the ORM as a list.
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve</typeparam>
        /// <returns>IList of all objects matching the given type from the ORM.</returns>
        public IList<T> GetAllList<T>() where T : class
        {
            return GetAll<T>().ToList<T>();
        }

        /// <summary>
        /// Deletes the given object from the ORM.
        /// </summary>
        /// <param name="instance">Instance of the object to remove</param>
        public void Delete<T>(T instance) where T : class
        {
            TestContext.Set<T>().Remove(instance as T);
        }

        public string GetDatabaseVersion(string contextKey)
        {
            return HistoryRepository.GetAppliedMigrations().Select(r => r.MigrationId).FirstOrDefault();
        }

        /// <summary>
        /// Add the given object to the database context.
        /// </summary>
        /// <param name="instance"></param>
        public void Add<T>(T instance) where T : class
        {
            if (instance != null)
            {
                TestContext.Set<T>().Add(instance);
            }
        }

        /// <summary>
        /// Add a detached object to the database context.
        /// </summary>
        /// <param name="instance"></param>
        public void AddDetached<T>(T instance) where T : class
        {
            if (instance != null)
            {
                TestContext.Set<T>().Attach(instance);
                TestContext.Entry(instance).State = EntityState.Added;
            }
        }

        /// <summary>
        /// Add a detached object to the database context.
        /// </summary>
        /// <param name="instance"></param>
        public void UpdateDetached<T>(T instance) where T : class
        {
            if (instance != null)
            {
                TestContext.Set<T>().Attach(instance);
                TestContext.Entry(instance).State = EntityState.Modified;
            }
        }


        private bool _disposed;

    }
}
