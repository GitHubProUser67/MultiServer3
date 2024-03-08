using System.Xml;

namespace WebUtils.JUGGERNAUT.farm.furniture
{
    public class furniture_slot_saved
    {
        public static string? ProcessSlotSaved(Dictionary<string, string>? QueryParameters, string apiPath)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];
                string? slot = QueryParameters["slot"];

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(slot))
                {
                    Directory.CreateDirectory($"{apiPath}/juggernaut/farm/User_Data");

                    if (File.Exists($"{apiPath}/juggernaut/farm/User_Data/{user}.xml"))
                    {
                        // Load the XML string into an XmlDocument
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load($"{apiPath}/juggernaut/farm/User_Data/{user}.xml");

                        // Find the <lastLayout> element
                        XmlElement? lastLayoutElement = xmlDoc.SelectSingleNode("/xml/resources/lastLayout") as XmlElement;

                        if (lastLayoutElement != null)
                        {
                            // Replace the value of <lastLayout> with a new value
                            lastLayoutElement.InnerText = slot;

                            File.WriteAllText($"{apiPath}/juggernaut/farm/User_Data/{user}.xml", xmlDoc.OuterXml);
                        }
                    }

                    return string.Empty;
                }
            }

            return null;
        }
    }
}
