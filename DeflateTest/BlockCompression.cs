using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateTest
{
    struct BlockCompressionBuffer
    {
        public readonly byte[] Input;
        public readonly byte[] Output;

        public bool IsEmpty => (Input == null) & (Output == null);

        public int BlockSize => Input.Length;

        public BlockCompressionBuffer(int blockSize)
        {
            // https://github.com/madler/zlib/blob/2fa463bacfff79181df1a5270fb67cc679a53e71/compress.c#L81-L86
            // compressBound(sourceLen): sourceLen + sourceLen/4096 + sourceLen/16384 + sourceLen/33554432 + 13

            Input = new byte[blockSize];
            Output = new byte[Zlib.compressBound(blockSize)];
        }
    }

    class BlockCompression
    {
        public static void Compress(Stream input, Stream output, BlockCompressionBuffer buffer)
        {
            if (!input.CanRead) throw new ArgumentException("stream must be readable", nameof(input));
            if (!output.CanWrite) throw new ArgumentException("stream must be writable", nameof(output));

            var inputBlock = buffer.Input;
            var outputBlock = buffer.Output;
            var blockSize = buffer.BlockSize;

            var w = new BinaryWriter(output);

            for (; ; )
            {
                int read = input.Read(inputBlock, 0, blockSize);
                if (read == 0)
                {
                    break;
                }

                int compressedSize = CompressBlock(inputBlock, outputBlock, read);

                // todo: if compressedSize < blockSize-(2+13) else no compression

                w.Write((ushort)compressedSize);
                w.Write(outputBlock, 0, compressedSize);

                if (read < blockSize)
                {
                    break;
                }
            }
        }

        private static int CompressBlock(byte[] inputBlock, byte[] outputBlock, int read)
        {
            int compressedSize = outputBlock.Length;

            int result = Zlib.compress2(outputBlock, ref compressedSize, inputBlock, read, 9); // 1-9, fast to slow, -1 for default
            if (!(result == 0))
            {
                throw new ZlibException(result);
            }

            return compressedSize;

            //var compressed = new MemoryStream(outputBlock, true);

            //var deflate = new DeflateStream(compressed, CompressionLevel.Optimal, true);
            //deflate.Write(inputBlock, 0, read);
            //deflate.Close();

            //return (int)compressed.Position;
        }
    }
}
