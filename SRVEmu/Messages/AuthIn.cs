using SRVEmu.DataStore;

namespace SRVEmu.Messages
{
    public class AuthIn : AbstractMessage
    {
        public override string _Name { get => "auth"; }

        public string? NAME { get; set; }
        public string? PASS { get; set; }
        public string? TOS { get; set; }
        public string? MID { get; set; }
        public string FROM { get; set; } = "US";
        public string LANG { get; set; } = "en";

        public string? PROD { get; set; }
        public string? VERS { get; set; }
        public string? SLUS { get; set; }
        public string? REGN { get; set; }
        public string? CLST { get; set; }
        public string? NETV { get; set; }
        public string? MINAGE { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            DbAccount? user = mc.Database.GetByName(NAME);
            if (user == null)
            {
                client.SendMessage(new AuthImst());
                return;
            }

            CustomLogger.LoggerAccessor.LogInfo("Logged in: " + user.Username);
            mc.TryLogin(user, client);
        }
    }
}
