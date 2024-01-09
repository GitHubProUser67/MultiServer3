using HTTPServer.API.UBISOFT.Models;
using Newtonsoft.Json;

namespace HTTPServer.API.UBISOFT.OnlineConfigService
{
    public static class JsonData
    {
        private static Dictionary<string, string> DriverSanFransiscoPCResponse = new()
        {
            { "SandboxUrl",                     @"prudp:/address=pdc-lb-rdv-prod01.ubisoft.com;port=60105;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv01.ubisoft.com:60105"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };

        private static Dictionary<string, string> Hawx2PS3Response = new()
        {
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod02.ubisoft.com;port=60115;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv02.ubisoft.com:60115"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };

        private static Dictionary<string, string> DriverSanFransiscoPS3Response = new()
        {
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod03.ubisoft.com;port=61110;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv03.ubisoft.com:61110"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };

        private static Dictionary<string, string> GhostReconFutureSoliderPS3Response = new()
        {
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod04.ubisoft.com;port=61120;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv04.ubisoft.com:61120"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };


        private static Dictionary<string, string> AC3PS3Response = new()
        {
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod05.ubisoft.com;port=61125;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv05.ubisoft.com:61125"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };

        private static Dictionary<string, string> AC3MULTPS3Response = new()
        {
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod06.ubisoft.com;port=61127;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv06.ubisoft.com:61127"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };

        public static string GetOnlineConfigPSN(string? onlineConfigID)
        {
            var list = new List<OnlineConfigEntry>();

            switch (onlineConfigID)
            {
                case "f70cace9c14a4656bccb03a661dbe660":
                    foreach (var v in AC3MULTPS3Response)
                    {
                        list.Add(new OnlineConfigEntry
                        {
                            Name = v.Key,
                            Values = new[] { v.Value }
                        });
                    }
                    break;
                case "90a8c600b48142629f1e102133e18398":
                    foreach (var v in AC3PS3Response)
                    {
                        list.Add(new OnlineConfigEntry
                        {
                            Name = v.Key,
                            Values = new[] { v.Value }
                        });
                    }
                    break;
                case "36e80e4e6db64f04877c928211c8bb71":
                    foreach (var v in Hawx2PS3Response)
                    {
                        list.Add(new OnlineConfigEntry
                        {
                            Name = v.Key,
                            Values = new[] { v.Value }
                        });
                    }
                    break;
                case "e330746d922f44e3b7c2c6e5637f2e53":
                    foreach (var v in DriverSanFransiscoPS3Response)
                    {
                        list.Add(new OnlineConfigEntry
                        {
                            Name = v.Key,
                            Values = new[] { v.Value }
                        });
                    }
                    break;
                case "18a7f5db34d34b118c1a0b8eeb517fd7":
                    foreach (var v in GhostReconFutureSoliderPS3Response)
                    {
                        list.Add(new OnlineConfigEntry
                        {
                            Name = v.Key,
                            Values = new[] { v.Value }
                        });
                    }
                    break;
                default:
                    foreach (var v in DriverSanFransiscoPCResponse)
                    {
                        list.Add(new OnlineConfigEntry
                        {
                            Name = v.Key,
                            Values = new[] { v.Value }
                        });
                    }
                    break;
            }

            return JsonConvert.SerializeObject(list);
        }
    }
}
