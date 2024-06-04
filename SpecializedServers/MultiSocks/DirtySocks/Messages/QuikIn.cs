namespace MultiSocks.DirtySocks.Messages
{
    public class QuikIn : AbstractMessage
    {
        public override string _Name { get => "quik"; }
        public string? KIND { get; set; }
        public string? QMFP { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new QuikOut());
            //client.SendMessage(new GcreOut());
        }
    }
}
