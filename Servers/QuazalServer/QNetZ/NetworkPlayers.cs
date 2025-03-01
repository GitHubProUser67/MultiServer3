using CustomLogger;
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

            playerInfo.OnDropped();
            Players.Remove(playerInfo);
        }

		public static void DropPlayers()
		{
			lock (lockObject) // Prevents the same action being done multiple times if the loop is very tight.
			{
                Players.RemoveAll(playerInfo => {
                    if (playerInfo.Client != null)
                    {
                        if (playerInfo.Client.State != QClient.StateType.Dropped)
                            return false;
                        if (playerInfo.Client.TimeSinceLastPacket < Constants.ClientTimeoutSeconds)
                            return false;
                    }

                    LoggerAccessor.LogWarn($"[Quazal NetworkPlayers] - auto-dropping player: {playerInfo.Name}");

                    playerInfo.OnDropped();
                    return true;
                });
            }
		}

        public static uint GenerateUniqueUint(string input)
        {
            uint result = 0;

            do
            {
                result = NetChecksummer.CRC32.Create(Encoding.UTF8.GetBytes(input + "QnetZM$3")) ^ result;

                // If below or equal 1000, modify the input slightly and recalculate.
                if (result <= 1000)
                    input += "_retry";
            }
            while (result <= 1000);

            return result;
        }
    }
}
