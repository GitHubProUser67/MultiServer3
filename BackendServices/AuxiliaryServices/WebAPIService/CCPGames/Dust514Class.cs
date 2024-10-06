using CustomLogger;
using NetworkLibrary.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebAPIService.CCPGames
{
    public class Dust514Class : IDisposable
    {
        private string workpath;
        private string absolutepath;
        private string method;
        private bool disposedValue;

        public Dust514Class(string method, string absolutepath, string workpath)
        {
            this.absolutepath = absolutepath;
            this.workpath = workpath;
            this.method = method;
        }

        public string ProcessRequest(byte[] PostData, string ContentType, List<KeyValuePair<string, string>> headers, bool https)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            //string GlobalState.charaId = string.Empty;
            //bool isLoggedIn = false;

            switch (method)
            {
                case "GET":
                    switch (absolutepath)
                    {
                        case "/dust/server/":
                            return @"{""serverVersion"":""1.0.0.7"",
                                    ""crestEndpoint"":{
                                    ""href"":""http://dust514.online:26004/crest/""},
                                    ""sentryDSN"":"""",
                                    ""edenTime"":"""",
                                    ""userCount"":0,
                                    ""authEndpoint"":{
                                    ""username"":""http://dust514.online:26004/dust/auth/""},
                                    ""status"":""OPEN""}";
                        case "/crest/":

                            if(GlobalState.isLoggedIn == true) {
                                return @"{""character"":{""name"":""Wrecker 39"",""href"":""http://dust514.online:26004/character/2100000000/"",""id"":2100000000,""contacts"":{""href"":""""},""private"":{""href"":""""},""mercenary"":{""href"":""""},""corporation"":{""href"":"""",""id"":0,""name"":""""},""corporationMember"":{""href"":""""},""mail"":{""href"":""""},""description"":"""",""gender"":false,""race"":{""id"":0,""name"":"""",""icon"":{""href"":""""},""description"":"""",""href"":""""},""bloodLine"":{""href"":"""",""id"":0},""portrait"":{""id"":0},""skills"":{""href"":""""},""notifications"":{""href"":""""},""channels"":{""href"":""""},""npeSettings"":{""href"":""""}},""marketGroups"":{""href"":""http://dust514.online:26004/market/groups/350001/""}}";
                            } else
                            {
                                return @"{""user"":
                                    {""href"":""http://dust514.online:26004/user/""},
                                    ""pathToGame"":{""href"":""http://dust514.online:26004/document_map/""},
                                    ""contentStreaming"":{""href"":""http://dust514.online:26004/content_streaming/manifest/""},
                                    ""races"":{""href"":""http://dust514.online:26004/static_data/races/""},
                                    ""bloodlines"":{""href"":""http://dust514.online:26004/static_data/bloodlines/""},
                                    ""specialties"":{""href"":""http://dust514.online:26004/static_data/specialties/""},
                                    ""portraits"":{""href"":""http://dust514.online:26004/static_data/portraits/""},
                                    ""mercenaries"":{""href"":""http://dust514.online:26004/mercenaries/""},
                                    ""battleQueues"":{""href"":""http://dust514.online:26004/battle/queues/""},
                                    ""battles"":{""href"":""http://dust514.online:26004/battle/battles/""},
                                    ""squads"":{""href"":""http://dust514.online:26004/squads/""}}";
                            }
                        case "/user/":
                            return @"{""notifications"":{""href"":""http://dust514.online:26004/notifications/""},
                                    ""mercenaries"":{""href"":""http://dust514.online:26004/mercenaries/""}}";
                        case "/document_map/":
                            return @"{""ageconfirmation"":{""href"":""http://dust514.online:26004/document_map/ageconfirmation/""},
                                    ""eula"":{""href"":""http://dust514.online:26004/document_map/eula/""},
                                    ""nda"":{""href"":""http://dust514.online:26004/document_map/nda/""},
                                    ""servernotavailable"":{""href"":""http://dust514.online:26004/document_map/servernotavailable/""},
                                    ""patchnotes"":{""href"":""http://dust514.online:26004/document_map/patchnotes/""}}";
                        case "/document_map/eula/":
                            return @"{""text"":""Fuck a EULA""}";
                        case "/document_map/patchnotes/":
                            return @"{""text"":""Patch notes not available""}";
                        case "/document_map/ageconfirmation/":
                            //return @"{""title"":""dcrest error"",""message"":""not found"",""type"":""dcrest: 404""}";
                            return @"{""text"":""Age Confirmation Not Implemented""}";
                        case "/content_streaming/manifest/":
                            return @"{""live"":[{""bundle"":""bundles/localbundle"",""p"":0}]}";
                        case "/bundles/localbundle":
                            return @"[{""url"":""HQMYB6TYSRH44WKXB4MOQ6ITACANZSIZ.PSARC"",""platform"":""PS3-Shipping"",""size"":300917324},
                                    {""url"":""MAJHNCJMZ6GWSFFDIK6NX7YFSXSLFIXE.PSARC"",""platform"":""PS3-Shipping"",""size"":138966717},
                                    {""url"":""OCIHWOESL5RHX6W3UK422BWSYJ7ZY7MK.PSARC"",""platform"":""PS3-Shipping"",""size"":974385094},
                                    {""url"":""OO26UJNUQFD4V3HM3P64AJJXBGPBXU5U.PSARC"",""platform"":""PS3-Shipping"",""size"":13450241}]";
                        case "/mercenaries/":
                            return @"{""items"":[{""favoriteFitting"":"""",""bloodline"":{""href"":""http://dust514.online:26004/bloodline/2/"",""id"":2},""gender"":1,""self"":"""",""skillPoints"":500000,""private"":{""href"":""http://dust514.online:26004/character/2100000000/mercenary/private/""},""corporation"":{""name"":""A Corporation"",""memberFor"":""Member Forever""},""character"":{""name"":""Wrecker 39"",""href"":""http://dust514.online:26004/character/2100000000/"",""id"":2100000000,""contacts"":{""href"":""http://dust514.online:26004/character/2100000000/contacts/""},""private"":{""href"":""http://dust514.online:26004/character/2100000000/private/""},""mercenary"":{""href"":""http://dust514.online:26004/mercenary/2100000000/""},""corporation"":{""href"":""http://dust514.online:26004/corporation/1/"",""id"":1,""name"":""""},""corporationMember"":{""href"":""http://dust514.online:26004/corporation/1/member/2100000000""},""mail"":{""href"":""http://dust514.online:26004/character/2100000000/mail/""},""description"":""fuckoff"",""gender"":true,""race"":{""id"":1,""name"":""Caldari"",""icon"":{""href"":""http://dust514.online:26004/race/1/icon/""},""description"":""Founded on the tenets of patriotism and hard work that carried its ancestors through hardships on an inhospitable homeworld, the Caldari State is today a corporate dictatorship, led by rulers who are determined to see it return to the meritocratic ideals of old. Ruthless and efficient in the boardroom as well as on the battlefield, the Caldari are living emblems of strength, persistence, and dignity."",""href"":""http://dust514.online:26004/race/1/""},""bloodLine"":{""href"":"""",""id"":0},""portrait"":{""id"":0},""skills"":{""href"":""""},""notifications"":{""href"":""http://dust514.online:26004/character/2100000000/notifications/""},""channels"":{""href"":""http://dust514.online:26004/character/2100000000/channels/""},""npeSettings"":{""href"":""http://dust514.online:26004/character/2100000000/mercenary/npe_settings/""}},""portrait"":{""id"":1},""wealth"":{""isk"":250000},""passiveSkillGain"":{""status"":false,""self"":{""href"":""http://dust514.online:26004""}},""npeState"":1}]}";
                        case "/static_data/races/":
                            return @"{""items"":[{""name"":""Caldari"",""description"":""Founded on the tenets of patriotism and hard work that carried its ancestors through hardships on an inhospitable homeworld, the Caldari State is today a corporate dictatorship, led by rulers who are determined to see it return to the meritocratic ideals of old. Ruthless and efficient in the boardroom as well as on the battlefield, the Caldari are living emblems of strength, persistence, and dignity."",""href"":""https://dust514.online/static_data/race/1/"",""icon"":{""href"":""icon_race_caldari""}},{""name"":""Minmatar"",""description"":""Once a thriving tribal civilization, the Minmatar were enslaved by the Amarr Empire for more than 700 years until a massive rebellion freed most, but not all, of those held in servitude. The Minmatar people today are resilient, ingenious, and hard-working. Many of them believe that democracy, though it has served them well for a long time, can never restore what was taken from them so long ago. For this reason they have formed a government truly reflective of their tribal roots. They will forever resent the Amarrians, and yearn for the days before the Empire"",""href"":""https://dust514.online/static_data/race/2/"",""icon"":{""href"":""icon_race_minmatar""}},{""name"":""Amarr"",""description"":""The Amarr Empire is the largest and oldest of the four empires. Ruled by a mighty God-Emperor, this vast theocratic society is supported by a broad foundation of slave labor. Amarrian citizens tend to be highly educated and fervent individuals, and as a culture Amarr adheres to the basic tenet that what others call slavery is in fact one step on a indentured person’s spiritual path toward fully embracing their faith. Despite several setbacks in recent history, the Empire remains arguably the most stable and militarily powerful nation-state in New Eden"",""href"":""https://dust514.online/static_data/race/4/"",""icon"":{""href"":""icon_race_amarr""}},{""name"":""Gallente"",""description"":""Champions of liberty and defenders of the downtrodden, the Gallente play host to the only true democracy in New Eden. Some of the most progressive leaders, scientists, and businessmen of the era have emerged from its diverse peoples. A pioneer of artificial intelligence, the Federation relies heavily on drones and other automated systems. This is not to detract from the skill of their pilots, though: the Gallente Federation is known for producing some of the best and bravest the universe has to offer."",""href"":""https://dust514.online/static_data/race/8/"",""icon"":{""href"":""icon_race_gallente""}}]}";

                        default:    
                            break;
                    }
                    break;
                case "PUT":
                    switch (absolutepath)
                    {
                        case "/":
                            return @"{""configuration"":{""href"":""http://dust514.online:26004/cfg/""}}";
                            
                    }
                    break;
                case "POST":
                    switch (absolutepath)
                    {
                        case "/dust/auth/":
                            
                            string pattern = @"character=(\d+)";
                            string authData = Encoding.UTF8.GetString(PostData);
                            Match match = Regex.Match(authData, pattern);

                            if (match.Success)
                            {
                                GlobalState.charaId = match.Groups[1].Value;
                                LoggerAccessor.LogWarn("Character ID: " + GlobalState.charaId);
                                GlobalState.isLoggedIn = true;
                            }
                            else
                            {
                                LoggerAccessor.LogWarn("Character ID not found.");
                                GlobalState.isLoggedIn = false;
                            }

                            return "{\"access_token\":\"8d00eee7-0405-5b40-de54-218d79cc17ce\",\"refresh_token\":\"8d00eee7-0405-5b40-de54-218d79cc17ce\",\"token_type\":\"dust\"}";
                    }
                    break;
                default:
                    break;
            }

            if (absolutepath.Equals($"character/{GlobalState.charaId}/private/"))
            {
                return $"{{\"location\":{{\"href\":\"http://dust514.online:26004/location/60014943/\"}},\"fittings\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mercenary/fittings/\"}}";
            }
            else if (absolutepath.Equals($"character/{GlobalState.charaId}/fulfillments/"))
            {
                return $"{{\"items\":\"\"}}";
            }
            else if (absolutepath.Equals($"character/{GlobalState.charaId}/mercenary/"))
            {
                return $"{{\"name\":\"Wrecker 39\",\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/\",\"id\":{GlobalState.charaId},\"contacts\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/contacts/\"}},\"private\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/private/\"}},\"mercenary\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mercenary/\"}},\"corporation\":{{\"href\":\"http://dust514.online:26004/corporation/1000134/\",\"id\":1,\"name\":\"A Corporation\"}},\"corporationMember\":{{\"href\":\"http://dust514.online:26004/corporation/1/member/{GlobalState.charaId}/\"}},\"mail\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mail/\"}},\"description\":\"\",\"gender\":true,\"race\":{{\"id\":1,\"name\":\"\",\"icon\":{{\"href\":\"http://dust514.online:26004/race/1/icon/\"}},\"description\":\"\",\"href\":\"http://dust514.online:26004/race/1/\"}},\"bloodLine\":{{\"href\":\"http://dust514.online:26004/bloodline/2\",\"id\":2}},\"portrait\":{{\"id\":1}},\"skills\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mercenary/skills/\"}},\"notifications\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/notifications/\"}},\"channels\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/channels/\"}},\"npeSettings\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mercenary/npe_settings/\"}}";
            }
            else if (absolutepath.Equals($"character/{GlobalState.charaId}/"))
            {

                //Get Player character
                return $"{{\"name\":\"Wrecker 39\",\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/\",\"id\":{GlobalState.charaId}," +
                    $"\"contacts\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/contacts/\"}}," +
                    $"\"private\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/private/\"}}," +
                    $"\"mercenary\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mercenary/\"}}," +
                    $"\"corporation\":{{\"href\":\"http://dust514.online:26004/corporation/1000134/\",\"id\":1,\"name\":\"A Corporation\"}}," +
                    $"\"corporationMember\":{{\"href\":\"http://dust514.online:26004/corporation/1/member/{GlobalState.charaId}/\"}}," +
                    $"\"mail\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mail/\"}},\"description\":\"\",\"gender\":true," +
                    $"\"race\":{{\"id\":1,\"name\":\"\"," +
                    $"\"icon\":{{\"href\":\"http://dust514.online:26004/race/1/icon/\"}},\"description\":\"\"," +
                    $"\"href\":\"http://dust514.online:26004/race/1/\"}}," +
                    $"\"bloodLine\":{{\"href\":\"http://dust514.online:26004/bloodline/2\",\"id\":2}}," +
                    $"\"portrait\":{{\"id\":1}},\"skills\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mercenary/skills/\"}}," +
                    $"\"notifications\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/notifications/\"}}," +
                    $"\"channels\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/channels/\"}}," +
                    $"\"npeSettings\":{{\"href\":\"http://dust514.online:26004/character/{GlobalState.charaId}/mercenary/npe_settings/\"}}";
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    absolutepath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~HELLFIREClass()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public static class GlobalState
    {
        public static string charaId { get; set; }
        public static bool isLoggedIn { get; set; } = false;
    }
}
