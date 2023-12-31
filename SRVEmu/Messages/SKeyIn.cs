namespace SRVEmu.Messages
{
    public class SKeyIn : AbstractMessage
    {
        public override string _Name { get => "skey"; }
        public string? SKEY { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (SKEY == "$5075626c6963204b6579")
            {
                client.SendMessage(new SKeyOut()
                {
                    DP = "PC/Burnout-2008/na1",
                    GFID = "\"ODS:19038.110.Base Product;BURNOUT PARADISE ULTIMATE EDITION_PC_ONLINE_ACCESS\"",
                    PLATFORM = "pc",
                    PSID = "PS-REG-BURNOUT2008",
                    SKEY = "$51ba8aee64ddfacae5baefa6bf61e009"
                });
            }
            else
                //TODO: get actual session key
                client.SendMessage(new SKeyOut());
        }
    }
}
