namespace MultiSocks.DirtySocks.Messages
{
    public class DirIn : AbstractMessage
    {
        public override string _Name { get => "@dir"; }

        public string? SKU { get; set; } // BOP
        public string? SDKVERS { get; set; } // BOP
        public string? BUILDDATE { get; set; } // BOP
        public string? PROD { get; set; }
        public string? VERS { get; set; }
        public string? FROM { get; set; }
        public string? LANG { get; set; }
        public string? SLUS { get; set; }
        public string? REGN { get; set; }
        public string? CLST { get; set; }
        public string? NETV { get; set; }
        public string? LOC { get; set; }
        public string? MID { get; set; }
        public string? ENTL { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            DirOut response = new();
            var rc = context as RedirectorServer;
            if (rc == null) return;
            Random? rand = new();
            response.SESS = (rand.Next(1000, 10000) + rand.Next(1000, 10000) + rand.Next(10, 100)).ToString();
            response.MASK = $"{rand.Next(1000, 10000)}f3f70ecb1757cd7001b9a7a{rand.Next(1000, 10000)}";
            rand = null;
            response.ADDR = rc.RedirIP;
            response.PORT = rc.RedirPort;

            client.SendMessage(response);
        }
    }
}
