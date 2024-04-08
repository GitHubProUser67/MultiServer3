using MultiSocks.DirtySocks.Messages;

namespace MultiSocks.DirtySocks
{
    public class RedirectorServer : AbstractDirtySockServer
    {
        public override Dictionary<string, Type?> NameToClass { get; } =
            new Dictionary<string, Type?>()
            {
                { "@dir", typeof(DirIn) },
                { "@tic", null } // We not respond so crypto is not applied.
            };

        public string RedirIP;
        public string RedirPort;

        public RedirectorServer(ushort port, string targetIP, ushort targetPort, bool lowlevel, string? Project = null, string? SKU = null, bool secure = false, string CN = "") : base(port, lowlevel, Project, SKU, secure, CN)
        {
            RedirIP = targetIP;
            RedirPort = targetPort.ToString();
        }
    }
}
