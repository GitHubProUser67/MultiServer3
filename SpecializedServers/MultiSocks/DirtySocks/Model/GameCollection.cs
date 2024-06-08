using MultiSocks.DirtySocks.Messages;
using System.Collections.Concurrent;

namespace MultiSocks.DirtySocks.Model
{
    public class GameCollection
    {
        private int GameID = 1;
        public ConcurrentDictionary<int, Game> GamesSessions = new();

        public virtual Game? AddGame(int maxSize, int minSize, string custFlags, string @params,
                string name, bool priv, string seed, string sysFlags, string? pass, int roomId)
        {
            if (!GamesSessions.Values.Any(game =>
                    game.Name == name &&
                    game.Priv == priv))
            {
                Game game = new(maxSize, minSize, GameID, custFlags, @params,
                                name, priv, seed, sysFlags, pass, roomId);
                GameID++;
                GamesSessions.TryAdd(game.ID, game);
                return game;
            }
            else
                CustomLogger.LoggerAccessor.LogWarn("[Game] - Trying to add a game while an other with same properties exists!");

            return null;
        }

        public virtual void RemoveGame(Game game)
        {
            if (GamesSessions.ContainsKey(game.ID))
            {
                CustomLogger.LoggerAccessor.LogWarn($"[Game] - Removing Game:{game.Name}:{game.ID}.");

                foreach (User user in GamesSessions[game.ID].Users.GetAll())
                {
                    user.Connection?.SendMessage(new GdelOut());

                    user.CurrentGame = null;

                    if (!string.IsNullOrEmpty(user.Connection?.Context.Project))
                    {
                        if (user.Connection.Context.Project.Contains("DPR-09"))
                            user.SendPlusWho(user, "DPR-09");
                        else if (user.Connection.Context.Project.Contains("BURNOUT5"))
                            user.SendPlusWho(user, "BURNOUT5");
                    }
                    else
                        user.SendPlusWho(user, string.Empty);
                }

                GamesSessions.TryRemove(game.ID, out _);
            }
        }

        public void UpdateGame(Game game)
        {
            if (GamesSessions.ContainsKey(game.ID))
                GamesSessions[game.ID] = game;
        }

        public Game? GetGameByName(string? name, string? pass)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return GamesSessions.FirstOrDefault(x => x.Value.Name == name && (!string.IsNullOrEmpty(x.Value.pass) ? x.Value.pass == pass : (x.Value.pass == null || x.Value.pass == string.Empty))).Value;
        }

        public int Count()
        {
            return GamesSessions.Count;
        }
    }
}
