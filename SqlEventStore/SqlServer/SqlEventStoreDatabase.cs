﻿using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace EventCore.SqlServer
{
    public class SqlEventStoreDatabase
    {
        private string _connectionString;

        public string DataSource => new SqlConnectionStringBuilder(_connectionString).DataSource;
        public string Name => new SqlConnectionStringBuilder(_connectionString).InitialCatalog;

        public SqlEventStoreDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<SqlConnection> OpenAsync()
        {
            // todo: SQL Azure connection retry policy

            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }

        public SqlConnection Open()
        {
            // todo: SQL Azure connection retry policy

            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        public async Task InitializeAsync()
        {
            var type = GetType();

            using (var script = new StreamReader(type.Assembly.GetManifestResourceStream($"{type.Namespace}.Internal.es-schema.sql")))
            {
                using (var conn = await OpenAsync())
                {
                    foreach (var batch in SqlScript.MultiBatch(script))
                    {
                        using (var cmd = new SqlCommand(batch, conn))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
    }
}
