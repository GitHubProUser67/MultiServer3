using CustomLogger;

namespace QuazalServer.QNetZ
{
	public class DO
	{
		public static void HandlePacket(QPacketHandlerPRUDP handler, QPacket p, QClient client)
		{
			LoggerAccessor.LogError("[DO] - Duplicated objects are not implemented on this server.");
		}
	}
}
