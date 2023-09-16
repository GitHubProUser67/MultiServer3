namespace MultiServer.Addons.Org.BouncyCastle.Crypto
{
    public interface IMacDerivationFunction
        : IDerivationFunction
    {
        IMac Mac { get; }
    }
}
