using EventCore;
using EventCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlEventStoreCli
{
    class Program
    {
        static void Main(string[] args)
        {
            for (; ; )
            {
                var ln = Console.ReadLine();
                if (ln == null)
                {
                    break;
                }
                var split = ln.Split(new[] { '\t', ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    switch (split[0])
                    {
                        case "create":
                            SqlLocalDB.Create(database: split[1]);
                            break;
                        case "exit":
                            return;
                        default:
                            Console.Error.WriteLine($"unknown command: {split[0]}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[{ex.GetType()}]: {ex.Message}");
                }
            }
        }
    }
}
