using CustomLogger;
using System.Security.Cryptography;
using System.Text;

namespace QuazalServer.QNetZ
{
	public static class NetworkPlayers
	{
		public static uint RVCIDCounter = 0xBB98E;

        private static object lockObject = new();

        public static readonly List<PlayerInfo> Players = new();

        public static PlayerInfo? GetPlayerInfoByPID(uint pid)
		{
			foreach (PlayerInfo pl in Players)
			{
				if (pl.PID == pid)
					return pl;
			}
			return null;
		}

		public static PlayerInfo? GetPlayerInfoByUsername(string userName)
		{
			foreach (PlayerInfo pl in Players)
			{
				if (pl.Name == userName)
					return pl;
			}
			return null;
		}

		public static PlayerInfo CreatePlayerInfo(QClient connection)
		{
            PlayerInfo plInfo = new()
            {
                Client = connection,
                PID = 0,
                RVCID = RVCIDCounter++
            };

            Players.Add(plInfo);

			return plInfo;
		}

		public static void PurgeAllPlayers()
		{
			Players.Clear();
		}

		public static void DropPlayerInfo(PlayerInfo plInfo)
		{
			if (plInfo.Client != null)
                plInfo.Client.Info = null;

            plInfo.OnDropped();
			LoggerAccessor.LogWarn($"[Quazal NetworkPlayers] - dropping player: {plInfo.Name}");
			
			Players.Remove(plInfo);
		}

		public static void DropPlayers()
		{
			lock (lockObject) // Prevents the same action being done multiple times if the loop is very tight.
			{
                Players.RemoveAll(plInfo => {
                    if (plInfo.Client?.State == QClient.StateType.Dropped &&
                        (DateTime.UtcNow - plInfo.Client.LastPacketTime).TotalSeconds > Constants.ClientTimeoutSeconds)
                    {
                        plInfo.OnDropped();
                        LoggerAccessor.LogWarn($"[Quazal NetworkPlayers] - auto-dropping player: {plInfo.Name}");

                        return true;
                    }
                    return false;
                });
            }
		}

        public static uint GenerateUniqueUint(string input)
        {
            // Convert input string to bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Compute hash using MD5 algorithm
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(hashBytes);

            // Take the first 4 bytes of the hash as the uint value
            return Math.Max(BitConverter.ToUInt32(hashBytes, 0), 1000);
        }
    }
}
