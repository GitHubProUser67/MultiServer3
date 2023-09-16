using System.IO;

using MultiServer.Addons.Org.BouncyCastle.Security;
using MultiServer.Addons.Org.BouncyCastle.Utilities.IO.Pem;

namespace MultiServer.Addons.Org.BouncyCastle.OpenSsl
{
	/// <remarks>General purpose writer for OpenSSL PEM objects.</remarks>
	public class PemWriter
		: Utilities.IO.Pem.PemWriter
	{
		/// <param name="writer">The TextWriter object to write the output to.</param>
		public PemWriter(TextWriter writer)
			: base(writer)
		{
		}

		public void WriteObject(object obj) 
		{
			WriteObject(obj, null, null, null);
		}

		public void WriteObject(object obj, string algorithm, char[] password, SecureRandom random)
		{
            try
            {
                base.WriteObject(new MiscPemGenerator(obj, algorithm, password, random));
            }
            catch (PemGenerationException e)
            {
                if (e.InnerException is IOException inner)
                    throw inner;

                throw;
            }
		}
	}
}
