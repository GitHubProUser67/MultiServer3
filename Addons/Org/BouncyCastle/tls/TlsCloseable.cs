using System;
using System.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Tls
{
    public interface TlsCloseable
    {
        /// <exception cref="IOException"/>
        void Close();
    }
}
