using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.v2Services
{
    /// <summary>
    /// Authentication service (ticket granting)
    /// </summary>
    [RMCService(RMCProtocolId.TicketGrantingService)]
	public class TicketGrantingServiceLoginData : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult Login(string userName)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		/// <summary>
		/// Function where client login is performed by account ID and password
		/// </summary>
		[RMCMethod(2)]
		public RMCResult LoginEx(string userName, AnyData<LoginData> oExtraData)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(3)]
		public RMCResult RequestTicket(uint sourcePID, uint targetPID)
		{
            if (Context != null)
            {
                KerberosTicket kerberos = new(sourcePID, targetPID, Constants.SessionKey, Constants.ticket);

                TicketData ticketData = new()
                {
                    retVal = (int)ErrorCode.Core_NoError,
                };

                if (sourcePID == 100) // Quazal guest account.
                    ticketData.pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, "h7fyctiuucf");
                else
                {
                    UNIMPLEMENTED();
                }

                return Result(ticketData);
            }

            return Error(0);
        }
    }
}
