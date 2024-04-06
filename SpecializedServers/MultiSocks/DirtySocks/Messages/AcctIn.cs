using MultiSocks.DirtySocks.DataStore;
using System.Text.RegularExpressions;

namespace MultiSocks.DirtySocks.Messages
{
    public class AcctIn : AbstractMessage
    {
        public override string _Name { get => "acct"; }

        public string? SKU { get; set; }
        public string? SDKVERS { get; set; }
        public string? BUILDDATE { get; set; }
        public string TOS { get; set; } = "1";
        public string SHARE { get; set; } = "1";
        public string? TICK { get; set; }
        public string MADDR { get; set; } = string.Empty;
        public string? MAC { get; set; }
        public string? MINAGE { get; set; }
        public string? NAME { get; set; }
        public string PASS { get; set; } = string.Empty;
        public string MAIL { get; set; } = "tsbo@freeso.net";
        public string? BORN { get; set; }
        public string? GEND { get; set; }
        public string? SPAM { get; set; }
        public string? ALTS { get; set; }
        public string? FROM { get; set; }
        public string? LANG { get; set; }
        public string? PROD { get; set; }
        public string? VERS { get; set; }
        public string? SLUS { get; set; }
        public string? LOC { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            if (VERS == "BURNOUT5/ISLAND")
            {
                if (SKU == "PS3")
                {
                    // Create a Regex object and match the pattern
                    Regex regex = new(@"(.*?)\$");

                    Match match = regex.Match(MADDR);

                    // Check if a match is found
                    if (match.Success)
                        // Extract and print the captured value
                        NAME = match.Groups[1].Value;
                }

                DbAccount info = new()
                {
                    Username = NAME,
                    TOS = TOS,
                    SHARE = SHARE,
                    MAIL = MAIL,
                    Password = PASS,
                };

                bool created = DirtySocksServer.Database.CreateNew(info);
                if (created)
                {
                    CustomLogger.LoggerAccessor.LogInfo("Created new account: " + info.Username);
                    client.SendMessage(new AcctOut()
                    {
                        NAME = NAME,
                        PERSONAS = NAME,
                        AGE = "24"
                    });
                }
                else
                    client.SendMessage(new AcctDupl());
            }
            else if (VERS == "BURNOUT5/PSN_DAVIS")
            {
                // Create a Regex object and match the pattern
                Regex regex = new(@"(.*?)\$");

                Match match = regex.Match(MADDR);

                // Check if a match is found
                if (match.Success)
                    // Extract and print the captured value
                    NAME = match.Groups[1].Value;

                DbAccount info = new()
                {
                    Username = NAME,
                    TOS = TOS,
                    SHARE = SHARE,
                    MAIL = MAIL,
                    Password = PASS,
                };

                bool created = DirtySocksServer.Database.CreateNew(info);
                if (created)
                {
                    CustomLogger.LoggerAccessor.LogInfo("Created new account: " + info.Username);
                    client.SendMessage(new AcctOut()
                    {
                        NAME = NAME,
                        PERSONAS = NAME,
                        AGE = "24"
                    });
                }
                else
                    client.SendMessage(new AcctDupl());
            }
            else
            {
                DbAccount info = new()
                {
                    Username = NAME,
                    Password = PASS,
                };

                bool created = DirtySocksServer.Database.CreateNew(info);
                if (created)
                {
                    CustomLogger.LoggerAccessor.LogInfo("Created new account: " + info.Username);
                    client.SendMessage(new AcctOut()
                    {
                        NAME = NAME,
                        PERSONAS = NAME,
                        AGE = "24"
                    });
                }
                else
                    client.SendMessage(new AcctDupl());
            }
        }
    }

    public class AcctDupl : AbstractMessage
    {
        public override string _Name { get => "acctdupl"; }
    }
}
