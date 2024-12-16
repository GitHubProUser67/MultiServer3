using MultiSocks.Aries.Messages;
using System.Collections.Concurrent;

namespace MultiSocks.Aries.Model
{
    public class GameCollection
    {
        private int GameID = 1;
        public ConcurrentDictionary<int, AriesGame> GamesSessions = new();

        public virtual AriesGame? AddGame(int maxSize, int minSize, string custFlags, string @params,
                string name, bool priv, string seed, string sysFlags, string? pass, int roomId)
        {
            if (!GamesSessions.Values.Any(game =>
                    game.Name == name))
            {
                AriesGame game = new(maxSize, minSize, GameID, custFlags, @params,
                                name, priv, seed, sysFlags, pass, roomId);
                GameID++;
                GamesSessions.TryAdd(game.ID, game);
                return game;
            }
            else
                CustomLogger.LoggerAccessor.LogWarn("[Game] - Trying to add a game while an other with same properties exists!");

            return null;
        }

        public virtual void RemoveGame(AriesGame game)
        {
            if (GamesSessions.ContainsKey(game.ID))
            {
                CustomLogger.LoggerAccessor.LogWarn($"[Game] - Removing Game:{game.Name}:{game.ID}.");

                foreach (AriesUser user in GamesSessions[game.ID].Users.GetAll())
                {
                    user.Connection?.SendMessage(new Gdel());

                    user.CurrentGame = null;

                    user.SendPlusWho(user, user.Connection?.Context.Project);
                }

                GamesSessions.TryRemove(game.ID, out _);
            }
        }

        public void UpdateGame(AriesGame game)
        {
            if (GamesSessions.ContainsKey(game.ID))
                GamesSessions[game.ID] = game;
        }

        public AriesGame? GetGameByName(string? name, string? pass)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return GamesSessions.FirstOrDefault(x => x.Value.Name == name && (string.IsNullOrEmpty(x.Value.pass) || x.Value.pass == pass)).Value;
        }

        public int Count()
        {
            return GamesSessions.Count;
        }
    }
}
