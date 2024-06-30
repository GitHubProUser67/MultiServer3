namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Pent : AbstractMessage
    {
        public override string _Name { get => "pent"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            OutputCache.Add("GFID", GetInputCacheValue("GFID"));

            client.SendMessage(this);
        }
    }
}