using System.IO;
using System.Collections.Generic;
namespace WebAPIService.JUGGERNAUT.clearasil
{
    public class joinedspace
    {
        public static string? ProcessJoinedSpace(Dictionary<string, string>? QueryParameters, string apiPath)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];

                if (!string.IsNullOrEmpty(user))
                {
                    Directory.CreateDirectory($"{apiPath}/juggernaut/clearasil/space_access");

                    if (File.Exists($"{apiPath}/juggernaut/clearasil/space_access/{user}.xml"))
                        return File.ReadAllText($"{apiPath}/juggernaut/clearasil/space_access/{user}.xml");
                    else
                    {
                        string XmlData = "<xml><seconds>500</seconds><phase2>0</phase2><score>0</score></xml>";
                        File.WriteAllText($"{apiPath}/juggernaut/clearasil/space_access/{user}.xml", XmlData);
                        return XmlData;
                    }
                }
            }

            return null;
        }
    }
}
