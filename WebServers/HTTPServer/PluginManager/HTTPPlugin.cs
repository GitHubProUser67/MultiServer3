using HTTPServer.Models;

namespace HTTPServer.PluginManager
{
    public interface HTTPPlugin
    {
        Task HTTPStartPlugin(string param, ushort port);
        HttpResponse? ProcessPluginMessage(HttpRequest request);
    }
}