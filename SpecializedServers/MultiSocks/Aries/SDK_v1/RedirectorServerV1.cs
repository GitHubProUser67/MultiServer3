using MultiSocks.Aries.SDK_v1;
using MultiSocks.Aries.SDK_v1.Messages;

namespace MultiSocks.Aries.SDK_v1
{
    public class RedirectorServerV1 : AbstractAriesServerV1
    {
        public override Dictionary<string, Type?> NameToClass { get; } =
            new Dictionary<string, Type?>()
            {
                { "@dir", typeof(Dir) },
                { "@tic", null } // We not respond so crypto is not applied.
            };

        public string RedirIP;
        public string RedirPort;

        public RedirectorServerV1(ushort port, string targetIP, ushort targetPort, string? Project = null, string? SKU = null, bool secure = false, string CN = "", string email = "", bool WeakChainSignedRSAKey = false) : base(port, targetIP, Project, SKU, secure, CN, email, WeakChainSignedRSAKey)
        {
            RedirIP = targetIP;
            RedirPort = targetPort.ToString();
        }
    }
}
