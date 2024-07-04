using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
{
    /// <summary>
    /// Hermes Game Info protocol
    /// </summary>
    [RMCService(RMCProtocolId.GameInfoService)]
	class GameInfoService : RMCServiceBase
	{
		// files which server can renturn
		private static readonly string[] FileList = {
			"OnlineConfig.ini"
		};

        [RMCMethod(5)]
		public RMCResult GetFileInfoList(int indexStart, int numElements, string stringSearch)
		{
            List<PersistentInfo> fileList = new();

			if (!string.IsNullOrEmpty(stringSearch))
			{
                if (stringSearch == "*")
                {
                    foreach (string name in FileList.Skip(indexStart).Take(numElements))
                    {
                        string path = Path.Combine(QuazalServerConfiguration.QuazalStaticFolder + "/StaticFiles", name);

                        Console.WriteLine(path);

                        if (!File.Exists(path))
                            continue;

                        fileList.Add(new PersistentInfo
                        {
                            m_name = name,
                            m_size = (uint)new FileInfo(path).Length
                        });
                    }
                }
                else
                {
                    if (File.Exists(Path.Combine(QuazalServerConfiguration.QuazalStaticFolder + "/StaticFiles", stringSearch)))
                    {
                        fileList.Add(new PersistentInfo
                        {
                            m_name = stringSearch,
                            m_size = (uint)new FileInfo(Path.Combine(QuazalServerConfiguration.QuazalStaticFolder + "/StaticFiles", stringSearch)).Length
                        });
                    }
                }
            }

			return Result(fileList);
		}

        [RMCMethod(7)]
        public RMCResult UKN7() // Assasin's creed 3 doesn't like our response and block online connection.
        {
            UNIMPLEMENTED();
            return new RMCResult(new RMCPResponseEmpty(), true, 0x10001);
        }
    }
}
