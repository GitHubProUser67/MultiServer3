using System.Xml.Linq;

namespace HTTPServer.API.JUGGERNAUT.farm.animal
{
    public class animal_leveled
    {
        public static string? ProcessLeveled(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];
                string? type = QueryParameters["type"];
                string? id = QueryParameters["id"];
                string? level = QueryParameters["level"];

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(level))
                {
                    Directory.CreateDirectory($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data");

                    if (File.Exists($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml"))
                        File.WriteAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml",
                                UpdateLevel(File.ReadAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml"), id, type, level));

                    return string.Empty;
                }
            }

            return null;
        }

        private static string UpdateLevel(string xmlData, string id, string type, string newLevel)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlData);

                XElement? animalToUpdate = xdoc.Descendants("animal")
                    .FirstOrDefault(a => a.Element("id")?.Value == id && a.Element("t")?.Value == type);

                if (animalToUpdate != null)
                    animalToUpdate.Element("l").Value = newLevel.ToString();

                return xdoc.ToString();
            }
            catch (Exception)
            {

            }

            return xmlData;
        }
    }
}
