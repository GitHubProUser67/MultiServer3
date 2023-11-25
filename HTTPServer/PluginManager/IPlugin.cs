namespace HTTPServer.PluginManager
{
    public interface IPlugin
    {
        Task HTTPStartPlugin(string param, int port);
    }
}
