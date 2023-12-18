using QuazalServer.QNetZ.DDL;

namespace QuazalServer.RDVServices.DDL.Models
{
	public class PresenceElement
	{
		public PresenceElement()
		{
			argument = new qBuffer();
		}

		public uint principalId { get; set; }
		public bool isConnected { get; set; }
		public int phraseId { get; set; }
		public qBuffer argument { get; set; }
	}
}
