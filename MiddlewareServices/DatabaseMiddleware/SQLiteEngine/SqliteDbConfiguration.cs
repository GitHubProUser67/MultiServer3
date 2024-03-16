using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity;
using System.Data.SQLite.EF6;
using System.Data.SQLite;
using System.Data;
using System.Configuration;

namespace DatabaseMiddleware.SQLiteEngine
{
    public class SqliteDbConfiguration : DbConfiguration
    {
        public SqliteDbConfiguration()
        {
            string? assemblyName = typeof(SQLiteProviderFactory).Assembly.GetName().Name;

            if (!string.IsNullOrEmpty(assemblyName))
            {
                RegisterDbProviderFactories(assemblyName);
                SetProviderFactory(assemblyName, SQLiteFactory.Instance);
                SetProviderFactory(assemblyName, SQLiteProviderFactory.Instance);
                SetProviderServices(assemblyName,
                    (DbProviderServices)SQLiteProviderFactory.Instance.GetService(
                        typeof(DbProviderServices)));
            }
        }

        static void RegisterDbProviderFactories(string assemblyName)
        {
            var dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet != null)
            {
                var dbProviderFactoriesDataTable = dataSet.Tables.OfType<DataTable>()
                    .First(x => x.TableName == typeof(DbProviderFactories).Name);

                var dataRow = dbProviderFactoriesDataTable.Rows.OfType<DataRow>()
                    .FirstOrDefault(x => x.ItemArray[2]?.ToString() == assemblyName);

                if (dataRow != null)
                    dbProviderFactoriesDataTable.Rows.Remove(dataRow);

                dbProviderFactoriesDataTable.Rows.Add(
                    "SQLite Data Provider (Entity Framework 6)",
                    ".NET Framework Data Provider for SQLite (Entity Framework 6)",
                    assemblyName,
                    typeof(SQLiteProviderFactory).AssemblyQualifiedName
                    );
            }
        }
    }
}
