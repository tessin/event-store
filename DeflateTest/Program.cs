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
            //var text2 = File.ReadAllText(@"..\..\text.txt");
            var text2 = File.ReadAllText(@"rand.txt");

            for (int i = 1; i <= 20; i++)
            {
                var text = text2.Substring(0, Math.Min(text2.Length, 100 * i));

                var buffer = new MemoryStream();

                var deflate = new DeflateStream(buffer, CompressionLevel.Optimal, true);

                var textWriter = new StreamWriter(deflate, _encoding);
                textWriter.Write(text);
                textWriter.Flush();

                deflate.Dispose();

                Console.WriteLine($"{text.Length:N0}\t{(double)_encoding.GetByteCount(text) / buffer.Length:N2}\t{_encoding.GetByteCount(text) - buffer.Length:N0}");
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
