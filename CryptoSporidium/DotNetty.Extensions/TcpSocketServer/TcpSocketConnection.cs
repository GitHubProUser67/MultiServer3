using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Collections.Concurrent;

namespace DotNetty.Extensions
{
    public class TcpSocketConnection
    {
        private IChannel _channel;

        public TcpSocketConnection(IChannel channel)
        {
            _channel = channel;
        }

        public string? Id
        {
            get
            {
                if (_channel == null)
                    return null;
                return _channel.Id.AsShortText();
            }
        }

        public bool Open
        {
            get
            {
                if (_channel == null)
                    return false;

                return _channel.Open;
            }
        }

        public string Name { get; set; }

        private readonly ConcurrentDictionary<object, object> _dict = new ConcurrentDictionary<object, object>();

        public IDictionary<object, object> SessionItems
        {
            get
            {
                return _dict;
            }
        }

        public async Task SendAsync(byte[] bytes)
        {
            await _channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(bytes));
        }

        public async Task CloseAsync()
        {
            if (_channel != null)
                await _channel.CloseAsync();
        }
    }
}
