using EventCore;
using EventCore.SqlServer;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            var payload = Encoding.UTF8.GetBytes("{\n  \"projectId\": \"B2B528E2-E2EA-4ECB-9891-94C4ECA6462F\",\n  \"userId\": \"3BDECF3F-384D-4B0D-A3A9-13E6D6384F43\",\n  \"amount\": 50000,\n  \"currency\": \"SEK\"\n}");

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
                yield return new UncommittedEvent(streamId, seqNum, new Guid(), payload, payload.Length, DateTimeOffset.UtcNow);
            }
        }

        static void Main(string[] args)
        {
            var t = TimeSpan.FromSeconds(10);

            //FillAsync().GetAwaiter().GetResult();
            GetEnumerableStreamBenchmarkAsync(t).GetAwaiter().GetResult();
            //AppendBenchmark2Async(x => x, 5, t).GetAwaiter().GetResult();
            //AppendBenchmark2Async(x => x.WithCommandTextAppend(), 5, t).GetAwaiter().GetResult();
            return;

            // todo: fill test, bulk copy is faster

            //

            Console.WriteLine("Append (default)");

            foreach (var batchSize in new[] { 5, 25, 250 })
            {
                var sw = new Stopwatch();
                sw.Start();
                var n = AppendBenchmarkAsync(cs => new SqlEventStore(cs), batchSize, sw, t).GetAwaiter().GetResult();
                Console.WriteLine($"{batchSize,3}: {n,7:0.0} op/s");
            }

            Console.WriteLine("Append (bulk copy)");

            foreach (var batchSize in new[] { 5, 25, 250 })
            {
                var sw = new Stopwatch();
                sw.Start();
                var n = AppendBenchmarkAsync(cs => new SqlEventStore(cs).WithBulkCopyAppend(), batchSize, sw, t).GetAwaiter().GetResult();
                Console.WriteLine($"{batchSize,3}: {n,7:0.0} op/s");
            }
        }

        private static async Task<double> AppendBenchmarkAsync(Func<string, ISqlEventStore> factory, int batchSize, Stopwatch sw, TimeSpan timeout)
        {
            using (var db = new SqlLocalDB())
            {
                var es = factory(db.ConnectionString);

                await es.Database.InitializeAsync();

                var list = new List<UncommittedEvent>();

                int n = 0;
                foreach (var e in GetUncommittedSource(1000, 100000))
                {
                    list.Add(e);

                    if (!(list.Count < batchSize))
                    {
                        var batch = list.ToArray();
                        n += batch.Length;
                        list.Clear();
                        await es.AppendAsync(batch);

                        if (!(sw.Elapsed < timeout))
                        {
                            return n / sw.Elapsed.TotalSeconds;
                        }
                    }
                }

                return n / sw.Elapsed.TotalSeconds;
            }
        }

        private static async Task AppendBenchmark2Async(Func<ISqlEventStore, ISqlEventStore> configuration, int batchSize, TimeSpan timeout)
        {
            using (var db = new SqlLocalDB())
            {
                var es = new SqlEventStore(db.ConnectionString);

                await es.Database.InitializeAsync();

                var list = new List<UncommittedEvent>();

                var sw = new Stopwatch();

                var es2 = configuration(es);

                int n = 0;
                foreach (var e in GetUncommittedSource(1000, 100000))
                {
                    list.Add(e);

                    if (!(list.Count < batchSize))
                    {
                        var batch = list.ToArray();
                        n += batch.Length;
                        list.Clear();

                        sw.Start();

                        await es2.AppendAsync(batch);

                        sw.Stop();

                        if (!(sw.Elapsed < timeout))
                        {
                            break;
                        }
                    }
                }

                Console.WriteLine($"{n / sw.Elapsed.TotalSeconds:N1} op/s");
            }
        }

        private static async Task GetEnumerableStreamBenchmarkAsync(TimeSpan timeout)
        {
            using (var db = new SqlLocalDB())
            {
                var es = new SqlEventStore(db.ConnectionString);

                await es.Database.InitializeAsync();

                await es.WithBulkCopyAppend().AppendAsync(GetUncommittedSource(1000, 100000));

                var streamIdSet = es.GetEnumerable().Select(x => x.StreamId).Distinct().ToList();

                // 

                var fs = new[] {
                        "default",
                        "unknown",
                    };

                var xs = new[] {
                        new List<double>(),
                        new List<double>(),
                    };

                for (int k = 0; k < 10; k++)
                {
                    var i = 0;
                    foreach (var f in new Func<ISqlEventStore, ISqlEventStore>[] {
                        x => x,
                        })
                    {
                        var y = f(es);

                        var sw = new Stopwatch();

                        var rnd = Randomness.Create();

                        int n = 0;
                        while (sw.Elapsed < timeout)
                        {
                            var streamId = streamIdSet[rnd.Next(streamIdSet.Count)];

                            var minSeq = 1;
                            var maxSeq = minSeq + rnd.Next(100);

                            sw.Start();

                            n += y.GetEnumerableStream(streamId, minSeq, maxSeq).Count();

                            sw.Stop();
                        }

                        var p = n / sw.Elapsed.TotalSeconds;
                        xs[i].Add(p);
                        i++;
                    }

                    Console.Write($"{k}");

                    for (int j = 0; j < 2; j++)
                    {
                        var p = xs[j][k];

                        var fit = Tuple.Create(0d, 0d);

                        if (xs[j].Count > 1)
                        {
                            fit = Fit.Line(Enumerable.Range(1, xs[j].Count).Select(x => (double)x).ToArray(), xs[j].ToArray());
                        }

                        Console.Write($" | {fs[j]}: {p,9:N1} op/s {fit.Item1,9:N1}+x*{fit.Item2,9:N1}");
                    }
                    Console.WriteLine();
                }
            }
        }

        private static async Task FillAsync()
        {
            using (var db = new SqlLocalDB())
            {
                var es = new SqlEventStore(db.ConnectionString);

                await es.Database.InitializeAsync();

                var marks = new List<double>();

                for (int i = 0; i < 100; i++)
                {
                    var sw = Stopwatch.StartNew();

                    await es.WithBulkCopyAppend().AppendAsync(GetUncommittedSource(1000, 100000));

                    var mark = 100000 / sw.Elapsed.TotalSeconds;
                    var size = new FileInfo(db.FileName).Length / (1024d * 1024d);

                    marks.Add(mark);

                    if (marks.Count > 1)
                    {
                        var fit = Fit.Line(Enumerable.Range(1, marks.Count).Select(x => (double)x).ToArray(), marks.ToArray());

                        Console.WriteLine($"{mark,9:N1} op/s {size,10:N0} MiB {fit.Item2,9:N1} op/s");
                    }
                    else
                    {
                        Console.WriteLine($"{mark,9:N1} op/s {size,10:N0} MiB");
                    }
                }
            }
        }
    }
}
