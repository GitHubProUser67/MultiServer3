namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Rcat : AbstractMessage
    {
        public override string _Name { get => "rcat"; }
        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(new RcatOut()
            {
                CAT = "temp,,tp,res0,res1,res2,res3,1",
            });
        }
    }

    public class RcatOut : AbstractMessage
    {
        public override string _Name { get => "rcat"; }
        public string? CAT { get; set; }
    }
}