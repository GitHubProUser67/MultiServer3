namespace PSMultiServer.Addons.Medius.MEDIUS.Medius
{
    public interface IMediusComponent
    {
        int TCPPort { get; }
        int UDPPort { get; }

        void Start();
        Task Stop();
        Task Tick();
    }
}
