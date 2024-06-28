using MultiSocks.Aries.DataStore;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class EAMAuth : AbstractMessage
    {
        public override string _Name { get => "AUTH"; }

        public string? PRES { get; set; }
        public string? USER { get; set; }
        public string? PROD { get; set; }
        public string? VERS { get; set; }
        public string? PASS { get; set; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            var mc = context as EAMessengerServer;
            if (mc == null) return;

            DbAccount? user = AriesServer.Database.GetByName(GetInputCacheValue("USER").Split("/").First());
            if (user == null)
            {
                client.SendMessage(new EAMAuthOut());
                return;
            }

            CustomLogger.LoggerAccessor.LogInfo("EA Messenger Logged in: " + user.Username);
            mc.TryEAMLogin(user, client);
        }
    }

    public class EAMAuthOut : AbstractMessage
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
    }
}
