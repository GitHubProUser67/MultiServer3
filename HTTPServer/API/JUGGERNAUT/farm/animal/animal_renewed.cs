using System.Xml.Linq;

namespace HTTPServer.API.JUGGERNAUT.farm.animal
{
    public class animal_renewed
    {
        public static string? ProcessRenewed(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];
                string? type = QueryParameters["type"];
                string? id = QueryParameters["id"];

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(id))
                {
                    Directory.CreateDirectory($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data");

                    if (File.Exists($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{user}.xml"))
                        File.WriteAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{user}.xml",
                                UpdateTbu(File.ReadAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{user}.xml"), id, type));

                    return string.Empty;
                }
            }

            return null;
        }

        private static string UpdateTbu(string xmlData, string id, string type)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlData);

                XElement? animalToUpdate = xdoc.Descendants("animal")
                    .FirstOrDefault(a => a.Element("id")?.Value == id && a.Element("t")?.Value == type);

                if (animalToUpdate != null)
                    animalToUpdate.Element("tbu").Value = "1";

                return xdoc.ToString();
            }
            catch (Exception)
            {

            }

            return xmlData;
        }
    }
}
