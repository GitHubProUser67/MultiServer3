namespace MultiSocks.DirtySocks.Model
{
    public class Game
    {
        public int MaxSize;
        public int MinSize;
        public int ID;

        public string CustFlags;
        public string Params;
        public string Name;
        public string Seed;
        public string SysFlags;
        public string? pass;

        public bool Priv;

        public UserCollection Users = new();

        public Game(int maxSize, int minSize, int id, string custFlags, string @params,
                string name, bool priv, string seed, string sysFlags, string? Pass)
        {
            MaxSize = maxSize;
            MinSize = minSize;
            ID = id;
            CustFlags = custFlags;
            Params = @params;
            Name = name;
            Priv = priv;
            Seed = seed;
            SysFlags = sysFlags;
            pass = Pass;
        }

        public bool RemoveUserAndCheckGameValidity(User user)
        {
            lock (Users)
            {
                Users.RemoveUser(user);

                if (Users.Count() < MinSize)
                    return true;
            }

            return false;
        }
    }
}
