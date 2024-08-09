// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using HttpWebSocket = SpaceWizards.HttpListener.WebSockets.HttpWebSocket;

namespace SpaceWizards.HttpListener
{
    public sealed unsafe partial class HttpListenerContext
    {
        private HttpConnection _connection;

        internal HttpListenerContext(HttpConnection connection)
        {
            _connection = connection;
            _response = new HttpListenerResponse(this);
            Request = new HttpListenerRequest(this);
            ErrorStatus = 400;
        }

        internal int ErrorStatus { get; set; }

        internal string ErrorMessage { get; set; }

        internal bool HaveError => ErrorMessage != null;

        internal HttpConnection Connection => _connection;

        internal void ParseAuthentication(AuthenticationSchemes expectedSchemes)
        {
            if (expectedSchemes == AuthenticationSchemes.Anonymous)
                return;

            string header = Request.Headers[HttpKnownHeaderNames.Authorization];
            if (string.IsNullOrEmpty(header))
                return;

            if (IsBasicHeader(header))
            {
                _user = ParseBasicAuthentication(header.Substring(AuthenticationTypes.Basic.Length + 1));
            }
        }

        internal IPrincipal ParseBasicAuthentication(string authData) =>
            TryParseBasicAuth(authData, out HttpStatusCode errorCode, out string username, out string password) ?
                new GenericPrincipal(new HttpListenerBasicIdentity(username, password), Array.Empty<string>()) :
                null;

        internal static bool IsBasicHeader(string header) =>
            header.Length >= 6 &&
            header[5] == ' ' &&
            string.Compare(header, 0, AuthenticationTypes.Basic, 0, 5, StringComparison.OrdinalIgnoreCase) == 0;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        internal static bool TryParseBasicAuth(string headerValue, out HttpStatusCode errorCode, [NotNullWhen(true)] out string username, [NotNullWhen(true)] out string password)
#else
        internal static bool TryParseBasicAuth(string headerValue, out HttpStatusCode errorCode, out string username, out string password)
#endif
        {
            errorCode = HttpStatusCode.OK;
            username = password = null;
            try
            {
                if (string.IsNullOrWhiteSpace(headerValue))
                {
                    return false;
                }

                string authString = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue));
                int colonPos = authString.IndexOf(':');
                if (colonPos < 0)
                {
                    // username must be at least 1 char
                    errorCode = HttpStatusCode.BadRequest;
                    return false;
                }

                username = authString.Substring(0, colonPos);
                password = authString.Substring(colonPos + 1);
                return true;
            }
            catch
            {
                errorCode = HttpStatusCode.InternalServerError;
                return false;
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        public Task<WebSockets.HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval)
        {
            return HttpWebSocket.AcceptWebSocketAsyncCore(this, subProtocol, receiveBufferSize, keepAliveInterval);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<WebSockets.HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval, ArraySegment<byte> internalBuffer)
        {
            WebSocketValidate.ValidateArraySegment(internalBuffer, nameof(internalBuffer));
            HttpWebSocket.ValidateOptions(subProtocol, receiveBufferSize, HttpWebSocket.MinSendBufferSize, keepAliveInterval);
            return HttpWebSocket.AcceptWebSocketAsyncCore(this, subProtocol, receiveBufferSize, keepAliveInterval, internalBuffer);
        }
#endif
    }
}
