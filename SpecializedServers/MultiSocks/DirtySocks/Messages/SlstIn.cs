namespace SRVEmu.DirtySocks.Messages
{
    public class SlstIn : AbstractMessage
    {
        public override string _Name { get => "slst"; }
        public string? LOC { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            client.SendMessage(new SlstOut());
        }
    }
}
