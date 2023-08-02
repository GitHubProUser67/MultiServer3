using System.ComponentModel;
using System.Diagnostics;

namespace PSMultiServer.PoodleHTTP
{
    public static class NetAclChecker
    {
        public static void AddAddress(string address)
        {
            AddAddress(address, Environment.UserDomainName, Environment.UserName);
        }

        public static void AddAddress(string address, string domain, string user)
        {
            string args = $@"http add urlacl url={address}, user={domain}\{user}";

            try
            {
                ProcessStartInfo processStartInfo = new("netsh", args)
                {
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                };

                var process = Process.Start(processStartInfo);
                process?.WaitForExit();
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == 1223)
                {
                    ServerConfiguration.LogInfo("User canceled the operation by rejected the UAC.");
                }
                else
                {
                    ServerConfiguration.LogWarn($"Failed to 'netsh http add urlacl {address}' with an {nameof(Win32Exception)}.", e);
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogWarn($"Failed to 'netsh http add urlacl {address}'.", e);
            }
        }
    }
}
