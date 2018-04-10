using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateTest
{
    class Program
    {
        class Test
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public Guid Id2 { get; set; } = Guid.NewGuid();
            public string Text { get; set; } = "Note that we can write a lot faster than we can read but as soon as we enable compression, writing takes a tremendous hit. However, without compression the bandwith requirement is 250 Mbit/s.";
            public decimal Amount { get; set; } = 50000;
            public decimal Amount2 { get; set; } = 100000;
            public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
        }

        static void Main(string[] args)
        {
            Text();
            return;

            var a = Uncompressed(TimeSpan.FromSeconds(10));
            var b = Compressed(TimeSpan.FromSeconds(10));

            Console.WriteLine($"{((double)a / b):N2}");
        }

        private static void Text()
        {
            var text = File.ReadAllText(@"..\..\text.txt");
            //var text2 = File.ReadAllText(@"rand.txt");

            var blockSizes = new int[] { 1 * 1024, 4 * 1024, 8 * 1024, 16 * 1024 };

            Console.Write("N;uncompressed");
            foreach (var blockSize in blockSizes)
            {
                Console.Write($";compressed{blockSize}");
            }
            foreach (var blockSize in blockSizes)
            {
                Console.Write($";ratio{blockSize}");
            }
            foreach (var blockSize in blockSizes)
            {
                Console.Write($";throughput{blockSize}");
            }
            Console.WriteLine();

            for (int i = 1; i <= 16; i++)
            {
                var bytes = _encoding.GetBytes(text.Substring(0, i * 1024));

                var output = new MemoryStream();

                //var deflate = new DeflateStream(buffer, CompressionLevel.Optimal, true);

                //var textWriter = new StreamWriter(deflate, _encoding);
                //textWriter.Write(text);
                //textWriter.Flush();

                //deflate.Dispose();

                var cols = new List<Tuple<int, int, double, double>>();

                foreach (var blockSize in blockSizes)
                {
                    var blockBuffer = new BlockCompressionBuffer(blockSize);

                    var input = new MemoryStream(bytes, false);

                    var sw = new Stopwatch();

                    int n;
                    for (n = 0; n < 1000; n++)
                    {
                        // reset
                        input.Position = 0;

                        output.Position = 0;
                        output.SetLength(0);

                        sw.Start();

                        BlockCompression.Compress(input, output, blockBuffer);

                        sw.Stop();
                    }

                    var t = bytes.Length / sw.Elapsed.TotalSeconds; // Kbytes / s

                    var uncompressedSize = bytes.Length;
                    var compressedSize = (int)output.Length;
                    var ratio = compressedSize / (double)uncompressedSize;

                    cols.Add(Tuple.Create(uncompressedSize, compressedSize, ratio, sw.Elapsed.TotalSeconds)); // actually milliseconds
                }

                Console.Write($"{i};{cols[0].Item1}");
                foreach (var col in cols)
                {
                    Console.Write($";{col.Item2}");
                }
                foreach (var col in cols)
                {
                    Console.Write($";{col.Item3:0.00}");
                }
                foreach (var col in cols)
                {
                    Console.Write($";{col.Item4:0.00}");
                }
                Console.WriteLine();
            }
        }

        private static void Report(string title, double n, double m, double s)
        {
            var x = (n / s);
            var y = ((1d / 1024) * m / s);
            Console.WriteLine($"{title,16}: {x,7:N0} op/s {y,7:N0} KiB/s {x / y,5:N2} op/KiB");
        }

        private static void Report(string title, double n, double m, double s, double nm1, double mm1)
        {
            Report(title, n, m, s);
        }

        private static UTF8Encoding _encoding = new UTF8Encoding(false, false);

        private static long Uncompressed(TimeSpan t)
        {
            // it doesn't matter that this benchmark is faster, 
            // we don't have the ridiculous bandwidth needed to run at max speed

            var sw = new Stopwatch();

            var n = 0;
            var m = 0L;

            MemoryStream buffer = null;

            while (sw.Elapsed < t)
            {
                sw.Start();

                buffer = new MemoryStream();

                var textWriter = new StreamWriter(buffer, _encoding);
                var jsonWriter = new JsonTextWriter(textWriter);
                var sr = new JsonSerializer();
                sr.Serialize(jsonWriter, new Test());
                jsonWriter.Flush();

                sw.Stop();
                n++;
                m += buffer.Length;
            }

            Report("Write", n, m, sw.Elapsed.TotalSeconds);

            var nm1 = n;
            var mm1 = m;

            sw.Reset();

            n = 0;
            m = 0L;

            var source = buffer.ToArray();

            while (sw.Elapsed < t)
            {
                sw.Start();

                buffer = new MemoryStream(source, false);

                var textReader = new StreamReader(buffer, _encoding);
                var jsonReader = new JsonTextReader(textReader);
                var sr = new JsonSerializer();
                sr.Deserialize<Test>(jsonReader);

                sw.Stop();
                n++;
                m += buffer.Length;
            }

            Report("Read", n, m, sw.Elapsed.TotalSeconds, nm1, mm1);

            return source.Length;
        }

        private static long Compressed(TimeSpan t)
        {
            var sw = new Stopwatch();

            var n = 0;
            var m = 0L;

            MemoryStream buffer = null;

            while (sw.Elapsed < t)
            {
                sw.Start();

                buffer = new MemoryStream();

                var deflate = new DeflateStream(buffer, CompressionLevel.Optimal, true);

                var textWriter = new StreamWriter(deflate, _encoding);
                var jsonWriter = new JsonTextWriter(textWriter);
                var sr = new JsonSerializer();
                sr.Serialize(jsonWriter, new Test());
                jsonWriter.Flush();

                deflate.Dispose();

                sw.Stop();
                n++;
                m += buffer.Length;
            }

            Report("DeflateWrite", n, m, sw.Elapsed.TotalSeconds);

            var nm1 = n;
            var mm1 = m;

            sw.Reset();

            n = 0;
            m = 0L;

            var source = buffer.ToArray();

            while (sw.Elapsed < t)
            {
                sw.Start();

                buffer = new MemoryStream(source, false);

                var deflate = new DeflateStream(buffer, CompressionMode.Decompress);

                var textReader = new StreamReader(deflate, _encoding);
                var jsonReader = new JsonTextReader(textReader);
                var sr = new JsonSerializer();
                sr.Deserialize<Test>(jsonReader);

                sw.Stop();
                n++;
                m += buffer.Length;
            }

            Report("DeflateRead", n, m, sw.Elapsed.TotalSeconds, nm1, mm1);

            return source.Length;
        }
    }
}
