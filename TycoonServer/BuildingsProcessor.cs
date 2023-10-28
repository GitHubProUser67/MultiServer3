using HttpMultipartParser;
using System.Text.RegularExpressions;

namespace TycoonServer
{
    public class BuildingsProcessor
    {
        public static string? CreateBuilding(byte[]? PostData, string boundary, string UserID)
        {
            string xmlprofile = string.Empty;
            string Orientation = string.Empty;
            string Type = string.Empty;
            string TownID = string.Empty;
            string Index = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Orientation = data.GetParameterValue("Orientation");
                    Type = data.GetParameterValue("Type");
                    TownID = data.GetParameterValue("TownID");
                    Index = data.GetParameterValue("Index");
                    ms.Flush();
                }

                DateTime CurrentTime = DateTime.Now;

                if (File.Exists($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                {
                    xmlprofile = File.ReadAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_{TownID}.xml");

                    string pattern = $"<{Index:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){Index:F6}(</Index>)(<Type>).*?(</Type>)</{Index:F6}>";
                    string replacement = $"<{Index:F6}><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></{Index:F6}>";

                    File.WriteAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_{TownID}.xml", Regex.Replace(xmlprofile, pattern, replacement));
                }

                return $"<Response><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></Response>";
            }

            return "<Response></Response>";
        }

        public static string? RemoveBuilding(byte[]? PostData, string boundary, string UserID)
        {
            string xmlprofile = string.Empty;
            string TownID = string.Empty;
            string BuildingIndex = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    TownID = data.GetParameterValue("TownID");
                    BuildingIndex = data.GetParameterValue("BuildingIndex");
                    ms.Flush();
                }

                if (File.Exists($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                {
                    xmlprofile = File.ReadAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_{TownID}.xml");

                    string pattern = $"<{BuildingIndex:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){BuildingIndex:F6}(</Index>)(<Type>).*?(</Type>)</{BuildingIndex:F6}>";
                    string replacement = $"<{BuildingIndex:F6}><TimeBuilt></TimeBuilt><Orientation></Orientation><Index>{BuildingIndex}</Index><Type></Type></{BuildingIndex:F6}>";

                    File.WriteAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_{TownID}.xml", Regex.Replace(xmlprofile, pattern, replacement));
                }
            }

            return "<Response></Response>";
        }
    }
}
