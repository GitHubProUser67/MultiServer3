// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SpaceWizards.HttpListener
{
    public partial class HttpListener
    {
        internal SslStream CreateSslStream(Stream innerStream, bool ownsStream, RemoteCertificateValidationCallback callback)
        {
            return new SslStream(innerStream, ownsStream, callback);
        }

        internal X509Certificate2 LoadCertificateAndKey(IPAddress addr, int port)
        {
            X509Certificate2 certificate;

            lock (_internalLock)
            {
                // Actually load the certificate
                try
                {
                    if (_certificateCache != null && _certificateCache.TryGetValue(port, out certificate))
                    {
                        return certificate;
                    }

                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".mono");
                    path = Path.Combine(path, "httplistener");
                    string cert_file = Path.Combine(path, String.Format("{0}.pfx", port));
                    if (File.Exists(cert_file))
                    {
                        string pass_file = Path.Combine(path, String.Format("{0}.password.txt", port));
                        if (File.Exists(pass_file))
                        {
                            certificate = new X509Certificate2(cert_file, File.ReadAllText(pass_file));
                        }
                        else
                        {
                            certificate = new X509Certificate2(cert_file);
                        }

#if !NETCOREAPP2_1_OR_GREATER || !NETSTANDARD2_1_OR_GREATER
                        if (CertificateHelper.IsCertificateAuthority(certificate))
                        {
                            throw new NotSupportedException("The certificate store will only accept Authorities with .NETCORE 2.1 and up or .NETSTANDARD 2.1 and up");
                        }
#endif

                        return certificate;
                    }
                }
                catch
                {
                    // ignore errors
                }
            }

            return null;
        }
    }
}
