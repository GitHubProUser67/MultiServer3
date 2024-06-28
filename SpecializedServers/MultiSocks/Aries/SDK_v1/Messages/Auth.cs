using MultiSocks.Aries.DataStore;
using MultiSocks.Aries.SDK_v1.Messages.ErrorCodes;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Auth : AbstractMessage
    {
        public override string _Name { get => "auth"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            string? VERS = GetInputCacheValue("VERS") ?? string.Empty;
            string? SKU = GetInputCacheValue("SKU") ?? string.Empty;
            string? MADDR = GetInputCacheValue("MADDR");
            string? NAME = GetInputCacheValue("NAME");
            string? PASS = GetInputCacheValue("PASS");
            string? MAC = GetInputCacheValue("MAC");
            string? LOC = GetInputCacheValue("LOC");

            if (context is not MatchmakerServerV1 mc) return;

            client.VERS = VERS;
            client.SKU = SKU;

            switch (VERS)
            {
                case "BURNOUT5/ISLAND":
                case "BURNOUT5/TROPHIES":
                case "BURNOUT5/31":
                case "BURNOUT5/PSN_DAVIS":
                case "BOTTEST":
                    if (SKU == "PS3")
                    {
                        string[]? maddrparams = MADDR?.Split('$');

                        if (maddrparams != null)
                            NAME = maddrparams.FirstOrDefault();
                    }
                    break;

            }

            DbAccount? user = AriesServer.Database?.GetByName(NAME);
            if (user == null)
            {
                client.SendMessage(new AuthImst());
                return;
            }

            mc.TryLogin(user, client, PASS, LOC ?? "enUS", MAC);
        }
    }
}
