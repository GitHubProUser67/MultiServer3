namespace MultiSocks.DirtySocks.Messages
{
    public class HchkIn : AbstractMessage
    {
        public override string _Name { get => "hchk"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new HchkOut());
        }
    }
}
