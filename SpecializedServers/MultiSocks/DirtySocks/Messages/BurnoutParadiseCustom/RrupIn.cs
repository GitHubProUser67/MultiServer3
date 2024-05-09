namespace MultiSocks.DirtySocks.Messages
{
    public class RrupIn : AbstractMessage
    {
        public override string _Name { get => "rrup"; }
        public string? SKEY { get; set; }
        public string? R { get; set; }
        public string? V { get; set; }
        public string? C { get; set; }
        public string? U { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RrupOut());
        }
    }
}
