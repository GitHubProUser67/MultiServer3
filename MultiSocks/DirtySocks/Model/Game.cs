namespace SRVEmu.DirtySocks.Model
{
    public class Game
    {
        public int MaxSize;
        public int MinSize;
        public int ID;
        public int Count; // Current Player count

        public string? CustFlags;
        public string? Params;
        public string? Name;
        public string? Priv;
        public string? Seed;
        public string? SysFlags;

        public UserCollection Users = new();
    }
}
