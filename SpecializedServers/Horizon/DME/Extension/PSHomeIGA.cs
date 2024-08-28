using Horizon.MUM.Models;

namespace Horizon.DME.Extension
{
    public class PSHomeIGA
    {
        private static readonly byte[] KickCMD = new byte[] { 0x02, 0x0B, 0x00, 0x01, 0x00, 0x10, 0x64, 0x00, 0x00, 0x0B, 0xFF, 0xFF, 0xFF, 0xAB, 0xFF, 0xFF, 0xFF, 0xFF, 0x30, 0x32, 0x00, 0x00 }; // Thank you Camo!

        public static string KickClient(short DmeId, uint WorldId, uint DmeWorldId, bool retail)
        {
            ClientObject? homeDmeServer = retail ? DmeClass.TcpServer.GetServerPerAppId(20374) : DmeClass.TcpServer.GetServerPerAppId(20371);
            if (homeDmeServer != null && homeDmeServer.DmeWorld != null)
            {
                World? worldToSearchIn = homeDmeServer.DmeWorld.GetWorldById(WorldId, DmeWorldId);
                if (worldToSearchIn != null && worldToSearchIn.Clients.ContainsKey(DmeId))
                {
                    _ = Task.Run(() => { worldToSearchIn.SendTcpAppSingle(homeDmeServer, DmeId, KickCMD); });
                    return $"{DmeId} was kicked successfully in world: {worldToSearchIn.WorldId}!";
                }

                return $"{DmeId} was not found in a valid World!";
            }

            return "Home doesn't have any world populated!";
        }
    }
}
