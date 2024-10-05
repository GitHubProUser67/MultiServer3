using CustomLogger;
using CyberBackendLibrary.Extension;
using Horizon.MUM.Models;
using Horizon.RT.Models;
using System.Text;

namespace Horizon.SERVER.Extension.PlayStationHome
{
    public class HomeRTMTools
    {
        private static readonly byte[] RexecHubMessageHeader = new byte[] { 0x64, 0x00 };

        public static Task<bool> SendRemoteCommand(string targetClientIp, string? AccessToken, string command, bool Retail)
        {
            if (command.Length > ushort.MaxValue)
                return Task.FromResult(false);

            bool AccessTokenProvided = !string.IsNullOrEmpty(AccessToken);
            ClientObject? client;

            if (AccessTokenProvided)
                client = MediusClass.Manager.GetClientByAccessToken(AccessToken, Retail ? 20374 : 20371);
            else
                client = MediusClass.Manager.GetClientsByIp(targetClientIp, Retail ? 20374 : 20371)?.FirstOrDefault(client => !client.IsActiveServer /*Ignore DME server if on same IP*/);

            if (client != null)
            {
                byte[] HubRexecMessage = OtherExtensions.CombineByteArrays(RexecHubMessageHeader, new byte[][] { BitConverter.GetBytes(BitConverter.IsLittleEndian ? EndianTools.EndianUtils.ReverseUshort((ushort)(command.Length + 9)) : (ushort)(command.Length + 9))
                    , OtherExtensions.HexStringToByteArray("FFFFFFE5FFFFFFFF"), EnsureMultipleOfEight(Encoding.ASCII.GetBytes(command)) });

                client.Queue(new MediusBinaryFwdMessage1()
                {
                    MessageID = new MessageId(),
                    MessageType = RT.Common.MediusBinaryMessageType.TargetBinaryMsg,
                    OriginatorAccountID = client.AccountId,
                    MessageSize = HubRexecMessage.Length,
                    Message = HubRexecMessage
                });

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
