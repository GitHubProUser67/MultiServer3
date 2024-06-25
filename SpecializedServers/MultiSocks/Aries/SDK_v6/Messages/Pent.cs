namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Pent : AbstractMessage
    {
        public override string _Name { get => "pent"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            OutputCache.Add("GFID", GetInputCacheValue("GFID"));

            client.SendMessage(this);
        }
    }
}