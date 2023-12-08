using DSFServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	[RMCService(RMCProtocolId.NotificationEventManager)]
	public class NotificationEventManager : RMCServiceBase
	{
		[RMCMethod(1)]
		public void Notify(NotificationEvent notification)
		{
			// Dummy event
		}
	}
}
