namespace QuazalServer.QNetZ.Attributes
{
	/// <summary>
	/// RMC class attribute identifying class as a service/protocol hanlder
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class RMCServiceAttribute : Attribute
	{
		public readonly RMCProtocolId ProtocolId;
		public RMCServiceAttribute(RMCProtocolId protocolId)
		{
			ProtocolId = protocolId;
		}
	}
}