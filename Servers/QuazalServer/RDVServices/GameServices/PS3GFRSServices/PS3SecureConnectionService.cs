using CustomLogger;
using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;
using System.Text;
using QuazalServer.RDVServices.RMC;
using NetworkLibrary.Extension;

namespace QuazalServer.RDVServices.GameServices.PS3GFRSServices
{
    /// <summary>
	/// Secure connection service protocol
	/// </summary>
	[RMCService((ushort)RMCProtocolId.SecureConnectionService)]
    public class PS3SecureConnectionService : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult? Register(List<string> vecMyURLs)
        {
            if (Context != null)
            {
                // change address
                StationURL rdvConnectionUrl = new(vecMyURLs.Last().ToString())
                {
                    Address = Context.Client.Endpoint.Address.ToString()
                };
                rdvConnectionUrl["type"] = 3;

                RegisterResult result = new()
                {
                    pidConnectionID = Context.Client.PlayerInfo?.RVCID ?? 0,
                    retval = (int)ErrorCode.Core_NoError,
                    urlPublic = rdvConnectionUrl
                };

                return Result(result);
            }

            return null;
        }

        [RMCMethod(2)]
        public void RequestConnectionData()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(3)]
        public void RequestUrls()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(4)]
        public RMCResult RegisterEx(ICollection<StationURL> vecMyURLs, AnyData<SonyNPTicket> hCustomData)
        {
            if (hCustomData.data != null && hCustomData.data.ticket != null && hCustomData.data.ticket.data != null && Context != null && Context.Client.PlayerInfo != null)
            {
                // Extract the desired portion of the binary data
                byte[] extractedData = new byte[0x63 - 0x54 + 1];

                // Copy it
                //Array.Copy(hCustomData.data.ticket.data, 0x50, extractedData, 0, extractedData.Length);

                // Convert 0x00 bytes to 0x48 so FileSystem can support it
                for (int i = 0; i < extractedData.Length; i++)
                {
                    if (extractedData[i] == 0x00)
                        extractedData[i] = 0x48;
                }

                if (ByteUtils.FindBytePattern(hCustomData.data.ticket.data, new byte[] { 0x52, 0x50, 0x43, 0x4E }, 184) != -1)
                    LoggerAccessor.LogInfo($"[PS3SecureConnectionService] : User {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on RPCN");
                else
                    LoggerAccessor.LogInfo($"[PS3SecureConnectionService] : User {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on PSN");

                // change address
                StationURL rdvConnectionUrl = new(vecMyURLs.Last().ToString())
                {
                    Address = Context.Client.Endpoint.Address.ToString()
                };
                rdvConnectionUrl["type"] = 3;

                RegisterResult result = new()
                {
                    pidConnectionID = Context.Client.PlayerInfo.RVCID,
                    retval = (int)ErrorCode.Core_NoError,
                    urlPublic = rdvConnectionUrl
                };

                return Result(result);
            }
            else
                LoggerAccessor.LogError($"[RMC Secure] Error: Unknown Custom Data class {hCustomData.className}");

            return Error((int)ErrorCode.RendezVous_ClassNotFound);
        }

        [RMCMethod(5)]
        public void TestConnectivity()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(6)]
        public void UpdateURLs()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(7)]
        public void ReplaceURL()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(8)]
        public void SendReport()
        {
            UNIMPLEMENTED();
        }
    }
}
