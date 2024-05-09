namespace MultiSocks.DirtySocks.Messages
{
    public class OpupIn : AbstractMessage
    {
        public override string _Name { get => "opup"; }
        public string? OFFPROG { get; set; }
        public string? DRVDEA { get; set; }
        public string? RIDDEA { get; set; }
        public string? FBURNCHALL { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new OpupOut());
        }
    }
}
