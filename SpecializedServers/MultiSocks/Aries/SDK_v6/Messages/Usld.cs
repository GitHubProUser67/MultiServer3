namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Usld : AbstractMessage
    {
        public override string _Name { get => "usld"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            if (context is not MatchmakerServerV6) return;

            OutputCache.Add("IMGATE", "0");
            OutputCache.Add("QMSG0", "Wanna play?");
            OutputCache.Add("QMSG1", "I rule!");
            OutputCache.Add("QMSG2", "Doh!");
            OutputCache.Add("QMSG3", "Mmmm... doughnuts.");
            OutputCache.Add("QMSG04", "What time is it?");
            OutputCache.Add("QMSG05", "The truth is out of style.");
            OutputCache.Add("SPM_EA", "1");
            OutputCache.Add("SPM_PART", "0");
            OutputCache.Add("UID", "$00000000000003fe");

            client.SendMessage(this);
        }
    }
}
