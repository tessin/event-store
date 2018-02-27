using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EventCore.SqlServer
{
    static class SqlScript
    {
        public static IEnumerable<string> MultiBatch(TextReader reader)
        {
            // GO is not a Transact - SQL statement; it is a command recognized by the sqlcmd and osql utilities and SQL Server Management Studio Code editor.
            // SQL Server utilities interpret GO as a signal that they should send the current batch of Transact-SQL statements to an instance of SQL Server.The current batch of statements is composed of all statements entered since the last GO, or since the start of the ad hoc session or script if this is the first GO.
            // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/sql-server-utilities-statements-go

            // note that we do not support the `GO [count]` format, just "GO"

            var sb = new StringBuilder();
            for (; ; )
            {
                var s = reader.ReadLine();
                if (s == null)
                {
                    break;
                }
                if (s.Equals("go", StringComparison.OrdinalIgnoreCase))
                {
                    var batch = sb.ToString();
                    sb.Length = 0;
                    yield return batch;
                }
                else
                {
                    sb.AppendLine(s);
                }
            }
            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }
    }
}
