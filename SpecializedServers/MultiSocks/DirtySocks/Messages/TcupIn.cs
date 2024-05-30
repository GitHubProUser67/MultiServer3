namespace MultiSocks.DirtySocks.Messages
{
    public class TcupIn : AbstractMessage
    {
        public override string _Name { get => "tcup"; }
        public string? SCORE { get; set; }
        public string? SKEY { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new TcupOut());
        }
    }
}
