namespace MultiSocks.DirtySocks.Messages
{
    public class SKeyIn : AbstractMessage
    {
        public override string _Name { get => "skey"; }
        public string? SKEY { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            //TODO: get actual session key
            if (!string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5"))
                client.SendMessage(new SKeyOut() { SKEY = "$baadcodebaadcodebaadcodebaadcode", DP = "PS3/Burnout-Dec2007/mod" } );
            else
                client.SendMessage(new SKeyOut());
        }
    }
}
