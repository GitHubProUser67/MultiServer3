namespace SRVEmu.Messages
{
    public class SeleIn : AbstractMessage
    {
        public override string _Name { get => "sele"; }

        public string ROOMS { get; set; } = "1";

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            //TODO: provide some actual statistics
            client.SendMessage(new SeleOut());
        }
    }
}
