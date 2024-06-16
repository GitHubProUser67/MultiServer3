namespace MultiSocks.DirtySocks.Messages
{
    public class RgetIn : AbstractMessage
    {
        public override string _Name { get => "RGET"; }
        public string? LRSC { get; set; }
        public string? LIST { get; set; }
        public string? PRES { get; set; }
        public string? ID { get; set; }


        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RgetOut()
            {
                ID = ID,
                SIZE = "0"
            });

        }
    }
}
