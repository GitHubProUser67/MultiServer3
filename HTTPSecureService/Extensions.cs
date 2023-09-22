using MultiServer.HTTPService;
using System.Text;
using NetCoreServer;

namespace MultiServer.HTTPSecureService
{
    public class Extensions
    {
        public static void ReturnNotFoundError(HttpResponse response, string link)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(PreMadeWebPages.filenotfound.Replace("PUT_LINK_HERE", link));

            // Set the response headers for the HTML content
            response.SetContentType("text/html");

            response.SetHeader("Content-Encoding", "UTF8");

            response.SetBody(buffer);
        }
    }
}
