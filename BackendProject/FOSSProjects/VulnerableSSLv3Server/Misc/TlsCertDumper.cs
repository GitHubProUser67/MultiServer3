using System.Net.Sockets;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Tls.Crypto;

namespace BackendProject.FOSSProjects.VulnerableSSLv3Server.Misc;

public static class TlsCertDumper
{
    public static (string IssuerDN, string SubjectDN) DumpPubFeslCert(string serverHost, int port)
    {
        BcTlsCrypto crypto = new(new SecureRandom());

        using TcpClient tcpClient = new TcpClient(serverHost, port);
        using NetworkStream? networkStream = tcpClient.GetStream();
        TlsClientProtocol tlsClientProtocol = new(networkStream);

        TlsAuthCertDumper certDumper = new();
        Ssl3TlsClient tlsClient = new(crypto, certDumper);

        CustomLogger.LoggerAccessor.LogInfo($"[VulnerableSSLv3Server.Misc:TlsCertDumper] - Connecting to {serverHost}:{port}...");

        try
        {
            tlsClientProtocol.Connect(tlsClient);
            CustomLogger.LoggerAccessor.LogInfo("[VulnerableSSLv3Server.Misc:TlsCertDumper] - SSL Handshake with backend server successful!?!?");
            CustomLogger.LoggerAccessor.LogInfo("[VulnerableSSLv3Server.Misc:TlsCertDumper] - Closting conection");
            tlsClientProtocol.Close();
        }
        catch
        {
            // Intentionally swallow exceptions, we just need the certificate
        }

        TlsCertificate? serverCert = certDumper.ServerCertificates?[0];

        if (serverCert == null)
        {
            CustomLogger.LoggerAccessor.LogWarn("[VulnerableSSLv3Server.Misc:TlsCertDumper] - Failed to retrieve certificate from upstream server!");
            return (string.Empty, string.Empty);
        }

        // Gymnastics to extract exact DN strings from the upstream certificate.
        X509Certificate cCertificate = new(serverCert.GetEncoded());
        var x509cert = DotNetUtilities.ToX509Certificate(cCertificate);
        byte[] certDer = x509cert.GetRawCertData();

        X509Certificate bc = new X509CertificateParser().ReadCertificate(certDer);
        TbsCertificateStructure tbsCertificate = TbsCertificateStructure.GetInstance(Asn1Object.FromByteArray(bc.GetTbsCertificate()));

        string issuer = tbsCertificate.Issuer.ToString();
        string subject = tbsCertificate.Subject.ToString();

        CustomLogger.LoggerAccessor.LogInfo("[VulnerableSSLv3Server.Misc:TlsCertDumper] - Copying from upstream certificate:");
        CustomLogger.LoggerAccessor.LogInfo($"[VulnerableSSLv3Server.Misc:TlsCertDumper] - Issuer: {issuer}");
        CustomLogger.LoggerAccessor.LogInfo($"[VulnerableSSLv3Server.Misc:TlsCertDumper] - Subject: {subject}");

        return (issuer, subject);
    }
}

public class TlsAuthCertDumper : TlsAuthentication
{
    public TlsCertificate[]? ServerCertificates { get; private set; }

    public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
    {
        throw new NotImplementedException();
    }

    public void NotifyServerCertificate(TlsServerCertificate serverCertificate)
    {
        ServerCertificates = serverCertificate.Certificate.GetCertificateList();
    }
}