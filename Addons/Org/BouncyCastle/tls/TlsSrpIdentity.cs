using System;

namespace MultiServer.Addons.Org.BouncyCastle.Tls
{
    /// <summary>Processor interface for an SRP identity.</summary>
    public interface TlsSrpIdentity
    {
        byte[] GetSrpIdentity();

        byte[] GetSrpPassword();
    }
}
