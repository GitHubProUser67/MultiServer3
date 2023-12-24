using CustomLogger;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using BackendProject.Horizon.RT.Models;

namespace BackendProject.Horizon.LIBRARY.Pipeline.Tcp
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
                LoggerAccessor.LogWarn(ex.ToString());
            }
        }
    }
}
