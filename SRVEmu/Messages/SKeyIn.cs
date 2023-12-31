namespace SRVEmu.Messages
{
    public class SKeyIn : AbstractMessage
    {
        public override string _Name { get => "skey"; }
        public string? SKEY { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            //TODO: get actual session key
            client.SendMessage(new SKeyOut());
        }
    }
}
