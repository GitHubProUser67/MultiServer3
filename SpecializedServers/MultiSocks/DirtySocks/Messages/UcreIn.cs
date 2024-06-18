namespace MultiSocks.DirtySocks.Messages
{
    public class UcreIn : AbstractMessage
    {
        public override string _Name { get => "ucre"; }
        public string? NAME { get; set; }
        public string? SIZE { get; set; }
        public string? TYPE { get; set; }
        public string? UPDATES { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? CUSTFLAGS { get; set; }
        public string? DESC { get; set; }
        public string? PARAMS { get; set; }
        public string? PASS { get; set; }
        public string? SESS { get; set; }
        public string? AUX { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if(!string.IsNullOrEmpty(NAME))
            {
                client.SendMessage(new UcreOut()
                {
                    IDENT = "1",
                    NAME = NAME
                });
            } else
            {
                client.SendMessage(new UcreOut()
                {
                    IDENT = "1",
                    NAME = "UserSet1"
                });
            }
        }
    }
}
