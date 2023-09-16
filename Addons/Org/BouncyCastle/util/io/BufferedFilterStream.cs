using System.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Utilities.IO
{
    internal sealed class BufferedFilterStream
        : FilterStream
    {
        internal BufferedFilterStream(Stream s)
            : this(s, Streams.DefaultBufferSize)
        {
        }

        internal BufferedFilterStream(Stream s, int bufferSize)
            : base(new BufferedStream(s, bufferSize))
        {
        }
    }
}
