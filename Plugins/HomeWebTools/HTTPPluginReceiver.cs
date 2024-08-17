using CyberBackendLibrary.HTTP;
using HomeTools.AFS;
using HTTPServer;
using HTTPServer.Models;
using CyberBackendLibrary.HTTP.PluginManager;
using HTTPServer.RouteHandlers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver.Core;
using System.Net;

namespace HomeWebTools
{
    public class HTTPPluginReceiver : HTTPPlugin
    {
        private string APIStaticFolder = Directory.GetCurrentDirectory() + "/static";

        public Task HTTPStartPlugin(string param, ushort port)
        {
            APIStaticFolder = param + "/!HomeTools";

            Directory.CreateDirectory(APIStaticFolder + "/HelperFiles");

            AFSClass.MapperHelperFolder = APIStaticFolder + "/HelperFiles";

            _ = new Timer(AFSClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

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
                        case "POST":

                            switch (HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(HTTPProcessor.DecodeUrl(request.Url.RawWithQuery)))
                            {
                                #region LibSecure HomeTools
                                case "/!HomeTools/MakeBarSdat/":
                                    (byte[]?, string)? makeres = HomeToolsInterface.MakeBarSdat(APIStaticFolder, new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (makeres != null)
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Content-disposition", $"attachment; filename={makeres.Value.Item2}");
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(makeres.Value.Item1).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/UnBar/":
                                    (byte[]?, string)? unbarres = HomeToolsInterface.UnBar(APIStaticFolder, new MemoryStream(request.DataAsBytes), request.ContentType, APIStaticFolder + "/HelperFiles").Result;
                                    if (unbarres != null)
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Content-disposition", $"attachment; filename={unbarres.Value.Item2}");
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(unbarres.Value.Item1).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/CDS/":
                                    (byte[]?, string)? cdsres = HomeToolsInterface.CDS(new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (cdsres != null)
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Content-disposition", $"attachment; filename={cdsres.Value.Item2}");
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(cdsres.Value.Item1).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/CDSBruteforce/":
                                    (byte[]?, string)? cdsbruteres = HomeToolsInterface.CDSBruteforce(new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (cdsbruteres != null)
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Content-disposition", $"attachment; filename={cdsbruteres.Value.Item2}");
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(cdsbruteres.Value.Item1).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/HCDBUnpack/":
                                    (byte[]?, string)? hcdbres = HomeToolsInterface.HCDBUnpack(new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (hcdbres != null)
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Content-disposition", $"attachment; filename={hcdbres.Value.Item2}");
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(hcdbres.Value.Item1).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/TicketList/":
                                    (byte[]?, string)? ticketlistres = HomeToolsInterface.TicketList(new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (ticketlistres != null)
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Content-disposition", $"attachment; filename={ticketlistres.Value.Item2}");
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(ticketlistres.Value.Item1).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/INF/":
                                    (byte[]?, string)? infres = HomeToolsInterface.INF(new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (infres != null)
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Content-disposition", $"attachment; filename={infres.Value.Item2}");
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(infres.Value.Item1).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/ChannelID/":
                                    string? channelres = HomeToolsInterface.ChannelID(new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (!string.IsNullOrEmpty(channelres))
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(channelres).Result;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentType = "text/plain";
                                        sent = response.Send().Result;
                                    }
                                    break;
                                case "/!HomeTools/SceneID/":
                                    string? sceneres = HomeToolsInterface.SceneID(new MemoryStream(request.DataAsBytes), request.ContentType);
                                    if (!string.IsNullOrEmpty(sceneres))
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "text/plain";
                                        sent = response.Send(sceneres).Result;
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
            else if (obj is HttpRequest request)
            {
                HttpResponse? response = null;

                if (!string.IsNullOrEmpty(request.RawUrlWithQuery))
                {
                    switch (request.Method)
                    {
                        case "POST":

                            switch (HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(request.RawUrlWithQuery))
                            {
                                #region LibSecure HomeTools
                                case "/!HomeTools/MakeBarSdat/":
                                    (byte[]?, string)? makeres = HomeToolsInterface.MakeBarSdat(APIStaticFolder, request.GetDataStream, request.GetContentType());
                                    if (makeres != null)
                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, makeres.Value.Item1, makeres.Value.Item2);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/UnBar/":
                                    (byte[]?, string)? unbarres = HomeToolsInterface.UnBar(APIStaticFolder, request.GetDataStream, request.GetContentType(), APIStaticFolder + "/HelperFiles").Result;
                                    if (unbarres != null)
                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, unbarres.Value.Item1, unbarres.Value.Item2);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/CDS/":
                                    (byte[]?, string)? cdsres = HomeToolsInterface.CDS(request.GetDataStream, request.GetContentType());
                                    if (cdsres != null)
                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, cdsres.Value.Item1, cdsres.Value.Item2);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/CDSBruteforce/":
                                    (byte[]?, string)? cdsbruteres = HomeToolsInterface.CDSBruteforce(request.GetDataStream, request.GetContentType());
                                    if (cdsbruteres != null)
                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, cdsbruteres.Value.Item1, cdsbruteres.Value.Item2);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/HCDBUnpack/":
                                    (byte[]?, string)? hcdbres = HomeToolsInterface.HCDBUnpack(request.GetDataStream, request.GetContentType());
                                    if (hcdbres != null)
                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, hcdbres.Value.Item1, hcdbres.Value.Item2);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/TicketList/":
                                    (byte[]?, string)? ticketlistres = HomeToolsInterface.TicketList(request.GetDataStream, request.GetContentType());
                                    if (ticketlistres != null)
                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, ticketlistres.Value.Item1, ticketlistres.Value.Item2);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/INF/":
                                    (byte[]?, string)? infres = HomeToolsInterface.INF(request.GetDataStream, request.GetContentType());
                                    if (infres != null)
                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, infres.Value.Item1, infres.Value.Item2);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/ChannelID/":
                                    string? channelres = HomeToolsInterface.ChannelID(request.GetDataStream, request.GetContentType());
                                    if (!string.IsNullOrEmpty(channelres))
                                        response = HttpResponse.Send(channelres);
                                    else
                                        response = HttpBuilder.InternalServerError();
                                    break;
                                case "/!HomeTools/SceneID/":
                                    string? sceneres = HomeToolsInterface.SceneID(request.GetDataStream, request.GetContentType());
                                    if (!string.IsNullOrEmpty(sceneres))
                                        response = HttpResponse.Send(sceneres);
                                    else
                                        response = HttpBuilder.InternalServerError();
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
