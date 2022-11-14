using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;
using EPiServer.Marketing.Testing.Dal.Exceptions;
using EPiServer.Marketing.Testing.Dal.Mappings;
using EPiServer.Marketing.Testing.Dal.Migrations;
using EPiServer.ServiceLocation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.Testing.Dal.DataAccess
{
    //remove below and 
    //initialize full stack servic
    [ServiceConfiguration(ServiceType = typeof(ITestingDataAccess), Lifecycle = ServiceInstanceScope.Transient)]
    public class TestingDataAccess : ITestingDataAccess
    {
        public readonly Injected<IRepository> _repository;
        internal bool _UseEntityFramework;
        public bool IsDatabaseConfigured;

        public TestingDataAccess()
        {
            _UseEntityFramework = true;

            
        }

        public TestingDataAccess(IRepository repository)
        {
            _repository.Service = repository;
        }

        public void Archive(Guid testObjectId, Guid winningVariantId)
        {
            ArchiveHelper(_repository.Service, testObjectId, winningVariantId);

            SetTestState(testObjectId, DalTestState.Archived);
        }

        public void Delete(Guid testObjectId)
        {
            DeleteHelper(_repository.Service, testObjectId);
        }

        public IABTest Get(Guid testObjectId)
        {
            return _repository.Service.GetById(testObjectId);
        }

        // TODO : rename to GetTestsByItemId
        public List<IABTest> GetTestByItemId(Guid originalItemId)
        {
            return _repository.Service.GetAll().Where(t => t.OriginalItemId == originalItemId).ToList();
        }

        public List<IABTest> GetTestList(DalTestCriteria criteria)
        {
            return GetTestListHelper(_repository.Service, criteria);
        }

        public void IncrementCount(Guid testId, int itemVersion, DalCountType resultType, Guid kpiId)
        {
            IncrementCountHelper(_repository.Service, testId, itemVersion, resultType, kpiId);
        }

        public void AddKpiResultData(Guid testId, int itemVersion, IDalKeyResult keyResult, int keyType)
        {
            AddKpiResultDataHelper(_repository.Service, testId, itemVersion, keyResult, keyType);
        }

        public Guid Save(DalABTest testObject)
        {
            return SaveHelper(_repository.Service, testObject);
        }

        public IABTest Start(Guid testObjectId )
        {
            if (IsTestActive(testObjectId))
            {
                throw new Exception("The test page already has an Active test");
            }

            return SetTestState(testObjectId, DalTestState.Active);
        }

        public void Stop(Guid testObjectId)
        {
            SetTestState(testObjectId, DalTestState.Done);
        }

        public long GetDatabaseVersion(string schema, string contextKey)
        {
            if (HasTableNamed((BaseRepository)_repository.Service, DatabaseVersion.TableToCheckFor))
            {
                // the sql scripts need to be run!
                IsDatabaseConfigured = true;
            }

            return IsDatabaseConfigured ? GetDatabaseVersionHelper(_repository.Service, contextKey) : 0;
        }

        #region Private Helpers

        private long GetDatabaseVersionHelper(IRepository repo, string contextKey)
        {
            var lastMigration = repo.GetDatabaseVersion(contextKey);

            // we are only interested in the numerical part of the key (i.e. 201609091719244_Initial)
            var version = lastMigration.Split('_')[0];

            return Convert.ToInt64(version);
        }

        private void DeleteHelper(IRepository repo, Guid testId)
        {
            repo.DeleteTest(testId);
            repo.SaveChanges();
        }

        private List<IABTest> GetTestListHelper(IRepository repo, DalTestCriteria criteria)
        {
            // if no filters are passed in, just return all tests
            var filters = criteria.GetFilters();
            if (!filters.Any())
            {
                return repo.GetAll().ToList();
            }

            var variantOperator = DalFilterOperator.And;
            IQueryable<IABTest> variantResults = null;
            var variantId = Guid.Empty;
            var pe = Expression.Parameter(typeof(DalABTest), "test");
            Expression wholeExpression = null;

            // build up expression tree based on the filters that are passed in
            foreach (var filter in filters)
            {
                // if we are filtering on a single property(not an element in a list) create the expression
                if (filter.Property != DalABTestProperty.VariantId)
                {
                    var left = Expression.Property(pe, typeof(DalABTest).GetProperty(filter.Property.ToString()));
                    var right = Expression.Constant(filter.Value);
                    var expression = Expression.Equal(left, right);

                    // first time through, so we just set the expression to the first filter criteria and continue to the next one
                    if (wholeExpression == null)
                    {
                        wholeExpression = expression;
                        continue;
                    }

                    // each subsequent iteration we check to see if the filter is for an AND or OR and append accordingly
                    wholeExpression = filter.Operator == DalFilterOperator.And
                        ? Expression.And(wholeExpression, expression)
                        : Expression.Or(wholeExpression, expression);
                }
                else
                // if we are filtering on an item in a list, then generate simple results that we can lump in at the end
                {
                    variantId = new Guid(filter.Value.ToString());
                    variantOperator = filter.Operator;
                    variantResults = repo.GetAll().Where(x => x.Variants.Any(v => v.ItemId == variantId));
                }
            }

            IQueryable<IABTest> results = null;
            IQueryable<IABTest> tests;

            try
            {
                tests = repo.GetAll().AsQueryable();
            }
            catch (Exception)
            {
                throw new DatabaseDoesNotExistException();
            }
            
            // if we have created an expression tree, then execute it against the tests to get the results
            if (wholeExpression != null)
            {
                var whereCallExpression = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new Type[] { tests.ElementType },
                    tests.Expression,
                    Expression.Lambda<Func<DalABTest, bool>>(wholeExpression, new ParameterExpression[] { pe })
                    );

                results = tests.Provider.CreateQuery<DalABTest>(whereCallExpression);
            }

            // if we are also filtering against a variantId, include those results
            if (variantResults != null)
            {
                if (results == null)
                {
                    return variantResults.ToList();
                }

                results = variantOperator == DalFilterOperator.And
                    ? results.Where(test => test.Variants.Any(v => v.ItemId == variantId))
                    : results.Concat(variantResults).Distinct();
            }

            return results.ToList<IABTest>();
        }
        
        private void IncrementCountHelper(IRepository repo, Guid testId, int itemVersion, DalCountType resultType, Guid kpiId)
        {
            var test = repo.GetById(testId);
            var variant = test.Variants.First(v => v.ItemVersion == itemVersion);

            if (resultType == DalCountType.View)
            {
                variant.Views++;
            }
            else
            {
                // multiple kpi's - increase count for the specific kpi that converted and increase conversion count for test by the kpi's weight
                if (variant.DalKeyConversionResults.Count > 0)
                {
                    var result = variant.DalKeyConversionResults.First(r => r.KpiId == kpiId);
                    result.Conversions++;
                    variant.Conversions += result.Weight;
                }
                else  // single kpi
                {
                    variant.Conversions++;
                }
            }

            variant.ModifiedDate = DateTime.UtcNow;

            repo.SaveChanges();
        }

        private void AddKpiResultDataHelper(IRepository repo, Guid testId, int itemVersion, IDalKeyResult keyResult, int type)
        {
            var test = repo.GetById(testId);
            var variant = test.Variants.First(v => v.ItemVersion == itemVersion);

            if (type == (int)DalKeyResultType.Financial)
            {
                variant.DalKeyFinancialResults.Add((DalKeyFinancialResult)keyResult);
            }
            else if (type == (int)DalKeyResultType.Value)
            {
                variant.DalKeyValueResults.Add((DalKeyValueResult)keyResult);
            }
            else if (type == (int)DalKeyResultType.Conversion)
            {
                variant.DalKeyConversionResults.Add((DalKeyConversionResult)keyResult);
            }

            variant.ModifiedDate = DateTime.UtcNow;

            repo.SaveChanges();
        }

        private void ArchiveHelper(IRepository repo, Guid testid, Guid variantId)
        {
            var test = repo.GetById(testid);
            var variant = test.Variants.First(v => v.Id == variantId);

            variant.IsWinner = true;

            if (DateTime.UtcNow < test.EndDate)
            {
                test.EndDate = DateTime.UtcNow;
            }

            repo.SaveChanges();
        }

        private Guid SaveHelper(IRepository repo, DalABTest testObject)
        {
            var id = testObject.Id;
            FullStack_Repository _fsRepo1 = new FullStack_Repository();
            
            var test = repo.GetById(testObject.Id) as DalABTest;
            if (test == null)
            {
                var keyList = _fsRepo1.AddExperiment(testObject);
                var flagKey = FullStackConstants.GetFlagKey(testObject.Title);
                var experimentKey = FullStackConstants.GetExperimentKey(testObject.Title);
                testObject.FS_FlagKey = flagKey;
                testObject.FS_ExperimentKey = experimentKey;
                repo.Add(testObject);

                
            }
            else
            {
                switch (test.State)
                {
                    case DalTestState.Inactive:
                        test.Title = testObject.Title;
                        test.Description = testObject.Description;
                        test.OriginalItemId = testObject.OriginalItemId;
                        test.LastModifiedBy = testObject.LastModifiedBy;
                        test.StartDate = testObject.StartDate.ToUniversalTime();
                        test.EndDate = testObject.EndDate.ToUniversalTime();

                        test.ModifiedDate = DateTime.UtcNow;
                        test.ParticipationPercentage = testObject.ParticipationPercentage;

                        // remove any existing kpis that are not part of the new test
                        foreach (var existingKpi in test.KeyPerformanceIndicators.ToList())
                        {
                            if (testObject.KeyPerformanceIndicators.All(k => k.Id != existingKpi.Id))
                            {
                                repo.Delete(existingKpi);
                            }
                        }

                        // update existing kpis that are still around and add any that are new
                        foreach (var newKpi in testObject.KeyPerformanceIndicators)
                        {
                            var existingKpi = test.KeyPerformanceIndicators.SingleOrDefault(k => k.Id == newKpi.Id);

                            if (existingKpi != null)
                            {
                                existingKpi.KeyPerformanceIndicatorId = newKpi.KeyPerformanceIndicatorId;
                                existingKpi.ModifiedDate = DateTime.UtcNow;
                            }
                            else
                            {
                                test.KeyPerformanceIndicators.Add(newKpi);
                            }
                        }

                        UpdateVariants(repo, test, testObject);
                        break;
                    case DalTestState.Done:
                        test.State = testObject.State == DalTestState.Archived ? DalTestState.Archived : DalTestState.Done;
                        test.IsSignificant = testObject.IsSignificant;
                        test.ZScore = testObject.ZScore;
                        test.ModifiedDate = DateTime.UtcNow;
                        UpdateVariants(repo, test, testObject);
                        break;
                    case DalTestState.Active:
                        test.ContentLanguage = test.ContentLanguage == string.Empty ? testObject.ContentLanguage : test.ContentLanguage;
                        test.State = testObject.State;
                        test.ModifiedDate = DateTime.UtcNow;
                        break;
                }
            }
            repo.SaveChanges();

            return id;
        }

        private void UpdateVariants(IRepository repo, IABTest test, IABTest testObject)
        {
            // remove any existing variants that are not part of the new test
            foreach (var existingVariant in test.Variants.ToList())
            {
                if (testObject.Variants.All(k => k.Id != existingVariant.Id))
                {
                    repo.Delete(existingVariant);
                }
            }

            // update existing variants that are still around and add any that are new
            foreach (var newVariant in testObject.Variants)
            {
                var existingVariant = test.Variants.SingleOrDefault(k => k.Id == newVariant.Id);

                if (existingVariant != null)
                {
                    existingVariant.ItemId = newVariant.ItemId;
                    existingVariant.ItemVersion = newVariant.ItemVersion;
                    existingVariant.ModifiedDate = DateTime.UtcNow;
                    existingVariant.Views = newVariant.Views;
                    existingVariant.Conversions = newVariant.Conversions;
                    existingVariant.IsWinner = newVariant.IsWinner;
                    existingVariant.IsPublished = newVariant.IsPublished;
                }
                else
                {
                    test.Variants.Add(newVariant);
                }
            }
        }

        private bool IsTestActive(Guid testId)
        {
            return IsTestActiveHelper(_repository.Service, testId);
        }

        private bool IsTestActiveHelper(IRepository repo, Guid testId)
        {
            var test = repo.GetById(testId);
            var tests = repo.GetAll()
                        .Where(t => t.OriginalItemId == test.OriginalItemId && t.State == DalTestState.Active);

            return tests.Any();
        }

        private IABTest SetTestState(Guid theTestId, DalTestState theState)
        {
            return SetTestStateHelper(_repository.Service, theTestId, theState);
        }

        private IABTest SetTestStateHelper(IRepository repo, Guid theTestId, DalTestState theState)
        {
            var aTest = repo.GetById(theTestId);
            aTest.State = theState;
            repo.SaveChanges();
            return aTest;
        }

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
        #endregion
    }
}
