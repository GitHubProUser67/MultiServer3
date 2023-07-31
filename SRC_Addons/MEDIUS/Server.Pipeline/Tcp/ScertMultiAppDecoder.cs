using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using PSMultiServer.Addons.Medius.RT.Models;

namespace PSMultiServer.Addons.Medius.Server.Pipeline.Tcp
{
    public class ScertMultiAppDecoder : MessageToMessageDecoder<RT_MSG_CLIENT_MULTI_APP_TOSERVER>
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ScertMultiAppDecoder>();

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        public ScertMultiAppDecoder()
        {

        }

        protected override void Decode(IChannelHandlerContext context, RT_MSG_CLIENT_MULTI_APP_TOSERVER input, List<object> output)
        {
            try
            {
                foreach (var message in input.Messages)
                    output.Add(message);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
