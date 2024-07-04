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
            return Players.Where(pl => pl.PID == pid).FirstOrDefault();
        }

		public static PlayerInfo? GetPlayerInfoByUsername(string userName)
		{
            return Players.Where(pl => pl.Name == userName).FirstOrDefault();
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

		public static void DropPlayerInfo(PlayerInfo playerInfo)
		{
			LoggerAccessor.LogWarn($"[Quazal NetworkPlayers] - dropping player: {playerInfo.Name}");

            if (playerInfo.Client != null)
                playerInfo.Client.PlayerInfo = null;

            playerInfo.OnDropped();
            Players.Remove(playerInfo);
        }

		public static void DropPlayers()
		{
			lock (lockObject) // Prevents the same action being done multiple times if the loop is very tight.
			{
                Players.RemoveAll(playerInfo => {
                    if (playerInfo.Client?.State != QClient.StateType.Dropped)
                        return false;
                    if (playerInfo.Client.TimeSinceLastPacket < Constants.ClientTimeoutSeconds)
                        return false;
                    LoggerAccessor.LogWarn($"[Quazal NetworkPlayers] - auto-dropping player: {playerInfo.Name}");
                    if (playerInfo.Client != null)
                        playerInfo.Client.PlayerInfo = null;
                    playerInfo.OnDropped();
                    return true;
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
