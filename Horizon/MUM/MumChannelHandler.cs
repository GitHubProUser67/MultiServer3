using BackendProject.Horizon.RT.Common;
using CustomLogger;
using Horizon.MEDIUS;
using Newtonsoft.Json;
using System.Net;

namespace Horizon.MUM
{
    public class MumChannelHandler
    {
        public static List<Channel> AccessibleChannels = new();

        public static string SerializeChannel(Channel channel)
        {
            return JsonConvert.SerializeObject(channel, Formatting.Indented);
        }

        public static string SerializeChannelsList()
        {
            if (AccessibleChannels.Count > 0)
                return JsonConvert.SerializeObject(AccessibleChannels, Formatting.Indented);
            else
                return "[]";
        }

        public static int GetIndexOfLocalChannelByNameAndAppId(string channelName, int AppId)
        {
            try
            {
                // Check if the AccessibleChannels list is not empty
                if (AccessibleChannels.Count > 0)
                {
                    // Find the index of the channel that matches the given name and ID
                    int index = AccessibleChannels
                        .FindIndex(channel => channel.Name == channelName && channel.ApplicationId == AppId);

                    // If a matching channel is found, return its index
                    if (index != -1)
                        return index;
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
                        string? RemoteChannelsList = GetPublicJsonConfig(ip, 10076, "GetChannels");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<Channel>? ConvertedChannelsList = JsonConvert.DeserializeObject<List<Channel>>(RemoteChannelsList);

                            if (ConvertedChannelsList != null && ConvertedChannelsList.Count > 0)
                            {
                                return ConvertedChannelsList
                                    .Where(x => x.Name == channel.Name && x.ApplicationId == channel.ApplicationId)
                                    .First();
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
                        string? RemoteChannelsList = GetPublicJsonConfig(ip, 10076, "GetChannels");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<Channel>? ConvertedChannelsList = JsonConvert.DeserializeObject<List<Channel>>(RemoteChannelsList);

                            if (ConvertedChannelsList != null && ConvertedChannelsList.Count > 0)
                            {
                                return ConvertedChannelsList
                                    .Where(channel => channel.Id == WorldId && channel.ApplicationId == Appid)
                                    .First();
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
                        string? RemoteChannelsList = GetPublicJsonConfig(ip, 10076, "GetChannels");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<Channel>? ConvertedChannelsList = JsonConvert.DeserializeObject<List<Channel>>(RemoteChannelsList);

                            if (ConvertedChannelsList != null && ConvertedChannelsList.Count > 0)
                            {
                                return ConvertedChannelsList
                                    .Where(channel => channel.ApplicationId == Appid)
                                    .OrderBy(channel => channel.PlayerCount)
                                    .First();
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
                        string? RemoteChannelsList = GetPublicJsonConfig(ip, 10076, "GetChannels");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<Channel>? ConvertedChannelsList = JsonConvert.DeserializeObject<List<Channel>>(RemoteChannelsList);

                            if (ConvertedChannelsList != null && ConvertedChannelsList.Count > 0)
                            {
                                return ConvertedChannelsList.Where(x => x.Type == type &&
                                        x.ApplicationId == appId &&
                                        x.GenericField1 == FieldMask1 &&
                                        x.GenericField2 == FieldMask2 &&
                                        x.GenericField3 == FieldMask3 &&
                                        x.GenericField4 == FieldMask4 &&
                                        x.GenericFieldLevel == (MediusWorldGenericFieldLevelType)filterMaskLevelType)
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize);
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
                        string? RemoteChannelsList = GetPublicJsonConfig(ip, 10076, "GetChannels");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<Channel>? ConvertedChannelsList = JsonConvert.DeserializeObject<List<Channel>>(RemoteChannelsList);

                            if (ConvertedChannelsList != null && ConvertedChannelsList.Count > 0)
                            {
                                return ConvertedChannelsList.Where(x => x.Type == type &&
                                        x.ApplicationId == appId)
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize);
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
                        string? RemoteChannelsList = GetPublicJsonConfig(ip, 10076, "GetChannels");
                        if (!string.IsNullOrEmpty(RemoteChannelsList))
                        {
                            List<Channel>? ConvertedChannelsList = JsonConvert.DeserializeObject<List<Channel>>(RemoteChannelsList);

                            if (ConvertedChannelsList != null && ConvertedChannelsList.Count > 0)
                                return (uint)ConvertedChannelsList.Count(x => x.Type == type && x.ApplicationId == appId);
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

        public static string? GetPublicJsonConfig(string ip, ushort port, string command)
        {
#if NET7_0
            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");
                client.DefaultRequestHeaders.Add("method", "GET");
                client.DefaultRequestHeaders.Add("content-type", "application/json; charset=UTF-8");
                HttpResponseMessage response = client.GetAsync($"http://{ip}:{port}/{command}/").Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {
                // Not Important.
            }
#else
            try
            {
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                WebClient client = new();
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");
                client.Headers.Add("method", "GET");
                client.Headers.Add("content-type", "application/json; charset=UTF-8");
                return client.DownloadStringTaskAsync(new Uri($"http://{ip}:{port}/{command}/")).Result;
#pragma warning restore
            }
            catch (Exception)
            {
                // Not Important.
            }
#endif

            return null;
        }
    }
}
