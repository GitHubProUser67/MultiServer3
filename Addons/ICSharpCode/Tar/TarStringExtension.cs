using MultiServer.Addons.ICSharpCode.SharpZipLib.Core;

namespace MultiServer.Addons.ICSharpCode.SharpZipLib.Tar
{
	internal static class TarStringExtension
	{
		public static string ToTarArchivePath(this string s)
		{
			return PathUtils.DropPathRoot(s).Replace(Path.DirectorySeparatorChar, '/');
		}
	}
}
