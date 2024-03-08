using BackendProject.MiscUtils;
using HttpMultipartParser;

namespace WebUtils.NDREAMS.Aurora
{
    public class visitClass
    {
        public static string? ProcessVisit(byte[]? PostData, string? ContentType)
        {
            string friends = string.Empty;
            string name = string.Empty;
            string age = string.Empty;
            string bonus = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    friends = data.GetParameterValue("friends");
                    name = data.GetParameterValue("name");
                    age = data.GetParameterValue("age");
                    bonus = data.GetParameterValue("bonus");

                    ms.Flush();
                }

                return $"<xml><result><new>true</new><days>1</days><bonus>{bonus}</bonus><sessions>0</sessions><first>true</first></result></xml>";
            }

            return null;
        }
    }
}
