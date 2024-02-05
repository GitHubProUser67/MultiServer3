using BackendProject.Horizon.RT.Common;
using CustomLogger;
using Horizon.MEDIUS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Horizon.MUM
{
    public class MumChannelHandler
    {
        private readonly static List<List<Channel>> AccessibleChannels = new();

        public static Task AddMumChannelsList(List<Channel> channelstoadd)
        {
            lock (AccessibleChannels)
            {
                AccessibleChannels.Add(channelstoadd);
            }

            return Task.CompletedTask;
        }

        public static Task UpdateMumChannels(int index, List<Channel> channelstoupdate)
        {
            lock (AccessibleChannels)
            {
                AccessibleChannels[index] = channelstoupdate;
            }

            return Task.CompletedTask;
        }

        public static string JsonSerializeChannel(Channel channel)
        {
            return JsonConvert.SerializeObject(channel, Formatting.Indented, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
            });
        }

        public static string XMLSerializeChannel(Channel channel)
        {
            return JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(channel, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
            }), "Channel")?.OuterXml ?? "<Channel></Channel>";
        }

        public static string JsonSerializeChannelsList()
        {
            if (AccessibleChannels.Count > 0)
                return JsonConvert.SerializeObject(AccessibleChannels, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                });
            else
                return "[]";
        }

        public static string XMLSerializeChannelsList()
        {
            if (AccessibleChannels.Count > 0)
                return JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ChannelsList", JToken.Parse(JsonConvert.SerializeObject(AccessibleChannels, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                })))).ToString()
                    , "Root")?.OuterXml ?? "<Root></Root>";
            else
                return "<Root></Root>";
        }

        public static string GetCRC32ChannelsList()
        {
            string XMLData = "<Root>";

            foreach (List<Channel> channels in AccessibleChannels)
            {
                for (int i = 0; i < channels.Count; i++)
                {
                    Channel channel = channels[i];
                    XMLData += $"<CRC32 name=\"{channel.Name}\">{new BackendProject.MiscUtils.Crc32Utils().Get(Encoding.UTF8.GetBytes(channel.Name + XMLSerializeChannel(channel))):X}</CRC32>";
                }
            }

            return XMLData + "</Root>";
        }

        public static int GetIndexOfLocalChannelByNameAndAppId(string channelName, int AppId)
        {
            try
            {
                for (int i = 0; i < AccessibleChannels.Count; i++)
                {
                    // If a matching channel is found, return the index of the list where it was found.
                    if (AccessibleChannels[i].FindIndex(channel => channel.Name == channelName && channel.ApplicationId == AppId) != -1)
                        return i;
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetIndexOfLocalChannelByNameAndAppId thrown an exception: {e}");
            }

            // If no matching channel is found, return -1
            return -1;
        }

        public static Channel? GetRemoteChannelByName(Channel channel, IPAddress ClientIp)
        {
            try
            {
                if (MediusClass.MUMServerIPsList.Count > 0)
                {
                    foreach (string ip in MediusClass.MUMServerIPsList)
                    {
                        string? RemoteChannelsList = MumClient.GetJsonServerResult(ip, 10076, "GetChannelsJson");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<List<Channel>>? ConvertedChannelsLists = JsonConvert.DeserializeObject<List<List<Channel>>>(RemoteChannelsList, new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                            });

                            if (ConvertedChannelsLists != null)
                            {
                                for (int i = 0; i < ConvertedChannelsLists.Count; i++)
                                {
                                    Channel? matchingChannel = ConvertedChannelsLists[i].FirstOrDefault(x => x.Name == channel.Name && x.ApplicationId == channel.ApplicationId);

                                    if (matchingChannel != null)
                                        return matchingChannel;
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException invalidOperationException)
            {
                if (invalidOperationException.Message.Contains("Sequence contains no elements"))
                    LoggerAccessor.LogWarn($"[MUM] - GetRemoteChannelByName No matching channel found in any server.");
                else
                    LoggerAccessor.LogError($"[MUM] - GetRemoteChannelByName thrown an InvalidOperationException: {invalidOperationException}");
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetRemoteChannelByName thrown an exception: {e}");
            }

            return null;
        }

        public static Channel? GetRemoteChannelById(int WorldId, int Appid, IPAddress ClientIp)
        {
            try
            {
                if (MediusClass.MUMServerIPsList.Count > 0)
                {
                    foreach (string ip in MediusClass.MUMServerIPsList)
                    {
                        string? RemoteChannelsList = MumClient.GetJsonServerResult(ip, 10076, "GetChannelsJson");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<List<Channel>>? ConvertedChannelsLists = JsonConvert.DeserializeObject<List<List<Channel>>>(RemoteChannelsList, new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                            });

                            if (ConvertedChannelsLists != null)
                            {
                                for (int i = 0; i < ConvertedChannelsLists.Count; i++)
                                {
                                    Channel? matchingChannel = ConvertedChannelsLists[i].FirstOrDefault(x => x.Id == WorldId && x.ApplicationId == Appid);

                                    if (matchingChannel != null)
                                        return matchingChannel;
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException invalidOperationException)
            {
                if (invalidOperationException.Message.Contains("Sequence contains no elements"))
                    LoggerAccessor.LogWarn($"[MUM] - GetRemoteChannelById No matching channel found in any server.");
                else
                    LoggerAccessor.LogError($"[MUM] - GetRemoteChannelById thrown an InvalidOperationException: {invalidOperationException}");
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetRemoteChannelById thrown an exception: {e}");
            }

            return null;
        }

        public static Channel? GetLeastPopulatedRemoteChannel(int Appid, IPAddress ClientIp)
        {
            try
            {
                if (MediusClass.MUMServerIPsList.Count > 0)
                {
                    foreach (string ip in MediusClass.MUMServerIPsList)
                    {
                        string? RemoteChannelsList = MumClient.GetJsonServerResult(ip, 10076, "GetChannelsJson");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<List<Channel>>? ConvertedChannelsLists = JsonConvert.DeserializeObject<List<List<Channel>>>(RemoteChannelsList, new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                            });

                            if (ConvertedChannelsLists != null)
                            {
                                for (int i = 0; i < ConvertedChannelsLists.Count; i++)
                                {
                                    Channel? matchingChannel = ConvertedChannelsLists[i]
                                        .Where(channel => channel.ApplicationId == Appid)
                                        .OrderBy(channel => channel.PlayerCount)
                                        .FirstOrDefault();

                                    if (matchingChannel != null)
                                        return matchingChannel;
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException invalidOperationException)
            {
                if (invalidOperationException.Message.Contains("Sequence contains no elements"))
                    LoggerAccessor.LogWarn($"[MUM] - GetLeastPopulatedRemoteChannel No matching channel found in any server.");
                else
                    LoggerAccessor.LogError($"[MUM] - GetLeastPopulatedRemoteChannel thrown an InvalidOperationException: {invalidOperationException}");
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetLeastPopulatedRemoteChannel thrown an exception: {e}");
            }

            return null;
        }

        public static IEnumerable<Channel>? GetRemoteChannelListFiltered(int appId, int pageIndex, int pageSize, ChannelType type, ulong FieldMask1, ulong FieldMask2, ulong FieldMask3, ulong FieldMask4, MediusLobbyFilterMaskLevelType filterMaskLevelType, IPAddress ClientIp)
        {
            try
            {
                if (MediusClass.MUMServerIPsList.Count > 0)
                {
                    foreach (string ip in MediusClass.MUMServerIPsList)
                    {
                        string? RemoteChannelsList = MumClient.GetJsonServerResult(ip, 10076, "GetChannelsJson");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<List<Channel>>? ConvertedChannelsLists = JsonConvert.DeserializeObject<List<List<Channel>>>(RemoteChannelsList, new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                            });

                            if (ConvertedChannelsLists != null)
                            {
                                for (int i = 0; i < ConvertedChannelsLists.Count; i++)
                                {
                                    IEnumerable<Channel>? matchingChannelList = ConvertedChannelsLists[i]
                                        .Where(x => x.Type == type &&
                                            x.ApplicationId == appId &&
                                            x.GenericField1 == FieldMask1 &&
                                            x.GenericField2 == FieldMask2 &&
                                            x.GenericField3 == FieldMask3 &&
                                            x.GenericField4 == FieldMask4 &&
                                            x.GenericFieldLevel == (MediusWorldGenericFieldLevelType)filterMaskLevelType)
                                        .Skip((pageIndex - 1) * pageSize)
                                        .Take(pageSize);

                                    if (matchingChannelList != null)
                                        return matchingChannelList;
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException invalidOperationException)
            {
                if (invalidOperationException.Message.Contains("Sequence contains no elements"))
                    LoggerAccessor.LogWarn($"[MUM] - GetRemoteChannelListFiltered No matching channel found in any server.");
                else
                    LoggerAccessor.LogError($"[MUM] - GetRemoteChannelListFiltered thrown an InvalidOperationException: {invalidOperationException}");
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetRemoteChannelListFiltered thrown an exception: {e}");
            }

            return null;
        }

        public static IEnumerable<Channel>? GetRemoteChannelsList(int appId, int pageIndex, int pageSize, ChannelType type, IPAddress ClientIp)
        {
            try
            {
                if (MediusClass.MUMServerIPsList.Count > 0)
                {
                    foreach (string ip in MediusClass.MUMServerIPsList)
                    {
                        string? RemoteChannelsList = MumClient.GetJsonServerResult(ip, 10076, "GetChannelsJson");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<List<Channel>>? ConvertedChannelsLists = JsonConvert.DeserializeObject<List<List<Channel>>>(RemoteChannelsList, new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                            });

                            if (ConvertedChannelsLists != null)
                            {
                                for (int i = 0; i < ConvertedChannelsLists.Count; i++)
                                {
                                    IEnumerable<Channel>? matchingChannelList = ConvertedChannelsLists[i]
                                        .Where(x => x.Type == type &&
                                            x.ApplicationId == appId)
                                        .Skip((pageIndex - 1) * pageSize)
                                        .Take(pageSize);

                                    if (matchingChannelList != null)
                                        return matchingChannelList;
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException invalidOperationException)
            {
                if (invalidOperationException.Message.Contains("Sequence contains no elements"))
                    LoggerAccessor.LogWarn($"[MUM] - GetRemoteChannelsList No matching channel found in any server.");
                else
                    LoggerAccessor.LogError($"[MUM] - GetRemoteChannelsList thrown an InvalidOperationException: {invalidOperationException}");
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetRemoteChannelsList thrown an exception: {e}");
            }

            return null;
        }

        public static uint GetRemoteChannelCount(ChannelType type, int appId, IPAddress ClientIp)
        {
            try
            {
                if (MediusClass.MUMServerIPsList.Count > 0)
                {
                    foreach (string ip in MediusClass.MUMServerIPsList)
                    {
                        string? RemoteChannelsList = MumClient.GetJsonServerResult(ip, 10076, "GetChannelsJson");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<List<Channel>>? ConvertedChannelsLists = JsonConvert.DeserializeObject<List<List<Channel>>>(RemoteChannelsList, new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays
                            });

                            uint totalCount = 0;

                            if (ConvertedChannelsLists != null)
                            {
                                foreach (List<Channel> ConvertedChannelsList in ConvertedChannelsLists)
                                {
                                    // Add the count of matching channels to the total count
                                    totalCount += (uint)ConvertedChannelsList
                                        .Where(x => x.Type == type && x.ApplicationId == appId).Count();
                                }
                            }

                            return totalCount;
                        }
                    }
                }
            }
            catch (InvalidOperationException invalidOperationException)
            {
                if (invalidOperationException.Message.Contains("Sequence contains no elements"))
                    LoggerAccessor.LogWarn($"[MUM] - GetRemoteChannelCount No matching channel found in any server.");
                else
                    LoggerAccessor.LogError($"[MUM] - GetRemoteChannelCount thrown an InvalidOperationException: {invalidOperationException}");
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetRemoteChannelCount thrown an exception: {e}");
            }

            return 0;
        }
    }
}
