using BackendProject.MiscUtils;
using HttpMultipartParser;
using System.Text;
using System.Security.Cryptography;

namespace WebUtils.NDREAMS.Aurora
{
    public class Teaser
    {
        public static string? ProcessBeans(byte[]? PostData, string? ContentType)
        {
            string func = string.Empty;
            string key = string.Empty;
            string territory = string.Empty;
            string day = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    func = data.GetParameterValue("func");
                    key = data.GetParameterValue("key");
                    territory = data.GetParameterValue("territory");
                    day = data.GetParameterValue("day");

                    ms.Flush();
                }

                string MockedDay = "4"; // UNK what it does.

                return $"<xml><hash>{Xoff_GetSignature(day, MockedDay)}</hash><success>true</success><result><day>{MockedDay}</day></result></xml>";
            }

            return null;
        }

        public static string Xoff_VerifyKey(string playerregion, string salt)
        {
            return BitConverter.ToString(SHA1.HashData(Encoding.UTF8.GetBytes("xoff" + playerregion + string.Empty + salt + "done!"))).Replace("-", string.Empty).ToLower();
        }

        public static string Xoff_GetSignature(string day, string resultday)
        {
            if (int.TryParse(day, out int parsedDay))
                return BitConverter.ToString(SHA1.HashData(Encoding.UTF8.GetBytes(string.Format("Yum!Salted{0}", (parsedDay + 3) * 1239 - parsedDay * 6 + parsedDay) + string.Empty + resultday))).Replace("-", string.Empty).ToLower();
            else
                return string.Empty;
        }
    }
}
