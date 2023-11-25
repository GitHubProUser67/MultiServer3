// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Common;
using CustomLogger;

namespace Horizon.HTTPSERVICE
{
    sealed class CrudServerHandler : ChannelHandlerAdapter
    {
        static readonly ThreadLocalCache Cache = new();

        sealed class ThreadLocalCache : FastThreadLocal<AsciiString>
        {
            protected override AsciiString GetInitialValue()
            {
                DateTime dateTime = DateTime.UtcNow;
                return AsciiString.Cached($"{dateTime.DayOfWeek}, {dateTime:dd MMM yyyy HH:mm:ss z}");
            }
        }

        private static readonly AsciiString TypeJson = AsciiString.Cached("application/json");
        private static readonly AsciiString TypeIco = AsciiString.Cached("image/x-icon");
        private static readonly AsciiString ServerName = AsciiString.Cached("Microsoft HTTP API 2.0");
        private static readonly AsciiString CORS = AsciiString.Cached("Access-Control-Allow-Origin");
        private static readonly AsciiString ETAG = AsciiString.Cached("ETag");
        private static readonly AsciiString ContentTypeEntity = HttpHeaderNames.ContentType;
        private static readonly AsciiString DateEntity = HttpHeaderNames.Date;
        private static readonly AsciiString ContentLengthEntity = HttpHeaderNames.ContentLength;
        private static readonly AsciiString ServerEntity = HttpHeaderNames.Server;

        volatile ICharSequence date = Cache.Value;

        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            if (message is IFullHttpRequest request)
            {
                try
                {
                    Process(ctx, request);
                }
                finally
                {
                    ReferenceCountUtil.Release(message);
                }
            }
            else
                ctx.FireChannelRead(message);
        }

        private void Process(IChannelHandlerContext ctx, IFullHttpRequest request)
        {
            LoggerAccessor.LogInfo($"[Crud_Room_Manager] - Client - {ctx.Channel.RemoteAddress} Requested the HTTP/S Server with URL : {request.Uri}");

            switch (request.Uri)
            {
                case "/GetRooms/":
                    byte[] json = Encoding.UTF8.GetBytes(MEDIUS.CrudRoomManager.ToJson());
                    WriteResponse(ctx, Unpooled.WrappedBuffer(json), TypeJson, AsciiString.Cached($"{json.Length}"));
                    break;
                case "/favicon.ico":
                    if (File.Exists(Directory.GetCurrentDirectory() + "/static/wwwroot/http_service_favicon.ico"))
                    {
                        byte[] favicon = File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/http_service_favicon.ico");
                        WriteResponse(ctx, Unpooled.WrappedBuffer(favicon), TypeIco, AsciiString.Cached($"{favicon.Length}"));
                    }
                    else
                    {
                        DefaultFullHttpResponse notfoundresponse = new(HttpVersion.Http11, HttpResponseStatus.NotFound, Unpooled.Empty, false);
                        ctx.WriteAndFlushAsync(notfoundresponse);
                        ctx.CloseAsync();
                    }
                    break;
                default:
                    DefaultFullHttpResponse defaultresponse = new(HttpVersion.Http11, HttpResponseStatus.Forbidden, Unpooled.Empty, false);
                    ctx.WriteAndFlushAsync(defaultresponse);
                    ctx.CloseAsync();
                    break;
            }
        }

        private void WriteResponse(IChannelHandlerContext ctx, IByteBuffer buf, ICharSequence contentType, ICharSequence contentLength)
        {
            // Build the response object.
            DefaultFullHttpResponse response = new(HttpVersion.Http11, HttpResponseStatus.OK, buf, false);
            HttpHeaders headers = response.Headers;
            headers.Set(ContentTypeEntity, contentType);
            headers.Set(ServerEntity, ServerName);
            headers.Set(DateEntity, date);
            headers.Set(ContentLengthEntity, contentLength);
            headers.Set(CORS, "*");
            headers.Set(ETAG, Guid.NewGuid().ToString());

            // Close the non-keep-alive connection after the write operation is done.
            ctx.WriteAsync(response);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => context.CloseAsync();

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();
    }
}
