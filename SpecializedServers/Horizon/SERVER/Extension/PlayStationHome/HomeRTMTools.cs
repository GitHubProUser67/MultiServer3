using CyberBackendLibrary.Extension;
using Horizon.MUM.Models;
using Horizon.RT.Models;
using System.Text;

namespace Horizon.SERVER.Extension.PlayStationHome
{
    public class HomeRTMTools
    {
        private static readonly byte[] RexecHubMessageHeader = DataUtils.HexStringToByteArray("6400000FFFFFFFE5FFFFFFFF");

        public static Task SendRemoteCommand(ClientObject client, string command)
        {
            byte[] HubRexecMessage = DataUtils.CombineByteArray(RexecHubMessageHeader, EnsureMultipleOfFour(Encoding.ASCII.GetBytes(command)));

            client.Queue(new MediusBinaryFwdMessage1()
            {
                MessageID = new MessageId(),
                MessageType = RT.Common.MediusBinaryMessageType.TargetBinaryMsg,
                OriginatorAccountID = client.AccountId,
                MessageSize = HubRexecMessage.Length,
                Message = HubRexecMessage
            });

            return Task.CompletedTask;
        }

        private static byte[] EnsureMultipleOfFour(byte[] input)
        {
            int length = input.Length;
            int remainder = length % 4;

            if (remainder == 0)
                return input; // Already a multiple of 4

            byte[] paddedArray = new byte[length + (4 - remainder)];

            Array.Copy(input, paddedArray, length);

            return paddedArray;
        }
    }
}
