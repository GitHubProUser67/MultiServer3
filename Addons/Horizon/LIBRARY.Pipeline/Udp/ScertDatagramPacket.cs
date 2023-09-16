using MultiServer.Addons.Horizon.RT.Models;
using System.Net;

namespace MultiServer.Addons.Horizon.LIBRARY.Pipeline.Udp
{
    public class ScertDatagramPacket
    {

        public EndPoint Source { get; set; }

        public EndPoint Destination { get; set; }

        public BaseScertMessage Message { get; set; }


        public ScertDatagramPacket(BaseScertMessage message, EndPoint destination)
        {
            Destination = destination;
            Source = null;
            Message = message;
        }

        public ScertDatagramPacket(BaseScertMessage message, EndPoint target, EndPoint source)
        {
            Source = source;
            Destination = target;
            Message = message;
        }

    }
}
