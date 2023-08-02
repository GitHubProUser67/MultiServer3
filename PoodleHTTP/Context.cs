using System.Net;

namespace PSMultiServer.PoodleHTTP
{
    public delegate bool ParameterProvider(string key, out string value);

    public class Context
    {
        public HttpListenerRequest Request { get; }

        public HttpListenerResponse Response { get; }

        public ParameterProvider TryGetParameter { get; set; }

        public Context(HttpListenerRequest request, HttpListenerResponse response)
        {
            Request = request;
            Response = response;
            TryGetParameter = (string _, out string value) =>
            {
                value = null!;
                return false;
            };
        }
    }
}
