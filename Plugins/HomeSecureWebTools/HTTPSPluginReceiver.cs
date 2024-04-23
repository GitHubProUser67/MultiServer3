﻿using HomeTools.AFS;
using CyberBackendLibrary.HTTP;
using HTTPSecureServerLite.PluginManager;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver.Core;
using System.Net;
using WebAPIService;

namespace HomeSecureWebTools
{
    public class HTTPSPluginReceiver : HTTPSecurePlugin
    {
        private string APIStaticFolder = Directory.GetCurrentDirectory() + "/static";

        public Task HTTPSecureStartPlugin(string param, ushort port)
        {
            APIStaticFolder = param + "/!HomeTools";

            Directory.CreateDirectory(APIStaticFolder + "/HelperFiles");

            AFSClass.MapperHelperFolder = APIStaticFolder + "/HelperFiles";

            _ = new Timer(AFSClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

            return Task.CompletedTask;
        }

        public bool ProcessPluginMessage(HttpRequestBase request, HttpResponseBase response)
        {
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
    }
}