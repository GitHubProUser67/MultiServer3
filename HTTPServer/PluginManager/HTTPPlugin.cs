namespace HTTPServer.PluginManager
{
    public interface HTTPPlugin
    {
        Task HTTPStartPlugin(string param, int port);
    }
}