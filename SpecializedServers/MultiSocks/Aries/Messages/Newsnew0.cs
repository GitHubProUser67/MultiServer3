namespace MultiSocks.Aries.Messages
{
    public class Newsnew0 : AbstractMessage
    {
        public override string _Name { get => "newsnew0"; }
        public string BUDDYSERVERNAME { get; set; } = MultiSocksServerConfiguration.UsePublicIPAddress ? NetworkLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : NetworkLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();
        public string? BUDDYRESOURCE { get; set; }
        public string BUDDYPORT { get; set; } = "10899";
    }
}
