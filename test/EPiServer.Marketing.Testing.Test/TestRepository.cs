﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Test.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPiServer.Marketing.Testing.Test.Core
{
    internal class TestRepository : IRepository
    {
        public TestContext TestContext { get; set; }

        public TestRepository(TestContext testContext)
        {
            TestContext = testContext;
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

        public int SaveChanges()
        {
            return SaveChanges(0);
        }

        public int SaveChanges(int retryCount)
        {
            var records = 0;

            if (TestContext != null)
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions()
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                    }))
                {
                    records = TestContext.SaveChanges();
                    scope.Complete();
                }
            }

            return records;
        }

        public void Save(object instance)
        {
            TestContext.Entry(instance).State = EntityState.Modified;
        }

        public void Add<T>(T instance) where T : class
        {
            if (instance != null)
            {
                TestContext.Set<T>().Add(instance);
            }
        }

        public void Delete<T>(T instance) where T : class
        {
            TestContext.Set<T>().Remove(instance as T);
        }

        public void DeleteTest(object id)
        {
            var test = TestContext.Set<DalABTest>().Find(id);
            TestContext.Set<DalABTest>().Remove(test);
        }

        public T GetById<T>(object id) where T : class
        {
            throw new NotImplementedException();
        }

        public IABTest GetById(object id)
        {
            return TestContext.Set<DalABTest>().Find(id);
        }

        public IQueryable<T> GetAll<T>() where T : class
        {
            return TestContext.Set<T>().AsQueryable();
        }

        public IQueryable<IABTest> GetAll()
        {
            return TestContext.Set<DalABTest>().AsQueryable();
        }

        public IQueryable<IABTest> GetTestList(DalTestCriteria criteria)
        {
            var filters = criteria.GetFilters();

            var andFilters = filters.Where(filter => filter.Operator == DalFilterOperator.And);
            var orFilters = filters.Where(filter => filter.Operator == DalFilterOperator.Or);


            var tests = TestContext.ABTests.AsQueryable();
            var pe = Expression.Parameter(typeof(string), "test");
            Expression wholeExpression = null;

            foreach (var filter in filters)
            {
                Expression left = Expression.Property(pe, typeof(string).GetProperty(filter.Property.ToString()));
                Expression right = Expression.Constant(filter.Value);
                Expression e = Expression.Equal(left, right);

                // first time through, so we just set the expression to the first filter criteria and continue to the next one
                if (wholeExpression == null)
                {
                    wholeExpression = e;
                    continue;
                }

                // each subsequent iteration we check to see if the filter is for an AND or OR and append accordingly
                wholeExpression = filter.Operator == DalFilterOperator.And
                    ? Expression.And(wholeExpression, e) : Expression.Or(wholeExpression, e);
            }

            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { tests.ElementType },
                tests.Expression,
                Expression.Lambda<Func<DalABTest, bool>>(wholeExpression, new ParameterExpression[] { pe })
                );

            IQueryable<DalABTest> results = tests.Provider.CreateQuery<DalABTest>(whereCallExpression);
            return results;
        }

        public IList<T> GetAllList<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseVersion(string contextKey)
        {
            throw new NotImplementedException();
        }

        public void AddDetached<T>(T instance) where T : class
        {
            throw new NotImplementedException();
        }

        public void UpdateDetached<T>(T instance) where T : class
        {
            throw new NotImplementedException();
        }

        private bool _disposed;

    }
}
