namespace PSMultiServer.Addons.Medius.Server.Plugins.Interface
{
    public interface IPlugin
    {
        Task Start(string workingDirectory, IPluginHost host);
    }
}
