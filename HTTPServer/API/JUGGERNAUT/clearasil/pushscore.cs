using System.Xml;

namespace HTTPServer.API.JUGGERNAUT.clearasil
{
    public class pushscore
    {
        public static string? ProcessPushScore(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];
                string? score = QueryParameters["score"];

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(score))
                {
                    try
                    {
                        ScoreBoardData.UpdateScore(user, int.Parse(score));
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }

                    Directory.CreateDirectory($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/clearasil/space_access");

                    if (File.Exists($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/clearasil/space_access/{user}.xml"))
                    {
                        // Load the XML string into an XmlDocument
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/clearasil/space_access/{user}.xml");

                        // Find the <score> element
                        XmlElement? scoreElement = xmlDoc.SelectSingleNode("/xml/score") as XmlElement;

                        if (scoreElement != null)
                        {
                            try
                            {
                                int increment = int.Parse(score);
                                int existingscore = int.Parse(scoreElement.InnerText);
                                // Replace the value of <score> with a new value
                                scoreElement.InnerText = (existingscore + increment).ToString();
                            }
                            catch (Exception)
                            {
                                scoreElement.InnerText = score;
                            }

                            File.WriteAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/clearasil/space_access/{user}.xml", xmlDoc.OuterXml);
                        }
                    }

                    return string.Empty;
                }
            }

            return null;
        }
    }
}
