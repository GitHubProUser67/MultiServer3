using CyberBackendLibrary.HTTP;
using HttpMultipartParser;

namespace WebAPIService.HELLFIRE
{
    public class Redirector
    {
        public static string? ProcessMainRedirector(byte[] PostData, string ContentType)
        {
            string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string Command = data.GetParameterValue("Command");
                    switch (Command)
                    {
                        case "VersionCheck":
                            ms.Flush();
                            return "<Response><URL>http://game2.hellfiregames.com:61900/HomeTycoon</URL></Response>";
                        default:
                            break;

                    }
                    ms.Flush();
                }
            }

            return null;
        }
    }
}
