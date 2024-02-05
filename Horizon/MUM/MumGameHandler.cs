using CustomLogger;

namespace Horizon.MUM
{
    public class MumGameHandler
    {
        private readonly static List<Game> AccessibleGames = new();

        public static Task AddMumGame(Game gametotoadd)
        {
            lock (AccessibleGames)
            {
                AccessibleGames.Add(gametotoadd);
            }

            return Task.CompletedTask;
        }

        public static Task UpdateMumGame(int index, Game gametoupdate)
        {
            lock (AccessibleGames)
            {
                AccessibleGames[index] = gametoupdate;
            }

            return Task.CompletedTask;
        }

        public static int GetIndexOfLocalGameByNameAndAppId(string chatchannelname, string? gameName, int AppId)
        {
            if (string.IsNullOrEmpty(gameName))
                return -1;

            try
            {
                // Check if the AccessibleGames list is not empty
                if (AccessibleGames.Count > 0)
                {
                    // Find the index of the game that matches the given name and ID
                    int index = AccessibleGames
                        .FindIndex(game => game.GameName == gameName && game.ApplicationId == AppId && game.ChatChannel?.Name == chatchannelname);

                    // If a matching game is found, return its index
                    if (index != -1)
                        return index;
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetIndexOfLocalGameByNameAndAppId thrown an exception: {e}");
            }

            // If no matching game is found, return -1
            return -1;
        }
    }
}
