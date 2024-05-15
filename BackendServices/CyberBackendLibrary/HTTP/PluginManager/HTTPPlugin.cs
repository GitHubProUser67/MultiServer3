using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberBackendLibrary.HTTP.PluginManager
{
    public interface HTTPPlugin
    {
        Task HTTPStartPlugin(string param, ushort port);
        object? ProcessPluginMessage(object request);
    }
}