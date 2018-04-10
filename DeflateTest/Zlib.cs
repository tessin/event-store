using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeflateTest
{
    static class Zlib
    {
        [DllImport("zlibwapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int compressBound(int sourceLen);

        [DllImport("zlibwapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int compress2(byte[] dest, ref int destLen, byte[] source, int sourceLen, int level);
    }

    class ZlibException : Exception
    {
        public int ErrorCode { get; }

        enum constants
        {
            Z_BUF_ERROR = -5,
        }

        public ZlibException(int errorCode)
            : base($"{(constants)errorCode} ({errorCode})")
        {
            ErrorCode = errorCode;
        }
    }
}
