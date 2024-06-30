namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Ping : AbstractMessage
    {
        public override string _Name { get => "~png"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.Ping = (int)new TimeSpan(DateTime.Now.Ticks - client.PingSendTick).TotalMilliseconds;
        }
    }
}
