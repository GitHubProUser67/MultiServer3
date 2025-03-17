using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NetworkLibrary.HTTP;

namespace WebAPIService.JUGGERNAUT.farm.furniture
{
    public class furniture_up
    {
        public static string ProcessUp(byte[] PostData, string ContentType, string apiPath)
        {
            string file = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);
                file = data["file"].First();

                if (!string.IsNullOrEmpty(file))
                {
                    // Match the pattern in the input string
                    Match match = Regex.Match(file, @"<slotID>(\d+\.\d+)<\/slotID>");

                    // Check if the pattern is found
                    if (match.Success)
                    {
                        // Convert the string to float
                        if (float.TryParse(match.Groups[1].Value.Replace(".", ","), out float slotIDFloat))
                        {
                            // Convert float to int and remove trailing zeros
                            int slotIDInt = (int)slotIDFloat;

                            // Match the pattern in the input string
                            match = Regex.Match(file, @"<user>(.*?)<\/user>");

                            // Check if the pattern is found
                            if (match.Success)
                            {
                                // Extract the matched value
                                string userContent = match.Groups[1].Value;

                                Directory.CreateDirectory($"{apiPath}/juggernaut/farm/User_Data/{userContent}");

                                File.WriteAllText($"{apiPath}/juggernaut/farm/User_Data/{userContent}/{slotIDInt}.xml", file);

                                return string.Empty;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
