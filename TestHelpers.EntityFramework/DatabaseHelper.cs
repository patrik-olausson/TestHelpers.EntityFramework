using System;
using System.Data.Common;
using System.Data.Entity;

namespace TestHelpers.EntityFramework
{
    public static class DatabaseHelper
    {
        public static string CreateLocalDbConnectionString(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseName));

            return @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=" + databaseName + ";Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        }

        public static TDbContext CreateDbContext<TDbContext>(string connectionString) where TDbContext : DbContext
        {
            var ctor = typeof(TDbContext).GetConstructor(new[] { typeof(string) });
            if (ctor == null)
                throw new Exception("The DbContext must expose a constructor that takes a connection string as an argument.");

            return (TDbContext)ctor.Invoke(new object[] { connectionString });
        }

        public static TDbContext CreateDbContext<TDbContext>(DbConnection dbConnection) where TDbContext : DbContext
        {
            var ctor = typeof(TDbContext).GetConstructor(new[] { typeof(DbConnection) });
            if (ctor == null)
                throw new Exception("The DbContext must expose a constructor that takes a DbConnection as an argument.");

            return (TDbContext)ctor.Invoke(new object[] { dbConnection });
        }

        public static void ClearAllTables(this DbContext dbContext)
        {
            dbContext.Database.ExecuteSqlCommand("EXEC sp_MSForEachTable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"");
            dbContext.Database.ExecuteSqlCommand("EXEC sp_MSForEachTable \"DELETE FROM ?\"");
            dbContext.Database.ExecuteSqlCommand("EXEC sp_MSForEachTable \"ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all\"");
        }
    }
}