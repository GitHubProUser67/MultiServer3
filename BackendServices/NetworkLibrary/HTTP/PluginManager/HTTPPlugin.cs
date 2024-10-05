using System.Threading.Tasks;

namespace NetworkLibrary.HTTP.PluginManager
{
    public interface HTTPPlugin
    {
        Task HTTPStartPlugin(string param, ushort port);
        object ProcessPluginMessage(object request);
    }
}