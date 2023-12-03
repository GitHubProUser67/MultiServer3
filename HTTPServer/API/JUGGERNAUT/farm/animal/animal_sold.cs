using System.Xml.Linq;
using System.Xml;

namespace HTTPServer.API.JUGGERNAUT.farm.animal
{
    public class animal_sold
    {
        public static string? ProcessSold(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];
                string? type = QueryParameters["type"];
                string? id = QueryParameters["id"];
                string? amount = QueryParameters["amount"];

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(amount))
                {
                    Directory.CreateDirectory($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data");

                    if (File.Exists($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml"))
                    {
                        // Load the XML string into an XmlDocument
                        XmlDocument xmlDoc = new();
                        xmlDoc.LoadXml(RemoveAnimalEntry(File.ReadAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml"), type, id));

                        // Find the <gold> element
                        XmlElement? goldElement = xmlDoc.SelectSingleNode("/xml/resources/gold") as XmlElement;

                        if (goldElement != null)
                        {
                            try
                            {
                                // Replace the value of <gold> with a new value
                                goldElement.InnerText = (int.Parse(goldElement.InnerText) + int.Parse(amount)).ToString();
                            }
                            catch (Exception)
                            {
                                // Not Important
                            }

                            File.WriteAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml", xmlDoc.OuterXml);
                        }
                    }

                    return string.Empty;
                }
            }

            return null;
        }

        private static string RemoveAnimalEntry(string xmlData, string type, string id)
        {
            XDocument xdoc = XDocument.Parse(xmlData);

            XElement? animalToRemove = xdoc.Descendants("animal")
                .FirstOrDefault(a =>
                    a.Element("t")?.Value == type &&
                    a.Element("id")?.Value == id
                );

            if (animalToRemove != null)
                animalToRemove.Remove();

            return xdoc.ToString();
        }
    }
}
