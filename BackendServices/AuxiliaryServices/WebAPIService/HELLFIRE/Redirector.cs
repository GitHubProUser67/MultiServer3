using CyberBackendLibrary.HTTP;
using HttpMultipartParser;
using System.IO;

namespace WebAPIService.HELLFIRE
{
    public class Redirector
    {
        public static string? ProcessMainHomeTycoonRedirector(byte[] PostData, string ContentType)
        {
            string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string Command = data.GetParameterValue("Command");
                    switch (Command)
                    {
                        case "VersionCheck":
                            ms.Flush();
                            return "<Response><URL>http://game2.hellfiregames.com/HomeTycoon</URL></Response>";
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
