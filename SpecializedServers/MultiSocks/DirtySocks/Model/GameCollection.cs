namespace MultiSocks.DirtySocks.Model
{
    public class GameCollection
    {
        private int GameID = 1;
        private List<Game> Games = new();

        public virtual Game? AddGame(int maxSize, int minSize, string custFlags, string @params,
                string name, bool priv, string seed, string sysFlags, string? pass)
        {
            lock (Games)
            {
                if (!Games.Any(game =>
                    game.MaxSize == maxSize &&
                    game.MinSize == minSize &&
                    game.CustFlags == custFlags &&
                    game.Params == @params &&
                    game.Name == name &&
                    game.Priv == priv &&
                    game.Seed == seed &&
                    game.SysFlags == sysFlags &&
                    game.pass == pass))
                {
                    Game game = new(maxSize, minSize, GameID, custFlags, @params,
                                    name, priv, seed, sysFlags, pass);
                    GameID++;
                    Games.Add(game);
                    return game;
                }
                else
                    CustomLogger.LoggerAccessor.LogWarn("[Game] - Trying to add a game while an other with same properties exists!");
            }

            return null;
        }

        public virtual void RemoveGame(Game game)
        {
            lock (Games)
            {
                CustomLogger.LoggerAccessor.LogWarn($"[Game] - Removing Game:{game.Name}:{game.ID}.");
                Games.Remove(game);
            }
        }

        public Game? GetGameByName(string? name, string? pass)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            lock (Games)
            {
                return Games.FirstOrDefault(x => x.Name == name && (!string.IsNullOrEmpty(x.pass) ? x.pass == pass : (x.pass == null || x.pass == string.Empty)));
            }
        }

        public int Count()
        {
            lock (Games)
            {
                return Games.Count;
            }
        }
    }
}
