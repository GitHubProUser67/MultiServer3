namespace SRVEmu.DataStore
{
    public class DbAccount
    {
        public int ID;
        public string? Username;
        public string? Password; //todo: hash
        public string TOS = "0";
        public string SHARE = "1";
        public string MAIL = "defaultEA@gmail.com";
        public List<string> Personas = new();
    }
}
