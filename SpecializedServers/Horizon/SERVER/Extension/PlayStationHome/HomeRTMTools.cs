using CustomLogger;
using NetworkLibrary.Extension;
using Horizon.MUM.Models;
using Horizon.RT.Models;
using System.Text;

namespace Horizon.SERVER.Extension.PlayStationHome
{
    public class HomeRTMTools
    {
        private static readonly byte[] RexecHubMessageHeader = new byte[] { 0x64, 0x00 };

        private static List<string> ForbiddenWords = new() { "rexec", "ping" };

        public static Task<bool> SendRemoteCommand(string targetClientIp, string? AccessToken, string command, bool Retail)
        {
            if (string.IsNullOrEmpty(command) || command.Length > ushort.MaxValue || (!command.StartsWith("say", StringComparison.InvariantCultureIgnoreCase) && ForbiddenWords.Any(x => x.Contains(command, StringComparison.InvariantCultureIgnoreCase))))
                return Task.FromResult(false);

            bool AccessTokenProvided = !string.IsNullOrEmpty(AccessToken);
            List<ClientObject>? clients = null;

            if (AccessTokenProvided)
            {
                ClientObject? client = MediusClass.Manager.GetClientByAccessToken(AccessToken, Retail ? 20374 : 20371);
                if (client != null)
                {
                    clients = new()
                    {
                        client
                    };
                }
            }
            else
                clients = MediusClass.Manager.GetClientsByIp(targetClientIp, Retail ? 20374 : 20371);

            if (clients != null)
            {
                byte[] HubRexecMessage = OtherExtensions.CombineByteArrays(RexecHubMessageHeader, new byte[][] { BitConverter.GetBytes(BitConverter.IsLittleEndian ? EndianTools.EndianUtils.ReverseUshort((ushort)(command.Length + 9)) : (ushort)(command.Length + 9))
                    , OtherExtensions.HexStringToByteArray("FFFFFFE5FFFFFFFF"), EnsureMultipleOfEight( OtherExtensions.CombineByteArray(Encoding.UTF8.GetBytes(command), Encoding.ASCII.GetBytes("\0"))) });

                clients.ForEach(x => x.Queue(new MediusBinaryFwdMessage1()
                {
                    MessageID = new MessageId("o"),
                    MessageType = RT.Common.MediusBinaryMessageType.TargetBinaryMsg,
                    OriginatorAccountID = x.AccountId,
                    MessageSize = HubRexecMessage.Length,
                    Message = HubRexecMessage
                }));

                return Task.FromResult(true);
            }

            LoggerAccessor.LogError($"[HomeRTMTools] - {(!AccessTokenProvided ? $"Ip:{targetClientIp}" : $"AccessToken:{AccessToken}")} didn't return any Medius clients!");

            return Task.FromResult(false);
        }

        private static byte[] EnsureMultipleOfEight(byte[] input)
        {
            int length = input.Length;
            int remainder = length % 8;

            if (remainder == 0)
                return input; // Already a multiple of 8

            byte[] paddedArray = new byte[length + (8 - remainder)];

            Array.Copy(input, paddedArray, length);

            return paddedArray;
        }
    }
}
