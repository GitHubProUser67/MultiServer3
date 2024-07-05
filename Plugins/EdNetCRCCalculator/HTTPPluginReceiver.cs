using CyberBackendLibrary.HTTP;
using MozaicHTTP;
using MozaicHTTP.Models;
using CyberBackendLibrary.HTTP.PluginManager;
using System;
using System.Threading.Tasks;

namespace EdNetCRCCalculator
{
    public class HTTPPluginReceiver : HTTPPlugin
    {
        public Task HTTPStartPlugin(string param, ushort port)
        {
            return Task.CompletedTask;
        }

        public object? ProcessPluginMessage(object obj)
        {
            if (obj is HttpRequest request)
            {
                HttpResponse? response = null;

                if (!string.IsNullOrEmpty(request.Url))
                {
                    switch (request.Method)
                    {
                        case "GET":

                            switch (HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(request.Url))
                            {
                                #region EdNet CRC Tools
                                case "/!EdNet/GetCRC/":
                                    if (request.QueryParameters != null && request.QueryParameters.ContainsKey("str"))
                                    {
                                        if (request.QueryParameters.ContainsKey("v2") && request.QueryParameters["v2"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                                            response = HttpResponse.Send(false, EdNetService.CRCUtils.GetCRCFromStringHexadecimal(request.QueryParameters["str"], true));
                                        else
                                            response = HttpResponse.Send(false, EdNetService.CRCUtils.GetCRCFromStringHexadecimal(request.QueryParameters["str"], false));
                                    }
                                    else
                                        response = HttpBuilder.InternalServerError(false);
                                    break;
                                    #endregion
                            }

                            break;
                    }
                }

                return response;
            }

            return null;
        }
    }
}
