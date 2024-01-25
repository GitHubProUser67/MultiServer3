using System.Text.RegularExpressions;
using BackendProject.MiscUtils;

namespace HTTPServer.API.JUGGERNAUT.farm.furniture
{
    public class furniture_up
    {
        public static string? ProcessUp(byte[]? PostData, string? ContentType)
        {
            string file = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = HTTPUtils.ExtractAndSortUrlEncodedPOSTData(PostData);
                file = data["file"];

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

                                Directory.CreateDirectory($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{userContent}");

                                File.WriteAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{userContent}/{slotIDInt}.xml", file);

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
