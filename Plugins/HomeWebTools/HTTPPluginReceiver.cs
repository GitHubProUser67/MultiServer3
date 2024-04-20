using CyberBackendLibrary.HTTP;
using HomeTools.AFS;
using HTTPServer;
using HTTPServer.Models;
using HTTPServer.PluginManager;
using HTTPServer.RouteHandlers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebAPIService;

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

        public HttpResponse? ProcessPluginMessage(HttpRequest request)
        {
            HttpResponse? response = null;

            if (!string.IsNullOrEmpty(request.Url))
            {
                switch (request.Method)
                {
                    case "POST":

                        switch (HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(request.Url))
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
    }
}
