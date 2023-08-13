namespace PSMultiServer.Addons.Horizon.Server.Plugins.Interface
{
    public interface IPlugin
    {
        Task Start(string workingDirectory, IPluginHost host);
    }
}
