// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SpaceWizards.HttpListener.WebSockets
{
    internal static partial class HttpWebSocket
    {
        private const string SupportedVersion = "13";

        internal static async Task<HttpListenerWebSocketContext> AcceptWebSocketAsyncCore(HttpListenerContext context,
            string subProtocol,
            int receiveBufferSize,
            TimeSpan keepAliveInterval,
            ArraySegment<byte>? internalBuffer = null)
        {
            ValidateOptions(subProtocol, receiveBufferSize, MinSendBufferSize, keepAliveInterval);

            // get property will create a new response if one doesn't exist.
            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;
            ValidateWebSocketHeaders(context);

            string secWebSocketVersion = request.Headers[HttpKnownHeaderNames.SecWebSocketVersion];

            // Optional for non-browser client
            string origin = request.Headers[HttpKnownHeaderNames.Origin];

            string[] secWebSocketProtocols = null;
            string outgoingSecWebSocketProtocolString;
            bool shouldSendSecWebSocketProtocolHeader =
                ProcessWebSocketProtocolHeader(
                    request.Headers[HttpKnownHeaderNames.SecWebSocketProtocol],
                    subProtocol,
                    out outgoingSecWebSocketProtocolString);

            if (shouldSendSecWebSocketProtocolHeader)
            {
                secWebSocketProtocols = new string[] { outgoingSecWebSocketProtocolString };
                response.Headers.Add(HttpKnownHeaderNames.SecWebSocketProtocol, outgoingSecWebSocketProtocolString);
            }

            // negotiate the websocket key return value
            string secWebSocketKey = request.Headers[HttpKnownHeaderNames.SecWebSocketKey];
            string secWebSocketAccept = GetSecWebSocketAcceptString(secWebSocketKey);

            response.Headers.Add(HttpKnownHeaderNames.Connection, HttpKnownHeaderNames.Upgrade);
            response.Headers.Add(HttpKnownHeaderNames.Upgrade, WebSocketUpgradeToken);
            response.Headers.Add(HttpKnownHeaderNames.SecWebSocketAccept, secWebSocketAccept);

            response.StatusCode = (int)HttpStatusCode.SwitchingProtocols; // HTTP 101
            response.StatusDescription = HttpStatusDescription.Get(HttpStatusCode.SwitchingProtocols);

            HttpResponseStream responseStream = (response.OutputStream as HttpResponseStream);

            // Send websocket handshake headers
            await responseStream.WriteWebSocketHandshakeHeadersAsync().ConfigureAwait(false);

            WebSocket webSocket = WebSocket.CreateFromStream(context.Connection.ConnectedStream, isServer: true, subProtocol, keepAliveInterval);

            HttpListenerWebSocketContext webSocketContext = new HttpListenerWebSocketContext(
                                                                request.Url,
                                                                request.Headers,
                                                                request.Cookies,
                                                                context.User,
                                                                request.IsAuthenticated,
                                                                request.IsLocal,
                                                                request.IsSecureConnection,
                                                                origin,
                                                                secWebSocketProtocols != null ? secWebSocketProtocols : Array.Empty<string>(),
                                                                secWebSocketVersion,
                                                                secWebSocketKey,
                                                                webSocket);

            return webSocketContext;
        }

        private const bool WebSocketsSupported = true;
    }
}
#endif