using MultiSocks.DirtySocks.DataStore;

namespace MultiSocks.DirtySocks.Messages
{
    public class EAMAuthIn : AbstractMessage
    {
        public override string _Name { get => "AUTH"; }

        public string? PRES { get; set; }
        public string? USER { get; set; }
        public string? PROD { get; set; }
        public string? VERS { get; set; }
        public string? PASS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as EAMessengerServer;
            if (mc == null) return;

            DbAccount? user = DirtySocksServer.Database.GetByName(USER.Split("/").First());
            if (user == null)
            {
                client.SendMessage(new EAMAuthOut());
                return;
            }

            CustomLogger.LoggerAccessor.LogInfo("EA Messenger Logged in: " + user.Username);
            mc.TryEAMLogin(user, client, VERS);
        }
    }
}
