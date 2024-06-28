namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Newsnew0 : AbstractMessage
    {
        public override string _Name { get => "newsnew0"; }
        public string BUDDYSERVERNAME { get; set; } = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();
        public string? BUDDYRESOURCE { get; set; }
        public string BUDDYPORT { get; set; } = "10899";
    }
}
