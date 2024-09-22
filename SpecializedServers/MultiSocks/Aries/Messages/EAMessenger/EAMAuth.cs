using MultiSocks.Aries.DataStore;

namespace MultiSocks.Aries.Messages
{
    public class EAMAuth : AbstractMessage
    {
        public override string _Name { get => "AUTH"; }

        public string? TOS { get; set; }
        public string? NAME { get; set; }
        public string MAIL { get; set; } = "tsbo@freeso.net";
        public string? BORN { get; set; }
        public string? GEND { get; set; }
        public string? FROM { get; set; }
        public string? SHARE { get; set; }
        public string? GFIDS { get; set; }
        public string? LANG { get; set; }
        public string? LOC { get; set; }
        public string SPAM { get; set; } = "NN";
        public string? PERSONAS { get; set; } // comma separated list
        public string? LAST { get; set; }
        public string? SINCE { get; set; }
        public string? ADDR { get; set; }
        public string? LUID { get; set; }
        public string? TOKEN { get; set; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not EAMessengerServer mc) return;

            string? PRES = GetInputCacheValue("PRES");
            string? USER = GetInputCacheValue("USER");
            string? PROD = GetInputCacheValue("PROD");
            string? VERS = GetInputCacheValue("VERS");
            string? PASS = GetInputCacheValue("PASS");

            DbAccount? user = AriesServer.Database?.GetByName(USER.Split("/").First());
            if (user == null)
            {
                client.SendMessage(this);
                return;
            }

            CustomLogger.LoggerAccessor.LogInfo("EA Messenger Logged in: " + user.Username);
            mc.TryEAMLogin(user, client, VERS);
        }
    }
}
