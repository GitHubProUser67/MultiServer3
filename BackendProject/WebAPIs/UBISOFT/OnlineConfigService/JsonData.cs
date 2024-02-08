using BackendProject.WebAPIs.UBISOFT.Models;
using Newtonsoft.Json;

namespace BackendProject.WebAPIs.UBISOFT.OnlineConfigService
{
    public static class JsonData
    {
        private readonly static Dictionary<string, string> DriverSanFransiscoPCResponse = new()
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

        private readonly static Dictionary<string, string> Hawx2PS3Response = new()
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

        private readonly static Dictionary<string, string> DriverSanFransiscoPS3Response = new()
        {
            { "GatorAuthServiceActivity",       @"DRIVER5PS3"},
            { "GatorAuthServiceURI",            @"http://gatorservice.ubi.com/AuthenticationServiceSSL.svc"},
            { "GatorConfigServiceURI",          @"http://gatorservice.ubi.com/ConfigService.svc"},
            { "GatorContentServiceURI",         @"http://gatorservice.ubi.com/ContentService.svc"},
            { "GatorServiceHost",               @"gatorservice.ubi.com"},
            { "GatorServicePort",               @"80"},
            { "GatorTransferServiceURI",        @"/TransferService.svc"},
            { "GatorUserServiceURI",            @"http://gatorservice.ubi.com/GatorUserService.svc"},
            { "NetworkPlatformServiceId",       @"EP0001-BLES00891_00"},
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod03.ubisoft.com;port=61110;serviceid=UP0001-BLUS30536_00"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv03.ubisoft.com:61110"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.0.3-RC-Share/test7/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };

        private readonly static Dictionary<string, string> GhostReconFutureSoliderPS3Response = new()
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


        private readonly static Dictionary<string, string> AC3PS3Response = new()
        {
            { "GenomeId",                       @"9a36ff77-d110-45c9-9dfb-2568d326f0f4"},
            { "killSwitch_contentFiltering",    @"0"},
            { "killSwitch_friendsProposal",     @"0"},
            { "killSwitch_friendsRequest",      @"0"},
            { "killSwitch_friendsSuggestions",  @"0"},
            { "killSwitch_socialFeed",          @"0"},
            { "killSwitch_socialFeedWrites",    @"0"},
            { "killSwitch_uplay",               @"0"},
            { "SandboxUrlPrivPS3",              @"prudp:/address=pdc-lb-rdv-prod05.ubisoft.com;port=62125;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod05.ubisoft.com;port=61125;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv05.ubisoft.com:61125"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
            { "UplayGameCode",                  @"AC3"},
            { "US_AcceptSuggestion_PS3",        @"http://friendsservice.ubi.com/friendsservice.svc/REST/AcceptSuggestion "},
            { "US_CommentPost_PS3",             @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/InsertWallPostComment/"},
            { "US_DeclineSuggestion_PS3",       @"http://friendsservice.ubi.com/friendsservice.svc/REST/DeclineSuggestion "},
            { "US_GetWallPostsByIds_PS3",       @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/GetWallPostsByIds/"},
            { "US_GetWalls_PS3",                @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/GetWalls/"},
            { "US_GetWallsPost_PS3",            @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/GetWallPosts/"},
            { "US_InitializeUser_PS3",          @"http://wspuplay-ext.ubi.com/UplayServices/WinServices/GameClientServices.svc/REST/JSON/InitializeUser/"},
            { "US_InsertPost_PS3",              @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/InsertWallPost/"},
            { "US_LikePost_PS3",                @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/InsertWallPostLike/"},
            { "US_Login_PS3",                   @"http://signupservice.ubi.com/PublicOnlineUserService.svc/REST/LogIn "},
            { "US_Logout_PS3",                  @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LogOut "},
            { "US_LookupExternalId_PS3",        @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LookupExternalAccountIDsByUbiAccountIDs "},
            { "US_LookupUplayId_PS3",           @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LookupUbiAccountIDsByExternalAccountNames "},
            { "US_LookupUplayUsername_PS3",     @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LookupUsernamesByUbiAccountIDs "},
            { "US_RequestFriends_PS3",          @"http://friendsservice.ubi.com/friendsservice.svc/REST/GetRelationshipsWithPresenceByToken "},
            { "US_RequestSuggestions_PS3",      @"http://friendsservice.ubi.com/friendsservice.svc/REST/GetSuggestionsByPage "},
            { "US_SharePost_PS3",               @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/ShareWallPost/"},
            { "US_SyncFriends_PS3",             @"http://friendsservice.ubi.com/friendsservice.svc/REST/ProposeContacts "},
        };

        private readonly static Dictionary<string, string> AC3MULTPS3Response = new()
        {
            { "GenomeId",                       @"9a36ff77-d110-45c9-9dfb-2568d326f0f4"},
            { "killSwitch_contentFiltering",    @"0"},
            { "killSwitch_friendsProposal",     @"0"},
            { "killSwitch_friendsRequest",      @"0"},
            { "killSwitch_friendsSuggestions",  @"0"},
            { "killSwitch_socialFeed",          @"0"},
            { "killSwitch_socialFeedWrites",    @"0"},
            { "killSwitch_uplay",               @"0"},
            { "SandboxUrlPrivPS3",              @"prudp:/address=pdc-lb-rdv-prod06.ubisoft.com;port=62127;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod06.ubisoft.com;port=61127;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv06.ubisoft.com:61127"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
            { "UplayGameCode",                  @"AC3"},
            { "US_AcceptSuggestion_PS3",        @"http://friendsservice.ubi.com/friendsservice.svc/REST/AcceptSuggestion "},
            { "US_CommentPost_PS3",             @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/InsertWallPostComment/"},
            { "US_DeclineSuggestion_PS3",       @"http://friendsservice.ubi.com/friendsservice.svc/REST/DeclineSuggestion "},
            { "US_GetWallPostsByIds_PS3",       @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/GetWallPostsByIds/"},
            { "US_GetWalls_PS3",                @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/GetWalls/"},
            { "US_GetWallsPost_PS3",            @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/GetWallPosts/"},
            { "US_InitializeUser_PS3",          @"http://wspuplay-ext.ubi.com/UplayServices/WinServices/GameClientServices.svc/REST/JSON/InitializeUser/"},
            { "US_InsertPost_PS3",              @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/InsertWallPost/"},
            { "US_LikePost_PS3",                @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/InsertWallPostLike/"},
            { "US_Login_PS3",                   @"http://signupservice.ubi.com/PublicOnlineUserService.svc/REST/LogIn "},
            { "US_Logout_PS3",                  @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LogOut "},
            { "US_LookupExternalId_PS3",        @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LookupExternalAccountIDsByUbiAccountIDs "},
            { "US_LookupUplayId_PS3",           @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LookupUbiAccountIDsByExternalAccountNames "},
            { "US_LookupUplayUsername_PS3",     @"http://signupservice.ubi.com/PublicSignupService.svc/REST/LookupUsernamesByUbiAccountIDs "},
            { "US_RequestFriends_PS3",          @"http://friendsservice.ubi.com/friendsservice.svc/REST/GetRelationshipsWithPresenceByToken "},
            { "US_RequestSuggestions_PS3",      @"http://friendsservice.ubi.com/friendsservice.svc/REST/GetSuggestionsByPage "},
            { "US_SharePost_PS3",               @"http://wspuplay-ext.ubi.com/UplayServices/ShareServices/GameClientServices.svc/REST/JSON/ShareWallPost/"},
            { "US_SyncFriends_PS3",             @"http://friendsservice.ubi.com/friendsservice.svc/REST/ProposeContacts "},
        };

        private readonly static Dictionary<string, string> AC2PS3Response = new()
        {
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod07.ubisoft.com;port=61129;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv07.ubisoft.com:61129"},
            { "uplay_DownloadServiceUrl",       @"http://wsuplay.ubi.com/UplayServices/UplayFacade/DownloadServicesRESTXML.svc/REST/XML/?url="},
            { "uplay_DynContentBaseUrl",        @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_DynContentSecureBaseUrl",  @"http://static8.cdn.ubi.com/"},
            { "uplay_LinkappBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/linkapp/1.1/"},
            { "uplay_MovieBaseUrl",             @"http://static8.cdn.ubi.com/u/Uplay/"},
            { "uplay_PackageBaseUrl",           @"http://static8.cdn.ubi.com/u/Uplay/Packages/1.5-Share-rc/"},
            { "uplay_WebServiceBaseUrl",        @"http://wsuplay.ubi.com/UplayServices/UplayFacade/ProfileServicesFacadeRESTXML.svc/REST/"},
        };

        private readonly static Dictionary<string, string> SCBLACKLISTPS3Response = new()
        {
            { "SandboxUrlPS3",                  @"prudp:/address=pdc-lb-rdv-prod08.ubisoft.com;port=61131;serviceid=UPxxxx-MYGAME"},
            { "SandboxUrlWS",                   @"pdc-vm-rdv08.ubisoft.com:61131"},
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
            List<OnlineConfigEntry> list = new();

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
                case "749b17f318b7463cbaa4eb5bc19527e5":
                    foreach (var v in AC2PS3Response)
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
                case "acd2bb86618441a7b2af9b4b952c9612":
                    foreach (var v in SCBLACKLISTPS3Response)
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
