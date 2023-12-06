using SRVEmu.Messages;

namespace SRVEmu
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

        public RedirectorServer(ushort port, string targetIP, ushort targetPort, bool lowlevel) : base(port, lowlevel)
        {
            RedirIP = targetIP;
            RedirPort = targetPort.ToString();
        }
    }
}
