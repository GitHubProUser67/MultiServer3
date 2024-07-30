using HttpMultipartParser;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WebAPIService.HELLFIRE.Helpers
{
    public class BuildingsProcessor
    {
        public static string CreateBuilding(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string Orientation = string.Empty;
            string Type = string.Empty;
            string TownID = string.Empty;
            string Index = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Orientation = data.GetParameterValue("Orientation");
                    Type = data.GetParameterValue("Type");
                    TownID = data.GetParameterValue("TownID");
                    Index = data.GetParameterValue("Index");
                    ms.Flush();
                }

                DateTime CurrentTime = DateTime.Now;

                if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                    File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml",
                    Regex.Replace(File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"),
                    $"<{Index:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){Index:F6}(</Index>)(<Type>).*?(</Type>)</{Index:F6}>",
                    $"<{Index:F6}><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></{Index:F6}>"));

                return $"<Response><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></Response>";
            }

            return "<Response></Response>";
        }

        public static string RemoveBuilding(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string xmlprofile = string.Empty;
            string TownID = string.Empty;
            string BuildingIndex = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    TownID = data.GetParameterValue("TownID");
                    BuildingIndex = data.GetParameterValue("BuildingIndex");
                    ms.Flush();
                }

                if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                    File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml",
                    Regex.Replace(File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"),
                    $"<{BuildingIndex:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){BuildingIndex:F6}(</Index>)(<Type>).*?(</Type>)</{BuildingIndex:F6}>",
                    $"<{BuildingIndex:F6}><TimeBuilt></TimeBuilt><Orientation></Orientation><Index>{BuildingIndex}</Index><Type></Type></{BuildingIndex:F6}>"));
            }

            return "<Response></Response>";
        }
    }
}
