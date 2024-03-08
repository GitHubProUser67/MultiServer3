namespace SRVEmu.DirtySocks.Messages
{
    public class GqwkIn : AbstractMessage
    {
        public override string _Name { get => "gqwk"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new GqwkOut()
            {

            });
        }
    }
}
