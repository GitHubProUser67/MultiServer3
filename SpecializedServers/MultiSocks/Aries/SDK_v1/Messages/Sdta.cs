namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Sdta : AbstractMessage
    {
        public override string _Name { get => "sdta"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            OutputCache.Add("SLOT", "0");
            OutputCache.Add("STATS", "1,2,3,4,5,6,7,8,9,10,11,12,13");

            client.SendMessage(this);
        }
    }
}
