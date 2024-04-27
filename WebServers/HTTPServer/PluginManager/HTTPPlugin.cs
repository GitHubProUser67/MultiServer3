using HTTPServer.Models;
using System.Threading.Tasks;

namespace HTTPServer.PluginManager
{
    public interface HTTPPlugin
    {
        Task HTTPStartPlugin(string param, ushort port);
        HttpResponse? ProcessPluginMessage(HttpRequest request);
    }
}