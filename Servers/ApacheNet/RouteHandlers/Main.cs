using NetworkLibrary.HTTP;
using System.Collections.Generic;
using System.Net;
using WebAPIService.THQ;
using WebAPIService.RCHOME;
using WatsonWebserver.Core;
using WebAPIService.HOMELEADERBOARDS;

namespace ApacheNet.RouteHandlers
{
    public class Main
    {
        public static List<Route> index = new() {
                new() {
                    Name = "Ubisoft MasterAdServerInitXml",
                    UrlRegex = "/MasterAdServerWS/MasterAdServerWS.asmx/InitXml",
                    Method = "POST",
                    Host = "master10.doublefusion.com",
                    Callable = (HttpContextBase ctx) => {
                        ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return ctx.Response.Send().Result;
                     }
                },
                new() {
                    Name = "Ubisoft GetOnlineConfig (including PSN)",
                    UrlRegex = "/OnlineConfigService.svc/GetOnlineConfig",
                    Method = "GET",
                    Host = "onlineconfigservice.ubi.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "application/json; charset=utf-8";
                            return ctx.Response.Send(WebAPIService.UBISOFT.OnlineConfigService.JsonData.GetOnlineConfigPSN(ctx.Request.RetrieveQueryValue("onlineConfigID"))).Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "Ubisoft MatchMakingConfig.aspx",
                    UrlRegex = "/MatchMakingConfig.aspx",
                    Method = "GET",
                    Host = "gconnect.ubi.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            string action = ctx.Request.RetrieveQueryValue("action");
                            string gid = ctx.Request.RetrieveQueryValue("gid");
                            string locale = ctx.Request.RetrieveQueryValue("locale");
                            string format = ctx.Request.RetrieveQueryValue("format");

                            if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(gid) && !string.IsNullOrEmpty(locale) && !string.IsNullOrEmpty(format))
                            {
                               switch (action)
                                {
                                    case "g_mmc":
                                        switch (gid)
                                        {
                                            case "e330746d922f44e3b7c2c6e5637f2e53": // DFSPS3
                                            case "20a6ed08781847c48e4cbc4dde73fd33": // DFSPS3
                                                switch (locale)
                                                {
                                                    default:
                                                        if (format == "xml")
                                                        {
                                                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                                            ctx.Response.ContentType = "text/html; charset=utf-8"; // Not an error, packet shows this content type...
                                                            return ctx.Response.Send(WebAPIService.UBISOFT.MatchMakingConfig.XMLData.DFS_PS3_NTSC_EN_XMLPayload).Result;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case "885642bfde8842b79bbcf2c1f8102403": // DFSPC
                                                switch (locale)
                                                {
                                                    default:
                                                        if (format == "xml")
                                                        {
                                                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                                            ctx.Response.ContentType = "text/html; charset=utf-8"; // Not an error, packet shows this content type...
                                                            return ctx.Response.Send(WebAPIService.UBISOFT.MatchMakingConfig.XMLData.DFS_PC_EN_XMLPayload).Result;
                                                        }
                                                        break;
                                                }
                                                break;
                                        }
                                        break;
                                }
                            }

                            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return ctx.Response.Send().Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "UFC Undisputed PS Home",
                    UrlRegex = "/index.php",
                    Method = "POST",
                    Host = "sonyhome.thqsandbox.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            string? UFCResult = UFC2010PsHomeClass.ProcessUFCUserData(ctx.Request.DataAsBytes, HTTPProcessor.ExtractBoundary(ctx.Request.ContentType), ApacheNetServerConfiguration.APIStaticFolder);
                            if (!string.IsNullOrEmpty(UFCResult))
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentType = "text/xml";
                                return ctx.Response.Send(UFCResult).Result;
                            }

                            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return ctx.Response.Send().Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "Home Firing Range leaderboard system",
                    UrlRegex = "/rchome/leaderboard.py/",
                    Method = "POST",
                    Host = null,
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            string? RCHOMEResult = new RCHOMEClass(ctx.Request.Method.ToString(), ctx.Request.Url.RawWithoutQuery, ApacheNetServerConfiguration.APIStaticFolder).ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                            if (!string.IsNullOrEmpty(RCHOMEResult))
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentType = "text/xml";
                                return ctx.Response.Send(RCHOMEResult).Result;
                            }

                            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return ctx.Response.Send().Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "Home Athletic games leaderboard system",
                    UrlRegex = "/entryBare.php",
                    Method = "POST",
                    Host = "homeleaderboards.software.eu.playstation.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            string? EntryBareResult = HOMELEADERBOARDSClass.ProcessEntryBare(ctx.Request.DataAsBytes, HTTPProcessor.ExtractBoundary(ctx.Request.ContentType), ApacheNetServerConfiguration.APIStaticFolder);
                            if (!string.IsNullOrEmpty(EntryBareResult))
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentType = "text/xml";
                                return ctx.Response.Send(EntryBareResult).Result;
                            }

                            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return ctx.Response.Send().Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "aurora_stats",
                    UrlRegex = "/scenes/aurora_stats.xml",
                    Method = "GET",
                    Host = "ndreams.stats.s3.amazonaws.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            return ctx.Response.Send("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                "<STATS>\n\t" +
                                "<TRACKING active=\"false\"/>\n\t" +
                                "<PURCHASE active=\"false\"/>\n\n\t" +
                                "<VISIT active=\"true\">\n\t\t" +
                                "<URL>http://pshome.ndreams.net/aurora/visit.php</URL>\n\t" +
                                "</VISIT>\n" +
                                "</STATS>").Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "aurora_mystery",
                    UrlRegex = "/scenes/mystery.xml",
                    Method = "GET",
                    Host = "ndreams.stats.s3.amazonaws.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            return ctx.Response.Send("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                "<mystery>\r\n\t" +
                                "<https url=\"http://pshome.ndreams.net/aurora/MysteryItems/mystery3.php\"/>\r\n" +
                                "</mystery>").Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "aurora_stats_config",
                    UrlRegex = "/aurora/aurora_stats_config.xml",
                    Method = "GET",
                    Host = "ndreams.stats.s3.amazonaws.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            return ctx.Response.Send("<stats>\n" +
                                "<pstats id=\"general\" data=\"ndreams.stats.s3.amazonaws.com/aurora\" target=\"pshome.ndreams.net/aurora\" fallover=\"pshome.ndreams.net/aurora\"/>\n" +
                                "</stats>").Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "xi2_stats_config",
                    UrlRegex = "/xi2/xi2_stats_config.xml",
                    Method = "GET",
                    Host = "ndreams.stats.s3.amazonaws.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            return ctx.Response.Send("<stats>\n\t" +
                                    "<pstats id=\"xi2\" data=\"ndreams.stats.s3.amazonaws.com/xi2\" target=\"pshome.ndreams.net/xi2/cont\" fallover=\"pshome.ndreams.net/xi2/cont\"/>\n\t" +
                                    "<pstats id=\"general\" data=\"ndreams.stats.s3.amazonaws.com/aurora\" target=\"pshome.ndreams.net/aurora\" fallover=\"pshome.ndreams.net/aurora\"/>\n" +
                                    "</stats>").Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "ansda_stats",
                    UrlRegex = "/ndreams.stats/scenes/ansda_stats.xml",
                    Method = "GET",
                    Host = "s3.amazonaws.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            return ctx.Response.Send(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                <STATS>
	                                <TRACKING active=""false"">
		                                <TIMING>10</TIMING>
		                                <SIZE>30</SIZE>
		                                <URL>http://pshome.ndreams.net/legacy/ansada/track.php</URL>
	                                </TRACKING>
	                                <PURCHASE active=""false"">
		                                <ITEMS url=""https://s3.amazonaws.com/ndreams.stats/scenes/ansdaobjs.txt"">
		                                </ITEMS>
		                                <ZONES>
			                                <ZONE x=""-15.21987"" y=""4.02206"" z=""-2.13493"" radius = ""3""/>
		                                </ZONES>
		                                <URL>http://pshome.ndreams.net/legacy/ansada/purchase.php</URL>
	                                </PURCHASE>
	                                <VISIT active=""false"">
		                                <URL>http://pshome.ndreams.net/legacy/ansada/visit.php</URL>
	                                </VISIT>
                                </STATS>").Result;
                        }
                        return false;
                     }
                },
                new() {
                    Name = "ndreams objs",
                    UrlRegex = "objs.txt",
                    Method = "GET",
                    Host = "s3.amazonaws.com",
                    Callable = (HttpContextBase ctx) => {
                        if (ApacheNetServerConfiguration.EnableBuiltInPlugins)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/plain";
                            return ctx.Response.Send(HTTPProcessor.RequestURLGET($"https://s3.amazonaws.com{ctx.Request.Url.RawWithQuery}")).Result;
                        }
                        return false;
                     }
                }
            };
    }
}
