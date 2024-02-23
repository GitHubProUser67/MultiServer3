namespace SRVEmu.DirtySocks.Messages
{
    public class Ping : AbstractMessage
    {
        public override string _Name { get => "~png"; }

        public string? TIME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.Ping = (int)new TimeSpan(DateTime.Now.Ticks - client.PingSendTick).TotalMilliseconds;
        }
    }
}
