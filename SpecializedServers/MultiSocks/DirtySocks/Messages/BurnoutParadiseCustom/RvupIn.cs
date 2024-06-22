namespace MultiSocks.DirtySocks.Messages
{
    public class RvupIn : AbstractMessage
    {
        public override string _Name { get => "rvup"; }
        public string? RIVAL0 { get; set; }
        public string? RIVAL1 { get; set; }
        public string? RIVAL2 { get; set; }
        public string? RIVAL3 { get; set; }
        public string? RIVAL4 { get; set; }
        public string? RIVAL5 { get; set; }
        public string? RIVAL6 { get; set; }
        public string? RIVAL7 { get; set; }
        public string? RIVAL8 { get; set; }
        public string? RIVAL9 { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RvupOut());
        }
    }
}