using SRVEmu.DataStore;

namespace SRVEmu.Messages
{
    public class AcctIn : AbstractMessage
    {
        public override string _Name { get => "acct"; }

        public string? NAME { get; set; }
        public string PASS { get; set; } = string.Empty;

        public string? MAIL { get; set; }
        public string? BORN { get; set; }
        public string? GEND { get; set; }
        public string? SPAM { get; set; }
        public string? ALTS { get; set; }
        public string? FROM { get; set; }
        public string? LANG { get; set; }
        public string? PROD { get; set; }
        public string? VERS { get; set; }
        public string? SLUS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            DbAccount info = new()
            {
                Username = NAME,
                Password = PASS,
            };

            bool created = mc.Database.CreateNew(info);
            if (created) {
                CustomLogger.LoggerAccessor.LogInfo("Created new account: " + info.Username);
                client.SendMessage(new AcctOut()
                {
                    NAME = NAME,
                    PERSONAS = string.Empty,
                    AGE = "24"
                });
            }
            else
                client.SendMessage(new AcctDupl());
        }
    }

    public class AcctDupl : AbstractMessage
    {
        public override string _Name { get => "acctdupl"; }
    }
}
