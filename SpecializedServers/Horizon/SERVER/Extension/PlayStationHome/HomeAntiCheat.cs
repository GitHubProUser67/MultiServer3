using CustomLogger;
using NetworkLibrary.Extension;
using DotNetty.Transport.Channels;
using EndianTools;
using Horizon.MUM.Models;
using Horizon.RT.Common;
using Horizon.RT.Models;
using static Horizon.SERVER.Medius.BaseMediusComponent;

namespace Horizon.SERVER.Extension.PlayStationHome
{
    public class HomeAntiCheat
    {
        #region Anticheat References

        private byte[] Ref1 = Convert.FromBase64String("j1S4yArhxE6OW2ZPQzq+oA==");

        private byte[] Ref2 = Convert.FromBase64String("XtKRFh5cJ/iW3RzTcyHa8g==");

        private byte[] Ref3 = Convert.FromBase64String("VDQrkh5H7aV9T/GbaDwNUw==");

        private byte[] Ref4 = Convert.FromBase64String("FA5bx20s0liKa36ROeQ8Lw==");

        private byte[] Ref5 = Convert.FromBase64String("gjhutOUMfyuOPC5gjtt9/Q==");

        private byte[] Ref6 = Convert.FromBase64String("AAAAAAAAAAE=");

        #endregion

        public async void ProcessAntiCheatQuery(ChannelData data, RT_MSG_SERVER_CHEAT_QUERY clientCheatQuery, IChannel? clientChannel)
        {
            // client channel is null, don't process
            if (clientChannel == null)
                return;

            // clientCheatQuery.Data is null, don't process
            if (clientCheatQuery.Data == null)
                return;

            ClientObject? client = data.ClientObject;
            byte[] QueryData = clientCheatQuery.Data;

            if (client?.ClientHomeData != null)
            {
                switch (client.ClientHomeData.Type)
                {
                    case "HDK With Offline":
                        switch (client.ClientHomeData.Version)
                        {
                            case "01.86.09":
                                switch (clientCheatQuery.StartAddress)
                                {
                                    case 0x101590b0:
                                        if (clientCheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH && (QueryData.Length != 16 || !OtherExtensions.AreArraysIdentical(QueryData, Ref3)))
                                        {
                                            string anticheatMsg = $"[MLS] - HOME ANTI-CHEAT - DETECTED MALICIOUS USAGE (Reason: FREEZE ATTEMPT) - User:{client?.IP + ":" + client?.AccountName} CID:{data.MachineId}";

                                            _ = client?.CurrentChannel?.BroadcastSystemMessage(client.CurrentChannel.LocalClients.Where(x => x != client), anticheatMsg, byte.MaxValue);

                                            LoggerAccessor.LogError(anticheatMsg);

                                            data.State = ClientState.DISCONNECTED;
                                            await clientChannel.CloseAsync();
                                        }
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case "HDK Online Only":
                        switch (client.ClientHomeData.Version)
                        {
                            default:
                                break;
                        }
                        break;
                    case "HDK Online Only (Dbg Symbols)":
                        switch (client.ClientHomeData.Version)
                        {
                            default:
                                break;
                        }
                        break;
                    case "Online Debug":
                        switch (client.ClientHomeData.Version)
                        {
                            default:
                                break;
                        }
                        break;
                    case "Retail":
                        switch (client.ClientHomeData.Version)
                        {
                            case "01.86.09":
                                switch (clientCheatQuery.StartAddress)
                                {
                                    case 268759069U:
                                        if (clientCheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH && QueryData.Length == 16 && (OtherExtensions.AreArraysIdentical(QueryData, Ref1)
                                            || OtherExtensions.AreArraysIdentical(QueryData, Ref2) || OtherExtensions.AreArraysIdentical(QueryData, Ref4)))
                                        {
                                            string anticheatMsg = $"[MLS] - HOME ANTI-CHEAT - DETECTED MALICIOUS USAGE (Reason: UNAUTHORIZED TOOL USAGE) - User:{client?.IP + ":" + client?.AccountName} CID:{data.MachineId}";

                                            _ = client?.CurrentChannel?.BroadcastSystemMessage(client.CurrentChannel.LocalClients.Where(x => x != client), anticheatMsg, byte.MaxValue);

                                            LoggerAccessor.LogError(anticheatMsg);

                                            data.State = ClientState.DISCONNECTED;
                                            await clientChannel.CloseAsync();
                                        }
                                        break;
                                    case 0x100BA820:
                                        if (clientCheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH && (QueryData.Length != 16 || !OtherExtensions.AreArraysIdentical(QueryData, Ref3)))
                                        {
                                            string anticheatMsg = $"[MLS] - HOME ANTI-CHEAT - DETECTED MALICIOUS USAGE (Reason: FREEZE ATTEMPT) - User:{client?.IP + ":" + client?.AccountName} CID:{data.MachineId}";

                                            _ = client?.CurrentChannel?.BroadcastSystemMessage(client.CurrentChannel.LocalClients.Where(x => x != client), anticheatMsg, byte.MaxValue);

                                            LoggerAccessor.LogError(anticheatMsg);

                                            data.State = ClientState.DISCONNECTED;
                                            await clientChannel.CloseAsync();
                                        }
                                        break;
                                    case 0x104F7320:
                                        if (clientCheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY && QueryData.Length == 4)
                                        {
                                            if (client != null && client.HomePointer == 0)
                                            {
                                                client.SetPointer(BitConverter.ToUInt32(BitConverter.IsLittleEndian ? EndianUtils.ReverseArray(QueryData) : QueryData));

                                                client.Tasks.TryAdd("1.86 Retail ANTI FREEZE", Task.Run(() =>
                                                {

                                                    while (true)
                                                    {
                                                        if (client.IsInGame)
                                                        {
                                                            CheatQuery(client.HomePointer + 5300U, 8, client, CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY);
                                                            CheatQuery(client.HomePointer + 6928U, 84, client, CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH);
                                                        }

                                                        Thread.Sleep(3500);
                                                    }

                                                }));
                                            }
                                        }
                                        break;
                                    default:
                                        if (client != null)
                                        {
                                            if (clientCheatQuery.StartAddress == client.HomePointer + 6928U && clientCheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH && QueryData.Length == 16 && OtherExtensions.AreArraysIdentical(QueryData, Ref5))
                                            {
                                                string anticheatMsg = $"[MLS] - HOME ANTI-CHEAT - DETECTED MALICIOUS USAGE (Reason: LAG FREEZE ATTEMPT) - User:{client?.IP + ":" + client?.AccountName} CID:{data.MachineId}";

                                                _ = client?.CurrentChannel?.BroadcastSystemMessage(client.CurrentChannel.LocalClients.Where(x => x != client), anticheatMsg, byte.MaxValue);

                                                LoggerAccessor.LogError(anticheatMsg);

                                                data.State = ClientState.DISCONNECTED;
                                                await clientChannel.CloseAsync();
                                            }
                                            else if (clientCheatQuery.StartAddress == client.HomePointer + 5300U && clientCheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY && QueryData.Length == 8 && OtherExtensions.AreArraysIdentical(QueryData, Ref6))
                                            {
                                                string anticheatMsg = $"[MLS] - HOME ANTI-CHEAT - DETECTED MALICIOUS USAGE (Reason: FREEZE ATTEMPT) - User:{client?.IP + ":" + client?.AccountName} CID:{data.MachineId}";

                                                _ = client?.CurrentChannel?.BroadcastSystemMessage(client.CurrentChannel.LocalClients.Where(x => x != client), anticheatMsg, byte.MaxValue);

                                                LoggerAccessor.LogError(anticheatMsg);

                                                data.State = ClientState.DISCONNECTED;
                                                await clientChannel.CloseAsync();
                                            }
                                        }
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
        }

        public List<(uint, int, CheatQueryType, int)> ProcessAntiCheatRequest(ClientObject? client, string HomeUserEntry)
        {
            List<(uint, int, CheatQueryType, int)> results = new();

            if (client?.ClientHomeData != null)
            {
                switch (client.ClientHomeData.Type)
                {
                    case "HDK With Offline":
                        switch (client.ClientHomeData.Version)
                        {
                            case "01.86.09":
                                results.Add((0x101590b0, 20, CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH, 1));
                                break;
                            default:
                                break;
                        }
                        break;
                    case "HDK Online Only":
                        switch (client.ClientHomeData.Version)
                        {
                            default:
                                break;
                        }
                        break;
                    case "HDK Online Only (Dbg Symbols)":
                        switch (client.ClientHomeData.Version)
                        {
                            default:
                                break;
                        }
                        break;
                    case "Online Debug":
                        switch (client.ClientHomeData.Version)
                        {
                            default:
                                break;
                        }
                        break;
                    case "Retail":
                        switch (client.ClientHomeData.Version)
                        {
                            case "01.86.09":
                                if (MediusClass.Settings.PlaystationHomeUsersServersAccessList.TryGetValue(HomeUserEntry, out string? value) && !string.IsNullOrEmpty(value))
                                {
                                    switch (value)
                                    {
                                        case "RTM":
                                            break;
                                        default:
                                            results.Add((268759069U, 27, CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH, 1));
                                            break;
                                    }
                                }
                                else
                                    results.Add((268759069U, 27, CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH, 1));

                                results.Add((0x100BA820, 20, CheatQueryType.DME_SERVER_CHEAT_QUERY_SHA1_HASH, 1));
                                results.Add((0x104F7320, 4, CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY, 1));
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }

            return results;
        }

        #region PokeEngine
        private bool CheatQuery(uint address, int Length, ClientObject client, CheatQueryType Type = CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY, int SequenceId = 1)
        {
            // address = 0, don't read
            if (address == 0)
                return false;

            // read client memory
            client.Queue(new RT_MSG_SERVER_CHEAT_QUERY()
            {
                QueryType = Type,
                SequenceId = SequenceId,
                StartAddress = address,
                Length = Length,
            });

            // return read
            return true;
        }

        private bool PokeAddress(uint patchLocation, byte[] Payload, ClientObject client)
        {
            // patch location = 0, don't patch
            if (patchLocation == 0)
                return false;

            // poke client memory
            client.Queue(new RT_MSG_SERVER_MEMORY_POKE()
            {
                start_Address = patchLocation,
                Payload = Payload,
                SkipEncryption = false,
            });

            // return patched
            return true;
        }
        #endregion
    }
}
