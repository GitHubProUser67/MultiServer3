namespace MultiSocks.DirtySocks.Messages
{
    public class UsldIn : AbstractMessage
    {
        public override string _Name { get => "usld"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            client.SendMessage(new UsldOut());

            client.Ping = (int)new TimeSpan(DateTime.Now.Ticks - client.PingSendTick).TotalMilliseconds;
        }
    }
}
