using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace TestHelpers.EntityFramework
{
    public class DbAssertHelper<TDbContext> : IDisposable
        where TDbContext : DbContext
    {
        public TDbContext DbContext { get; }

        public DbAssertHelper(TDbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            DbContext = dbContext;
        }

        /// <summary>
        /// Tries to count the rows of a specified table.
        /// </summary>
        public int GetCountFrom(string tableName)
        {
            return DbContext.Database.SqlQuery<int>($"select count(*) from {tableName}").ToList().First();
        }

        /// <summary>
        /// Tries to count the rows of a a table named just like the entity but is pluralized (with an s).
        /// </summary>
        public int GetCount<TEntity>()
        {
            return GetCountFrom(CreatePluralizedTableName<TEntity>());
        }

        /// <summary>
        /// Tries to get entity data from a table named just like the entity but pluralized (with an s).
        /// If the automatic pluralization doesn't work you can specify the table name explicitly.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity you want to get from the database. Example: OrderLine</typeparam>
        /// <param name="tableName">Optional parameter that explicitly specifies a table name. If no value is
        /// provided the pluralized name of the type is used. Example: type OrderLine will use table named OrderLines</param>
        /// <returns></returns>
        public List<TEntity> GetAll<TEntity>(string tableName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                tableName = CreatePluralizedTableName<TEntity>();

            return DbContext.Database.SqlQuery<TEntity>($"select * from {tableName}").ToList();
        }

        private string CreatePluralizedTableName<TEntity>()
        {
            return $"{typeof(TEntity).Name}s";
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
                DbContext.Dispose();
            }
        }
    }
}