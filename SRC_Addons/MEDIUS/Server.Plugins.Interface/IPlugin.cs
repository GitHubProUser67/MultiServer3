namespace PSMultiServer.SRC_Addons.MEDIUS.Server.Plugins.Interface
{
    public interface IPlugin
    {
        Task Start(string workingDirectory, IPluginHost host);
    }
}
