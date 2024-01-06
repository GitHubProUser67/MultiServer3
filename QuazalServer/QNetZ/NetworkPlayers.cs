using CustomLogger;

namespace QuazalServer.QNetZ
{
	public static class NetworkPlayers
	{
		public static uint RVCIDCounter = 0xBB98E;

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
            PlayerInfo plInfo = new();

			plInfo.Client = connection;
			plInfo.PID = 0;
			plInfo.RVCID = RVCIDCounter++;

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
			Players.RemoveAll(plInfo => { 
				if(plInfo.Client?.State == QClient.StateType.Dropped &&
					(DateTime.UtcNow - plInfo.Client.LastPacketTime).TotalSeconds > Constants.ClientTimeoutSeconds)
				{
					plInfo.OnDropped();
					LoggerAccessor.LogWarn($"[Quazal NetworkPlayers] - auto-dropping player: {plInfo.Name}");

					return true;
				}
				return false;
			});
		}

        public static uint GenerateUniqueUint(string input)
        {
            uint PID = (uint)input.GetHashCode();

            if (PID <= 1000)
                PID += Math.Min(1001, uint.MaxValue - PID + 1); // Ensure increment doesn't exceed remaining range + 1

            return PID ^ CalculateXorKey(input);
        }

        // Helper method to calculate XOR key from the input string
        private static uint CalculateXorKey(string input)
        {
            uint xorKey = 0;

            foreach (char c in input)
            {
                xorKey ^= c;
            }

            return xorKey;
        }
    }
}
