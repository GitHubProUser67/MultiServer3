using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace DotNetty.Extensions
{
    public static class Extention
    {
        /// <summary>
        /// 获取IByteBuffer中的byte[]
        /// </summary>
        /// <param name="byteBuffer">IByteBuffer</param>
        /// <returns></returns>
        public static byte[] ToBytes(this IByteBuffer byteBuffer)
        {
            int readableBytes = byteBuffer.ReadableBytes;
            if (readableBytes == 0)
                return ArrayExtensions.ZeroBytes;

            //if (byteBuffer.HasArray)
            //{
            //    return byteBuffer.Array.Slice(byteBuffer.ArrayOffset + byteBuffer.ReaderIndex, readableBytes);

            //}

            byte[]? bytes = new byte[readableBytes];
            byteBuffer.GetBytes(byteBuffer.ReaderIndex, bytes);
            return bytes;
        }
    }
}
