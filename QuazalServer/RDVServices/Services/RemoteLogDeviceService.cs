using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	[RMCService(RMCProtocolId.RemoteLogDeviceService)]
	public class RemoteLogDeviceService : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult Log(string strLine)
		{
			CustomLogger.LoggerAccessor.LogInfo($"[RMC] - Recieved from PID={Context?.Client?.Info?.PID}: {strLine}");
			return Error(0);
		}
	}
}
