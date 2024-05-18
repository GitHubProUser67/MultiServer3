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
            {
                if (context.SKU == "PS3")
                    client.SendMessage(new SKeyOut() { SKEY = "$baadcodebaadcodebaadcodebaadcode", DP = "PS3/Burnout-Dec2007/mod" });
                else
                    client.SendMessage(new SKeyOut() { SKEY = "$51ba8aee64ddfacae5baefa6bf61e009", DP = "PC/Burnout-2008/na1",
                        GFID = "\"ODS:19038.110.Base Product;BURNOUT PARADISE ULTIMATE EDITION_PC_ONLINE_ACCESS\"", PLATFORM = "pc", PSID = "PS-REG-BURNOUT2008" });
            }
            else
                client.SendMessage(new SKeyOut());
        }
    }
}
