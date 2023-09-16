using System;

namespace MultiServer.Addons.Org.BouncyCastle.Tls
{
    /// <summary>Base interface for an object sending and receiving DTLS data.</summary>
    public interface DatagramTransport
        : DatagramReceiver, DatagramSender, TlsCloseable
    {
    }
}
