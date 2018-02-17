using SqlEventStore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlEventStoreStress
{
    class Program
    {
        static IEnumerable<UncommittedEvent> GetUncommittedSource(int streamCount, int eventCount)
        {
            var stream = Enumerable.Repeat(0, streamCount).Select(_ => Guid.NewGuid()).ToArray();

            var rnd = new Random();
            var sqd = new Dictionary<Guid, int>();
            for (int i = 0; i < eventCount; i++)
            {
                var streamId = stream[rnd.Next(stream.Length)];
                if (sqd.TryGetValue(streamId, out var seqNum))
                {
                    sqd[streamId] = (seqNum = seqNum + 1);
                }
                else
                {
                    sqd.Add(streamId, seqNum = 1);
                }
                yield return new UncommittedEvent(streamId, seqNum, new Guid(), new byte[0], 0, DateTimeOffset.UtcNow);
            }
        }

        static void Main(string[] args)
        {
            var connectionString = LocalDB.Open("es_test");

            var es = new SqlEventStore.SqlEventStore(connectionString);

            es.CreateAsync();

            var sw = new Stopwatch();

            sw.Start();

            es.AppendAsync(GetUncommittedSource(1000, 1000)).GetAwaiter().GetResult();
            es.AppendAsync(GetUncommittedSource(1000, 10000)).GetAwaiter().GetResult();
            es.AppendAsync(GetUncommittedSource(1000, 100000)).GetAwaiter().GetResult();

            Console.WriteLine(sw.Elapsed);
        }
    }
}
