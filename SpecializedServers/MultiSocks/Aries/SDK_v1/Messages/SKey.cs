namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class SKey : AbstractMessage
    {
        public override string _Name { get => "skey"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            //TODO: get actual session key

            client.SKEY = "$51ba8aee64ddfacae5baefa6bf61e009";

            OutputCache.Add("SKEY", client.SKEY);

            if (context.SKU == "PS3")
                OutputCache.Add("DP", "PS3/Burnout-Dec2007/mod");
            else
                OutputCache.Add("DP", "XBOX360/Burnout-Dec2007/mod");

            client.SendMessage(this);
        }
    }
}
