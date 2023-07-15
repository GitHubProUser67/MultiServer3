using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Models;

namespace PSMultiServer.SRC_Addons.MEDIUS.Server.Pipeline.Udp
{
    public class ScertDatagramMultiAppDecoder : MessageToMessageDecoder<ScertDatagramPacket>
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ScertDatagramMultiAppDecoder>();

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        public ScertDatagramMultiAppDecoder()
        {

        }

        protected override void Decode(IChannelHandlerContext context, ScertDatagramPacket input, List<object> output)
        {
            try
            {
                if (input.Message is RT_MSG_CLIENT_MULTI_APP_TOSERVER multiApp)
                {
                    foreach (var message in multiApp.Messages)
                        output.Add(new ScertDatagramPacket(message, input.Destination, input.Source));
                }
                else
                {
                    output.Add(input);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
