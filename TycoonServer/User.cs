using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace TycoonServer
{
    public class User
    {
        public static string DefaultProfile = "<TotalCollected>0.000000</TotalCollected><Wallet>9999999.000000</Wallet><Workers>9999999.000000</Workers>" +
            "<GoldCoins>999999.000000</GoldCoins><SilverCoins>999999.000000</SilverCoins><Options><MusicVolume>1.0</MusicVolume></Options><Missions>" +
            "</Missions><Journal></Journal><Dialogs></Dialogs><Unlocked></Unlocked><Activities></Activities><Expansions></Expansions><Vehicles></Vehicles>" +
            "<Flags></Flags><Inventory></Inventory>";

        public static string UpdateUser(byte[] PostData, string boundary, string UserID)
        {
            string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            // Retrieve the user's JSON profile
            string profilePath = $"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultProfile;

            try
            {
                // Create an XmlDocument
                var doc = new XmlDocument();
                doc.LoadXml("<root>" + xmlProfile + "</root>"); // Wrap the XML string in a root element
                if (doc != null && PostData != null && !string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        // Update the profile values from the provided data
                        // Update the values based on IDs
                        doc.SelectSingleNode("//TotalCollected").InnerText = data.GetParameterValue("TotalCollected");
                        doc.SelectSingleNode("//Wallet").InnerText = data.GetParameterValue("Wallet");
                        doc.SelectSingleNode("//Workers").InnerText = data.GetParameterValue("Workers");
                        doc.SelectSingleNode("//GoldCoins").InnerText = data.GetParameterValue("GoldCoins");
                        doc.SelectSingleNode("//SilverCoins").InnerText = data.GetParameterValue("SilverCoins");

                        var jsonObject = JToken.Parse(data.GetParameterValue("Options"));
                        var fieldValues = jsonObject.ToObject<Dictionary<string, object>>();

                        if (fieldValues != null)
                        {
                            foreach (var fieldValue in fieldValues)
                            {
                                if (fieldValue.Key == "MusicVolume" && fieldValue.Value != null)
                                    doc.SelectSingleNode("//Options/MusicVolume").InnerText = fieldValue.Value.ToString();
                            }
                        }

                        // Get the updated XML string
                        updatedXMLProfile = doc.DocumentElement.InnerXml.Replace("<root>", "").Replace("</root>", "");

                        // Save the updated profile back to the file
                        File.WriteAllText(profilePath, updatedXMLProfile);

                        ms.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - An assertion was thrown in UpdateUser : {ex}");
            }

            return $"<Response>{updatedXMLProfile}</Response>";
        }
    }
}
