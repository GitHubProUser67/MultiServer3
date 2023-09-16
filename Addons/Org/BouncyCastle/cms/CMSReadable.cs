using System;
using System.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Cms
{
	public interface CmsReadable
	{
		Stream GetInputStream();
	}
}
