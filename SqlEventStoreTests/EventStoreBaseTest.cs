using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlEventStoreTests
{
    [TestClass]
    public abstract class EventStoreBaseTest
    {
        public SqlEventStore.SqlEventStore EventStore { get; private set; }

        [TestInitialize]
        public void Initialize()
        {
            var connStr = new SqlConnectionStringBuilder();

            connStr.DataSource = @"(LocalDB)\MSSQLLocalDB";
            connStr.IntegratedSecurity = true;

            var db = $"es_test_{Environment.TickCount}";

            using (var conn = new SqlConnection(connStr.ToString()))
            {
                conn.Open();
                var cmd = new SqlCommand($"create database [{db}]", conn);
                cmd.ExecuteNonQuery();
                conn.ChangeDatabase(db);
            }

            connStr.InitialCatalog = db;

            EventStore = new SqlEventStore.SqlEventStore(connStr.ToString());
            EventStore.CreateAsync().GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var e in EventStore.GetEnumerable())
            {
                Debug.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
            }

            var connStr = new SqlConnectionStringBuilder();

            connStr.DataSource = @"(LocalDB)\MSSQLLocalDB";
            connStr.IntegratedSecurity = true;

            using (var conn = new SqlConnection(connStr.ToString()))
            {
                conn.Open();

                var cmd1 = new SqlCommand($"alter database [{EventStore.Database}] set offline with rollback immediate", conn);
                cmd1.ExecuteNonQuery();

                var cmd2 = new SqlCommand($"drop database [{EventStore.Database}]", conn);
                cmd2.ExecuteNonQuery();
            }

            EventStore = null;
        }
    }
}
