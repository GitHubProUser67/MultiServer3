using HTTPServer.Models;

namespace HTTPServer.PluginManager
{
    public interface HTTPPlugin
    {
        Task HTTPStartPlugin(string param, int port);
        HttpResponse ProcessPluginMessage(HttpRequest request);
    }
}