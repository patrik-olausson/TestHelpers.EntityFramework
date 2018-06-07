using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace TestHelpers.EntityFramework
{
    public class EntityFrameworkTestHelper<TDbContext> : IDisposable
        where TDbContext : DbContext
    {
        private readonly bool _deleteDatabaseOnDispose;
        protected readonly TDbContext ArrangeContext;

        public EntityFrameworkTestHelper(
            string databaseName,
            bool deleteDatabaseOnDispose,
            Action<TDbContext> initializerAction = null)
        {
            var connectionString = DatabaseHelper.CreateLocalDbConnectionString(databaseName);
            ArrangeContext = DatabaseHelper.CreateDbContext<TDbContext>(connectionString);
            _deleteDatabaseOnDispose = deleteDatabaseOnDispose;
            initializerAction?.Invoke(ArrangeContext);
        }

        public virtual void ArrangeDatabase(Action<TDbContext> arrangeAction)
        {
            arrangeAction(ArrangeContext);
        }

        public virtual void AddEntities<TEntity>(params TEntity[] entities) where TEntity : class
        {
            foreach (var entity in entities)
            {
                AddEntity(entity);
            }
        }

        public virtual void AddEntity<TEntity>(TEntity entity) where TEntity : class
        {
            ArrangeContext.Set<TEntity>().Add(entity);
        }

        public virtual void AssertDatabase(Action<DbAssertHelper<TDbContext>> assertAction)
        {
            using (var assertHelper = new DbAssertHelper<TDbContext>(CreateNewContext(null)))
            {
                assertAction?.Invoke(assertHelper);
            }
        }

        public virtual TDbContext CreateNewContext(Action<string> testOutput)
        {
            var connectionString = ArrangeContext.Database.Connection.ConnectionString;
            var dbContext = DatabaseHelper.CreateDbContext<TDbContext>(connectionString);
            dbContext.Database.Log = testOutput;

            return dbContext;
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            try
            {
                return await ArrangeContext.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                throw CreateExceptionWithUnwrappedValidationMessages(ex);
            }
        }

        public virtual int SaveChanges()
        {
            try
            {
                return ArrangeContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                throw CreateExceptionWithUnwrappedValidationMessages(ex);
            }
        }

        protected Exception CreateExceptionWithUnwrappedValidationMessages(DbEntityValidationException ex)
        {
            var stringifiedValidationMessages = ex.EntityValidationErrors.SelectMany(
                x => x.ValidationErrors.Select(y => $"Property: '{y.PropertyName}' has error: '{y.ErrorMessage}'"));

            var message = string.Join(", ", stringifiedValidationMessages);

            return new Exception(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_deleteDatabaseOnDispose)
                {
                    ArrangeContext.Database.Delete();
                }

                ArrangeContext.Dispose();
            }
        }
    }
}
