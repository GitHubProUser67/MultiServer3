namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Snap : AbstractMessage
    {
        public override string _Name { get => "snap"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            OutputCache.Add("START", "0");
            OutputCache.Add("SEQN", "0");
            OutputCache.Add("RANGE", "0");
            OutputCache.Add("CHAN", "1");

            client.SendMessage(this);
        }
    }
}
