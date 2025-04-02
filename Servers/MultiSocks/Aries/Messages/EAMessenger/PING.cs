namespace MultiSocks.Aries.Messages
{
    public class PING : AbstractMessage
    {
        public override string _Name { get => "PING"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            PING ping = new PING();

            client.SendMessage(ping);
        }
    }
}