namespace PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Medius
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
