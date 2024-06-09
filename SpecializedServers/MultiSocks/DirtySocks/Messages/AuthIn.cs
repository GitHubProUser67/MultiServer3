using MultiSocks.DirtySocks.DataStore;
using System.Text.RegularExpressions;

namespace MultiSocks.DirtySocks.Messages
{
    public class AuthIn : AbstractMessage
    {
        public override string _Name { get => "auth"; }

        public string? SKU { get; set; }
        public string? SDKVERS { get; set; }
        public string? BUILDDATE { get; set; }
        public string? SHARE { get; set; }
        public string? GEND { get; set; }
        public string? SPAM { get; set; }
        public string? NAME { get; set; }
        public string? MAIL { get; set; }
        public string? TICK { get; set; }
        public string MADDR { get; set; } = string.Empty;
        public string? MAC { get; set; }
        public string? PASS { get; set; }
        public string? TOS { get; set; }
        public string? MID { get; set; }
        public string? PID { get; set; }
        public string? HWFLAG { get; set; }
        public string? HWMASK { get; set; }
        public string? LOGD { get; set; }
        public string FROM { get; set; } = "US";
        public string LANG { get; set; } = "en";
        public string? LOC { get; set; }
        public string? PROD { get; set; }
        public string VERS { get; set; } = string.Empty;
        public string? SLUS { get; set; }
        public string? REGN { get; set; }
        public string? CLST { get; set; }
        public string? NETV { get; set; }
        public string? MINAGE { get; set; }
        public string? ENTL { get; set; }
        public string? MASK { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            if (VERS == "BURNOUT5/ISLAND" || VERS == "BURNOUT5/TROPHIES" || VERS == "BURNOUT5/31" || VERS == "BOTTEST")
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
            }

            DbAccount? user = DirtySocksServer.Database.GetByName(NAME);
            if (user == null)
            {
                client.SendMessage(new AuthImst());
                return;
            }

            CustomLogger.LoggerAccessor.LogInfo("Logged in: " + user.Username);
            mc.TryLogin(user, client, VERS);
        }
    }
}
