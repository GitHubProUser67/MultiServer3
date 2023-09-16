using System.Net;

namespace MultiServer.PluginManager
{
    public interface IPlugin
    {
        Task Start(string workingDirectory, IPluginHost host);
        Task HTTPExecute(HttpListenerRequest request, HttpListenerResponse response, string stringparam);
    }
}
