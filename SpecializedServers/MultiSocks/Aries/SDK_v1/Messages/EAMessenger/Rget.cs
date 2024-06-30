namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Rget : AbstractMessage
    {
        public override string _Name { get => "RGET"; }
        public string? LRSC { get; set; }
        public string? LIST { get; set; }
        public string? PRES { get; set; }
        public string? ID { get; set; }


        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(new RgetOut()
            {
                ID = GetInputCacheValue("ID"),
                SIZE = "0"
            });

        }
    }

    public class RgetOut : AbstractMessage
    {
        public override string _Name { get => "RGET"; }
        public string? ID { get; set; }
        public string? SIZE { get; set; }
    }
}
