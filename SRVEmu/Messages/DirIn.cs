namespace SRVEmu.Messages
{
    public class DirIn : AbstractMessage
    {
        public override string _Name { get => "@dir"; }

        public string? PROD { get; set; }
        public string? VERS { get; set; }
        public string? FROM { get; set; }
        public string? LANG { get; set; }
        public string? SLUS { get; set; }
        public string? REGN { get; set; }
        public string? CLST { get; set; }
        public string? NETV { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            DirOut response = new();
            var rc = context as RedirectorServer;
            if (rc == null) return;

            response.ADDR = rc.RedirIP;
            response.PORT = rc.RedirPort;

            client.SendMessage(response);
        }
    }
}
