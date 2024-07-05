using CyberBackendLibrary.HTTP;
using HomeTools.AFS;
using MozaicHTTP;
using MozaicHTTP.Models;
using CyberBackendLibrary.HTTP.PluginManager;
using MozaicHTTP.RouteHandlers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebAPIService;

namespace EdNetCRCCalculator
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
            if (obj is HttpRequest request)
            {
                HttpResponse? response = null;

                if (!string.IsNullOrEmpty(request.Url))
                {
                    switch (request.Method)
                    {
                        case "POST":
                            byte[]? dataAsBytes = request.DataAsBytes;

                            if (dataAsBytes != null)
                            {
                                switch (HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(request.Url))
                                {
                                    #region LibSecure HomeTools
                                    case "/!HomeTools/MakeBarSdat/":
                                        (byte[]?, string)? makeres = HomeToolsInterface.MakeBarSdat(APIStaticFolder, new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (makeres != null)
                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, makeres.Value.Item1, makeres.Value.Item2);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/UnBar/":
                                        (byte[]?, string)? unbarres = HomeToolsInterface.UnBar(APIStaticFolder, new MemoryStream(dataAsBytes), request.GetContentType(), APIStaticFolder + "/HelperFiles").Result;
                                        if (unbarres != null)
                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, unbarres.Value.Item1, unbarres.Value.Item2);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/CDS/":
                                        (byte[]?, string)? cdsres = HomeToolsInterface.CDS(new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (cdsres != null)
                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, cdsres.Value.Item1, cdsres.Value.Item2);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/CDSBruteforce/":
                                        (byte[]?, string)? cdsbruteres = HomeToolsInterface.CDSBruteforce(new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (cdsbruteres != null)
                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, cdsbruteres.Value.Item1, cdsbruteres.Value.Item2);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/HCDBUnpack/":
                                        (byte[]?, string)? hcdbres = HomeToolsInterface.HCDBUnpack(new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (hcdbres != null)
                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, hcdbres.Value.Item1, hcdbres.Value.Item2);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/TicketList/":
                                        (byte[]?, string)? ticketlistres = HomeToolsInterface.TicketList(new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (ticketlistres != null)
                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, ticketlistres.Value.Item1, ticketlistres.Value.Item2);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/INF/":
                                        (byte[]?, string)? infres = HomeToolsInterface.INF(new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (infres != null)
                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, infres.Value.Item1, infres.Value.Item2);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/ChannelID/":
                                        string? channelres = HomeToolsInterface.ChannelID(new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (!string.IsNullOrEmpty(channelres))
                                            response = HttpResponse.Send(false, channelres);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                    case "/!HomeTools/SceneID/":
                                        string? sceneres = HomeToolsInterface.SceneID(new MemoryStream(dataAsBytes), request.GetContentType());
                                        if (!string.IsNullOrEmpty(sceneres))
                                            response = HttpResponse.Send(false, sceneres);
                                        else
                                            response = HttpBuilder.InternalServerError(false);
                                        break;
                                        #endregion

                                }
                            }
                            else
                                response = HttpBuilder.InternalServerError(false);

                            break;
                    }
                }

                return response;
            }

            return null;
        }
    }
}
