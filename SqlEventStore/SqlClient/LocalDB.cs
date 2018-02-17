using System;
using System.Data.SqlClient;

namespace SqlEventStore
{
    public class LocalDB : IDisposable
    {
        private const string MSSQLLocalDB = @"(LocalDB)\MSSQLLocalDB";

        public static string Create(string database, string dataSource = MSSQLLocalDB)
        {
            var cb = new SqlConnectionStringBuilder();

            cb.DataSource = dataSource;
            cb.IntegratedSecurity = true;

            using (var conn = new SqlConnection(cb.ToString()))
            {
                conn.Open();

                var cmd = new SqlCommand($"create database [{database}]", conn);
                cmd.ExecuteNonQuery();
            }

            cb.InitialCatalog = database;

            return cb.ToString();
        }

        public static string Open(string database, string dataSource = MSSQLLocalDB)
        {
            var cb = new SqlConnectionStringBuilder();

            cb.DataSource = dataSource;
            cb.IntegratedSecurity = true;
            cb.InitialCatalog = database;

            return cb.ToString();
        }

        public string DataSource { get; }
        public string Database { get; }
        public string ConnectionString { get; }

        public LocalDB(string database = null, string dataSource = @"(LocalDB)\MSSQLLocalDB")
        {
            DataSource = dataSource;
            Database = database ?? $"es_tmp_{Environment.TickCount}";
            ConnectionString = Create(database: Database);
        }

        public void Dispose()
        {
            var cb = new SqlConnectionStringBuilder();

            cb.DataSource = DataSource;
            cb.IntegratedSecurity = true;

            using (var conn = new SqlConnection(cb.ToString()))
            {
                conn.Open();

                {
                    // take database offline
                    var cmd = new SqlCommand($"alter database [{Database}] set offline with rollback immediate", conn);
                    cmd.ExecuteNonQuery();
                }

                {
                    // drop database (todo: delete data?)
                    var cmd = new SqlCommand($"drop database [{Database}]", conn);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
