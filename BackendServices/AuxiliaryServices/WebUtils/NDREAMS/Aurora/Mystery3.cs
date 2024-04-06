using BackendProject.MiscUtils;
using HttpMultipartParser;

namespace WebUtils.NDREAMS.Aurora
{
    public class Mystery3
    {
        public static string? ProcessMystery3(byte[]? PostData, string? ContentType, string fullurl, string apipath)
        {
            string key = string.Empty;
            string func = string.Empty;
            string name = string.Empty;
            DateTime CurrentDate = DateTime.Today;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                ushort min = 0;
                ushort max = 180;
                int turns = 0;

                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    key = data.GetParameterValue("key");
                    func = data.GetParameterValue("func");
                    name = data.GetParameterValue("name");

                    ms.Flush();
                }

                // Check if today is April 4th or October 25th
                if (CurrentDate.Month == 4 && CurrentDate.Day == 4)
                {
                    // Increase the chance of getting 80
                    if (new Random().Next(0, 100) < 70)
                        turns = 80;
                    else
                        turns = new Random().Next(min, max);
                }
                else if (CurrentDate.Month == 10 && CurrentDate.Day == 25)
                {
                    // Increase the chance of getting 60
                    if (new Random().Next(0, 100) < 80)
                        turns = 60;
                    else
                        turns = new Random().Next(min, max);
                }
                else
                    // For other dates, use a uniform random distribution
                    turns = new Random().Next(min, max);

                Directory.CreateDirectory(apipath + "/NDREAMS/Aurora/Mystery3");

                string TimestampProfilePath = apipath + $"/NDREAMS/Aurora/Mystery3/{name}.txt";

                switch (func)
                {
                    case "get":
                        if (File.Exists(TimestampProfilePath) && new FileInfo(TimestampProfilePath).CreationTime.Date == DateTime.Today)
                        {
                            string timestamp = File.ReadAllText(TimestampProfilePath);
                            return $"<xml><sig>{NDREAMSServerUtils.Server_GetSignature(fullurl, name, "collect", CurrentDate)}</sig><confirm>{NDREAMSServerUtils.Server_KeyToHash(key, CurrentDate, timestamp)}</confirm><timestamp>{timestamp}</timestamp><Turns>{turns}</Turns></xml>";
                        }
                        else
                            return $"<xml><sig>{NDREAMSServerUtils.Server_GetSignature(fullurl, name, "collect", CurrentDate)}</sig><confirm>{NDREAMSServerUtils.Server_KeyToHash(key, CurrentDate, "nil")}</confirm><timestamp>nil</timestamp><Turns>{turns}</Turns></xml>";
                }
            }

            return null;
        }
    }
}
