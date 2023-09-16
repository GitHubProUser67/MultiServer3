using System;

namespace MultiServer.Addons.Org.BouncyCastle.OpenSsl
{
	public interface IPasswordFinder
	{
		char[] GetPassword();
	}
}
