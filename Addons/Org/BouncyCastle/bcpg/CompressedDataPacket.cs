using System.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Bcpg
{
	/// <remarks>Generic compressed data object.</remarks>
    public class CompressedDataPacket
        : InputStreamPacket
    {
        private readonly CompressionAlgorithmTag algorithm;

		internal CompressedDataPacket(
            BcpgInputStream bcpgIn)
			: base(bcpgIn)
        {
            this.algorithm = (CompressionAlgorithmTag) bcpgIn.ReadByte();
        }

		/// <summary>The algorithm tag value.</summary>
        public CompressionAlgorithmTag Algorithm
		{
			get { return algorithm; }
		}
    }
}
