using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlEventStore
{
    static class Sql
    {
        public static Task ExecuteNonQueryAsync(this SqlConnection conn, string commandText)
        {
            var cmd = new SqlCommand(commandText, conn);
            return cmd.ExecuteNonQueryAsync();
        }

        public static async Task ExecuteScriptAsync(this SqlConnection conn, string resourceName)
        {
            var sql = new StreamReader(typeof(Sql).Assembly.GetManifestResourceStream($"SqlEventStore.{resourceName}")).ReadToEnd();
            var split = Regex.Split(sql, @"^go", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (var stmt in split)
            {
                await conn.ExecuteNonQueryAsync(stmt);
            }
        }
    }
}
