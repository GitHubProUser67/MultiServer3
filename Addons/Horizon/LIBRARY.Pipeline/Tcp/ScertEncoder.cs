using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.RT.Cryptography;
using MultiServer.Addons.Horizon.RT.Models;

namespace MultiServer.Addons.Horizon.LIBRARY.Pipeline.Tcp
{
    public class ScertEncoder : MessageToMessageEncoder<BaseScertMessage>
    {
        readonly ICipher[] _ciphers = null;
        readonly Func<RT_MSG_TYPE, CipherContext, ICipher> _getCipher = null;

        public ScertEncoder(params ICipher[] ciphers)
        {
            _ciphers = ciphers;
            _getCipher = (id, ctx) =>
            {
                return _ciphers?.FirstOrDefault(x => x.Context == ctx);
            };
        }

        protected override void Encode(IChannelHandlerContext ctx, BaseScertMessage message, List<object> output)
        {
            if (message is null)
                return;

            // Log
            if (message.CanLog())
                ServerConfiguration.LogInfo($"SEND {ctx.Channel}: {message}");

            if (!ctx.HasAttribute(Constants.SCERT_CLIENT))
                ctx.GetAttribute(Constants.SCERT_CLIENT).Set(new Attribute.ScertClientAttribute());
            var scertClient = ctx.GetAttribute(Constants.SCERT_CLIENT).Get();

            scertClient.OnMessage(message);

            // Serialize
            var msgs = message.Serialize(scertClient.MediusVersion, scertClient.ApplicationID, scertClient.CipherService);

            foreach (var msg in msgs)
            {
                var byteBuffer = ctx.Allocator.Buffer(msg.Length);
                byteBuffer.WriteBytes(msg);
                output.Add(byteBuffer);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            ServerConfiguration.LogWarn(exception.ToString());
            context.CloseAsync();
        }
    }
}
