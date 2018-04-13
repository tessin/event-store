using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventCore;
using EventCore.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlEventStoreTests
{
    [TestClass]
    public class UnitTest2
    {
        static async Task ForEachAsync<T>(IEnumerable<T> source, Func<T, Task> taskFactory, int? maxDegreeOfConcurrency = null)
        {
            var maxDegreeOfConcurrency2 = maxDegreeOfConcurrency ?? 2 * Environment.ProcessorCount;

            var tasks = new List<Task>();

            using (var it = source.GetEnumerator())
            {
                for (; ; )
                {
                    while (tasks.Count < maxDegreeOfConcurrency2 && it.MoveNext())
                    {
                        tasks.Add(taskFactory(it.Current));
                    }

                    if (tasks.Count == 0)
                    {
                        break;
                    }

                    var task = await Task.WhenAny(tasks);
                    await task; // unwrap
                    tasks.Remove(task);
                }
            }
        }

        static IEnumerable<UncommittedEvent> GetTestDataStream(int n)
        {
            var r = new Random();
            var typeId = Guid.NewGuid();
            for (int i = 0; i < n; i++)
            {
                var streamId = Guid.NewGuid();
                var payload = new byte[r.Next(256)];
                r.NextBytes(payload);
                yield return new UncommittedEvent(streamId, 1, typeId, payload, payload.Length, DateTimeOffset.UtcNow);
            }
        }

        [TestMethod, Ignore("benchmark")]
        public async Task TestMethod1()
        {
            var streamId = Guid.NewGuid();
            var created = DateTimeOffset.Now;

            var EventStore = new SqlEventStore(@"Server=tcp:wtxqsopwyb.database.windows.net,1433;Initial Catalog=event-store;Persist Security Info=False;User ID=john;Password=YCWJ^QNMZmmDF36F;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            await EventStore.Database.InitializeAsync();

            // see https://docs.microsoft.com/en-us/azure/sql-database/sql-database-dtu-resource-limits

            // S0
            //await ForEachAsync(GetTestDataStream(10000), e => EventStore.AppendAsync(new[] { e }), 60);

            // S3
            //await ForEachAsync(GetTestDataStream(10000), e => EventStore.AppendAsync(new[] { e }), 200);

            // S6
            //await ForEachAsync(GetTestDataStream(10000), e => EventStore.AppendAsync(new[] { e }), 200);

            // S9
            await ForEachAsync(GetTestDataStream(10000), e => EventStore.AppendAsync(new[] { e }), 400);
        }
    }
}
