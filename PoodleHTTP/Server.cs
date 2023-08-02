using System.Net;

namespace PSMultiServer.PoodleHTTP
{
    public class Server
    {
        private HttpListener? _listener;
        private Middleware<Context> _middleware = Middlewares.Empty<Context>();

        public bool IsRunning => _listener is { IsListening: true };

        public static string HostUrl { get; set; }

        public Server(string host, int port)
        {
            HostUrl = $"http://{host}:{port}/";
        }

        public Server Use()
        {
            return this;
        }

        public Server Use(Middleware<Context> middleware)
        {
            _middleware = _middleware.Then(middleware);
            return this;
        }

        public bool Start()
        {
            try
            {
                StartHttpListener();
                return true;
            }
            catch (HttpListenerException e)
            {
                ServerConfiguration.LogWarn("Failed to start HttpListener.", e);
                if (e.ErrorCode == 5)
                {
                    NetAclChecker.AddAddress(HostUrl);
                    StartHttpListener();
                    return true;
                }

                return false;
            }
        }

        public void Stop()
        {
            if (_listener is { IsListening: true })
            {
                _listener.Stop();
                ServerConfiguration.LogInfo("Http server has been stopped.");
            }
        }

        private void StartHttpListener()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(HostUrl);
            _listener.Start();

            AsyncProcessRequest();

            ServerConfiguration.LogInfo($"Http server has started listening: {HostUrl}...");
        }

        private void AsyncProcessRequest()
        {
            Task.Run(async () =>
            {
                while (_listener!.IsListening)
                {
                    try
                    {
                        HttpListenerContext context = await _listener.GetContextAsync();
                        Context ctx = new(context.Request, context.Response);

                        _middleware.Run(ctx);
                    }
                    catch (IOException e)
                    {
                        ServerConfiguration.LogWarn(nameof(IOException), e);
                    }
                    catch (HttpListenerException e)
                    {
                        const int errorOperationAborted = 995;
                        if (e.ErrorCode == errorOperationAborted)
                        {
                            // The IO operation has been aborted because of either a thread exit or an application request.
                            break;
                        }

                        ServerConfiguration.LogWarn(nameof(HttpListenerException), e);
                    }
                    catch (InvalidOperationException e)
                    {
                        ServerConfiguration.LogWarn(nameof(InvalidOperationException), e);
                    }
                }
            });
        }
    }
}
