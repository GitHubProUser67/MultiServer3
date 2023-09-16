using System;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Modes.Gcm
{
    [Obsolete("Will be removed")]
    public interface IGcmExponentiator
	{
		void Init(byte[] x);
		void ExponentiateX(long pow, byte[] output);
	}
}
