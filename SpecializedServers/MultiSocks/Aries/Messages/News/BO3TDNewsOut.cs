namespace MultiSocks.Aries.Messages
{
    public class BO3TDNewsOut : AbstractMessage
    {
        public override string _Name { get => "news"; }
        public string? BUDDY_URL { get; set; } = "msgconn.beta.ea.com";
        public string? BUDDY_SERVER { get; set; } = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();
        public string? BUDDY_PORT { get; set; } = "10899";
        public string? TOSAC_URL { get; set; }
        public string? NEWS_URL { get; set; }
    }
}
