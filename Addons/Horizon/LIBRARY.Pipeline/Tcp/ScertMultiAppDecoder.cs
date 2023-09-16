using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MultiServer.Addons.Horizon.RT.Models;

namespace MultiServer.Addons.Horizon.LIBRARY.Pipeline.Tcp
{
    public class ScertMultiAppDecoder : MessageToMessageDecoder<RT_MSG_CLIENT_MULTI_APP_TOSERVER>
    {
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
            catch (Exception ex)
            {
                ServerConfiguration.LogWarn(ex.ToString());
            }
        }
    }
}
