using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;

namespace QuazalServer.RDVServices.Services
{
	/// <summary>
	/// Secure connection service protocol
	/// </summary>
	[RMCService(RMCProtocolId.SecureConnectionService)]
	public class SecureConnectionService : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult? Register(List<string> vecMyURLs)
		{
            if (Context != null && Context.Client.PlayerInfo != null)
			{
                // change address
                StationURL rdvConnectionUrl = new(vecMyURLs.Last().ToString())
                {
                    Address = Context.Client.Endpoint.Address.ToString()
                };
                rdvConnectionUrl["type"] = 3;

                return Result(new RegisterResult()
                {
                    pidConnectionID = Context.Client.PlayerInfo.RVCID,
                    retval = (int)ErrorCode.Core_NoError,
                    urlPublic = rdvConnectionUrl
                });
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
		public RMCResult RegisterEx(ICollection<StationURL> vecMyURLs, AnyData<UbiAuthenticationLoginCustomData> hCustomData)
		{
			if (hCustomData.data != null && Context != null && Context.Client.PlayerInfo != null)
			{
                // change address
                StationURL rdvConnectionUrl = new(vecMyURLs.Last().ToString())
                {
                    Address = Context.Client.Endpoint.Address.ToString()
                };
                rdvConnectionUrl["type"] = 3;

				return Result(new RegisterResult()
                {
                    pidConnectionID = Context.Client.PlayerInfo.RVCID,
                    retval = (int)ErrorCode.Core_NoError,
                    urlPublic = rdvConnectionUrl
                });
			}
			else
                CustomLogger.LoggerAccessor.LogInfo($"[RMC Secure] Error: Unknown Custom Data class {hCustomData.className}");

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
