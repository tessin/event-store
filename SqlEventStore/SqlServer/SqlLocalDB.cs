using System;
using System.Data.SqlClient;
using System.IO;

namespace EventSourcing.SqlServer
{
    public class SqlLocalDB : IDisposable
    {
        private const string MSSQLLocalDB = @"(LocalDB)\MSSQLLocalDB";

        public static string Create(string database, string dataSource = MSSQLLocalDB, string path = "D:\\Temp")
        {
            if (!(database.IndexOfAny(new[] { '[', ']', '\'', '\"' }) == -1))
            {
                throw new ArgumentOutOfRangeException(nameof(database));
            }

            var cb = new SqlConnectionStringBuilder();

            cb.DataSource = dataSource;
            cb.IntegratedSecurity = true;

            using (var conn = new SqlConnection(cb.ToString()))
            {
                conn.Open();

                var cmd = new SqlCommand($"create database [{database}] on (name='{database}', filename='{Path.Combine(path, database)}.mdf')", conn);
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
        public string FileName { get; }

        public SqlLocalDB(string database = null, string dataSource = @"(LocalDB)\MSSQLLocalDB")
        {
            DataSource = dataSource;
            Database = database ?? $"es_tmp_{Environment.TickCount}";
            ConnectionString = Create(database: Database, path: "D:\\Temp");
            FileName = $"D:\\Temp\\{Database}.mdf";
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

            var dbFileName = FileName;
            var logFileName = Path.GetFileNameWithoutExtension(dbFileName) + "_log.ldf";

            File.Delete(dbFileName);
            File.Delete(logFileName);
        }
    }
}
