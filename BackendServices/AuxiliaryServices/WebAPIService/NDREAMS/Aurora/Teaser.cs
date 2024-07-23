using CyberBackendLibrary.HTTP;
using HttpMultipartParser;
using System.IO;
using System;
using CastleLibrary.Utils.Hash;

namespace WebAPIService.NDREAMS.Aurora
{
    public static class Teaser
    {
        public static string? ProcessBeans(byte[]? PostData, string? ContentType)
        {
            string func = string.Empty;
            string key = string.Empty;
            string territory = string.Empty;
            string day = string.Empty;
            string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    func = data.GetParameterValue("func");
                    key = data.GetParameterValue("key");
                    territory = data.GetParameterValue("territory");
                    day = data.GetParameterValue("day");

                    ms.Flush();
                }

                string ExpectedHash = Xoff_VerifyKey(territory, day);

                if (key == ExpectedHash)
                {
                    // Get the current day of the week
                    int MockedDay = DateTime.Today.DayOfWeek switch
                    {
                        DayOfWeek.Monday => 5,
                        DayOfWeek.Tuesday => 4,
                        DayOfWeek.Wednesday => 3,
                        DayOfWeek.Thursday => 2,
                        DayOfWeek.Friday => 1,
                        _ => 0,// Default to 5 for all other cases
                    };

                    return $"<xml><success>true</success><result><day>{MockedDay}</day><hash>{Xoff_GetSignature(int.Parse(day), MockedDay)}</hash></result></xml>";
                }
                else
                {
                    string errMsg = $"[nDreams] - Teaser: invalid key sent! Received:{key} Expected:{ExpectedHash}";
                    CustomLogger.LoggerAccessor.LogWarn(errMsg);
                    return $"<xml><success>false</success><error>Signature Mismatch</error><extra>{errMsg}</extra><function>ProcessBeans</function></xml>";
                }
            }

            return null;
        }

        public static string Xoff_VerifyKey(string playerregion, string day)
        {
            return NetHasher.ComputeSHA1StringWithCleanup("xoff" + playerregion + day + "done!").ToLower();
        }

        public static string Xoff_GetSignature(int day, int ResultDay)
        {
            return NetHasher.ComputeSHA1StringWithCleanup(string.Format("Yum!Salted{0}", (day + 3) * 1239 - day * 6 + day) + ResultDay).ToLower();
        }
    }
}
