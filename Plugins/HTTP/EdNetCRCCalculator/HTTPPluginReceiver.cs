using NetworkLibrary.HTTP;
using NetworkLibrary.HTTP.PluginManager;
using System;
using System.Threading.Tasks;
using WatsonWebserver.Core;
using System.Net;

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
            if (obj is HttpContextBase ctx)
            {
                HttpRequestBase request = ctx.Request;
                HttpResponseBase response = ctx.Response;

                bool sent = false;

                if (!string.IsNullOrEmpty(request.Url.RawWithQuery))
                {
                    switch (request.Method.ToString())
                    {
                        case "GET":

                            switch (HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(HTTPProcessor.DecodeUrl(request.Url.RawWithQuery)))
                            {
                                #region EdNet CRC Tools
                                case "/!EdNet/GetCRC/":
                                    if (request.QuerystringExists("str"))
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        if (request.QuerystringExists("v2") && request.RetrieveQueryValue("v2").Equals("true", StringComparison.InvariantCultureIgnoreCase))
                                            sent = response.Send(EdNetService.CRCUtils.GetCRCFromStringHexadecimal(request.RetrieveQueryValue("str"), true)).Result;
                                        else
                                            sent = response.Send(EdNetService.CRCUtils.GetCRCFromStringHexadecimal(request.RetrieveQueryValue("str"), false)).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                    #endregion
                            }

                            break;
                    }
                }

                return sent;
            }

            return null;
        }
    }
}
