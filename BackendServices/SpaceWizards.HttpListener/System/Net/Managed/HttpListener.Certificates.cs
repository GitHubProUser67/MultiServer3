// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
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

        internal X509Certificate LoadCertificateAndKey(IPAddress addr, int port)
        {
            lock (_internalLock)
            {
                // Actually load the certificate
                try
                {
                    if (_certificateCache != null && _certificateCache.TryGetValue(port, out X509Certificate2 certificate))
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
                            return new X509Certificate2(cert_file, File.ReadAllText(pass_file));
                        }
                        return new X509Certificate2(cert_file);
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
