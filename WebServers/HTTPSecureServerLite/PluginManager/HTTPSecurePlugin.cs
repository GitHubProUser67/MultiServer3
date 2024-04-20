using System.Threading.Tasks;
using WatsonWebserver.Core;

namespace HTTPSecureServerLite.PluginManager
{
    public interface HTTPSecurePlugin
    {
        Task HTTPSecureStartPlugin(string param, ushort port);
        bool ProcessPluginMessage(HttpRequestBase request, HttpResponseBase response);
    }
}