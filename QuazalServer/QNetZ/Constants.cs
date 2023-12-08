namespace QuazalServer.QNetZ
{
	public static class Constants
	{
		public static readonly string KeyDATA = "CD&ML";            // default ancient Quazal encryption key (RC4)
		public static readonly int PacketFragmentMaxSize = 963;
		public static readonly int PacketResendTimeSeconds = 2;
		public static readonly int ClientTimeoutSeconds = 60;

		public static byte[] SessionKey = new byte[] {
			0x9C, 0xB0, 0x1D, 0x7A, 0x2C, 0x5A,
			0x6C, 0x5B, 0xED, 0x12, 0x68, 0x45,
			0x69, 0xAE, 0x09, 0x0D
		};
	}
}
