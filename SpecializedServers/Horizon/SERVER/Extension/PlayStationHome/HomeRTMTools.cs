using CustomLogger;
using CyberBackendLibrary.Extension;
using Horizon.MUM.Models;
using Horizon.RT.Models;
using System.Text;

namespace Horizon.SERVER.Extension.PlayStationHome
{
    public class HomeRTMTools
    {
        private static readonly byte[] RexecHubMessageHeader = OtherExtensions.HexStringToByteArray("6400000FFFFFFFE5FFFFFFFF");

        public static Task<bool> SendRemoteCommand(string targetClientIp, string? AccessToken, string command, bool Retail)
        {
            bool AccessTokenProvided = !string.IsNullOrEmpty(AccessToken);
            ClientObject? client;

            if (AccessTokenProvided)
                client = MediusClass.Manager.GetClientByAccessToken(AccessToken, Retail ? 20374 : 20371);
            else
                client = MediusClass.Manager.GetClientsByIp(targetClientIp, Retail ? 20374 : 20371)?.FirstOrDefault(client => !client.IsActiveServer /*Ignore DME server if on same IP*/);

            if (client != null)
            {
                byte[] HubRexecMessage = OtherExtensions.CombineByteArray(RexecHubMessageHeader, EnsureMultipleOfFour(Encoding.ASCII.GetBytes(command)));

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
