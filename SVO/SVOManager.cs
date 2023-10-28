using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Models;
using Horizon.LIBRARY.Common;
using System.Net;

namespace SVO
{
    public class SVOManager
    {
        private Dictionary<string, int[]> _appIdGroups = new Dictionary<string, int[]>();

        private List<MediusFile> _mediusFiles = new List<MediusFile>();
        private List<MediusFileMetaData> _mediusFilesToUpdateMetaData = new List<MediusFileMetaData>();
#nullable enable
        private static DateTime? _lastSuccessfulDbAuth;
#nullable disable
        public static SVOManager Manager = new();

        private static DateTime _lastComponentLog = Utils.GetHighPrecisionUtcTime();

        private static async Task TickAsync()
        {
            try
            {
                // Attempt to authenticate with the db middleware
                // We do this every 24 hours to get a fresh new token
                if (_lastSuccessfulDbAuth == null || (Utils.GetHighPrecisionUtcTime() - _lastSuccessfulDbAuth.Value).TotalHours > 24)
                {
                    if (!await SVOServerConfiguration.Database.Authenticate())
                    {
                        // Log and exit when unable to authenticate
                        LoggerAccessor.LogError($"Unable to authenticate connection to Cache Server.");
                        return;
                    }
                    else
                    {
                        _lastSuccessfulDbAuth = Utils.GetHighPrecisionUtcTime();

                        // pass to manager
                        await Manager.OnDatabaseAuthenticated();

                        #region Check Cache Server Simulated
                        if (SVOServerConfiguration.Database._settings.SimulatedMode != true)
                            LoggerAccessor.LogInfo("Connected to Cache Server");
                        else
                            LoggerAccessor.LogInfo("Connected to Cache Server (Simulated)");
                        #endregion
                    }
                }

                if ((Utils.GetHighPrecisionUtcTime() - _lastComponentLog).TotalSeconds > 15f)
                    _lastComponentLog = Utils.GetHighPrecisionUtcTime();
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError(ex);
            }
        }

        public static async Task StartTickPooling()
        {
            // iterate
            while (true)
            {
                // tick
                await TickAsync();

                await Task.Delay(100);
            }
        }

        #region MFS
        public IEnumerable<MediusFile> GetFilesList(string path, string filenameBeginsWith, uint pageSize, uint startingEntryNumber)
        {
            lock (_mediusFiles)
            {

                string[] files = null;
                int counter = 0;

                if (filenameBeginsWith.ToString() == "*")
                    files = Directory.GetFiles(path).Select(file => Path.GetFileName(file)).ToArray();
                else
                    files = Directory.GetFiles(path, Convert.ToString(filenameBeginsWith));

                if (files.Length < pageSize)
                    counter = files.Count();
                else
                    counter = (int)pageSize - 1;

                for (int i = (int)startingEntryNumber - 1; i < counter; i++)
                {
                    string fileName = files[i];
                    FileInfo fi = new FileInfo(fileName);

                    try
                    {
                        _mediusFiles.Add(new MediusFile()
                        {
                            FileName = files[i],
                            FileID = i,
                            FileSize = (int)fi.Length,
                            CreationTimeStamp = (int)fi.CreationTime.ToUnixTime(),
                        });
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogError($"MFS FileList Exception:\n{e}");
                    }
                }
                return _mediusFiles;
            }
        }

        public IEnumerable<MediusFile> GetFilesListExt(string path, string filenameBeginsWith, uint pageSize, uint startingEntryNumber)
        {
            lock (_mediusFiles)
            {
                if (startingEntryNumber == 0)
                    return _mediusFiles;

                int counter = 0;
                string filenameBeginsWithAppended = filenameBeginsWith.Remove(filenameBeginsWith.Length - 1);

                string[] filesArray = Directory.GetFiles(path);

                //files = Directory.GetFiles(path).Select(file => Path.GetFileName(filenameBeginsWith)).ToArray();

                if (filesArray.Length < pageSize)
                    counter = filesArray.Count() - 1;
                else
                    counter = (int)pageSize - 1;

                for (int i = (int)(startingEntryNumber - 1); i < counter; i++)
                {
                    //string[] pathArray = path.TrimStart('[').TrimEnd(']').Split(',');

                    //string[] fileName = filesArray[i].Split(path, path.Length, options: StringSplitOptions.None);
                    //string FileNameAppended = UsingStringJoin(fileName);

                    try
                    {
                        string fileName = filesArray[i];
                        FileInfo fi = new FileInfo(fileName);

                        _mediusFiles.Where(x => x.FileName == fileName.StartsWith(filenameBeginsWithAppended).ToString()).ToList();

                        /*
                        _mediusFiles.Add(new MediusFile()
                        {
                            FileName = filenameBeginsWithAppended.ToString(),
                            FileID = (uint)i,
                            FileSize = (uint)fi.Length,
                            CreationTimeStamp = Utils.ToUnixTime(fi.CreationTime),
                        });
                        */
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogError($"MFS FileListExt Exception:\n{e}");
                    }
                }
                return _mediusFiles;
            }
        }

        public IEnumerable<MediusFileMetaData> UpdateFileMetaData(string path, int appId, MediusFile mediusFile, MediusFileMetaData mediusFileMetaData)

        {
            lock (_mediusFilesToUpdateMetaData)
            {

                /*
                if (filename.ToString() != null)
                {
                    files = Directory.GetFiles(path).Select(file => Path.GetFileName(file)).ToArray();
                }
                else
                {
                    files = Directory.GetFiles(path, Convert.ToString(filename));
                }
                */
                try
                {
                    _mediusFilesToUpdateMetaData.Add(new MediusFileMetaData()
                    {
                        Key = mediusFileMetaData.Key,
                        Value = mediusFileMetaData.Value,
                    });
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError($"MFS UpdateMetaData Exception:\n{e}");
                }

                return _mediusFilesToUpdateMetaData;
            }
        }
        #endregion

        #region App Ids

        public async Task OnDatabaseAuthenticated()
        {
            // get supported app ids
            var appids = await SVOServerConfiguration.Database.GetAppIds();

            // build dictionary of app ids from response
            _appIdGroups = appids.ToDictionary(x => x.Name, x => x.AppIds.ToArray());
        }

        public bool IsAppIdSupported(int appId)
        {
            return _appIdGroups.Any(x => x.Value.Contains(appId));
        }

        public int[] GetAppIdsInGroup(int appId)
        {
            return _appIdGroups.FirstOrDefault(x => x.Value.Contains(appId)).Value ?? new int[0];
        }

        #endregion

        #region Misc

        public SECURITY_MODE GetServerSecurityMode(SECURITY_MODE securityMode, RSA_KEY rsaKey)
        {
            int result;

            result = (int)securityMode;

            if (securityMode == SECURITY_MODE.MODE_UNKNOWN)
            {
                //result = (KM_GetLocalPublicKey(RSA_KEY, 0x80000000, 0) != 0) + 1;

                //securityMode = (SECURITY_MODE)result;
            }


            return (SECURITY_MODE)result;
        }

        public void rt_msg_server_check_protocol_compatibility(int clientVersion, byte p_compatible)
        {



        }

        #region AnonymouseAccountIdGenerator
        /// <summary>
        /// Generates a Random Anonymous AccountID for MediusAnonymouseAccountRequest <br></br>
        /// Or if one doesn't exist in Database
        /// </summary>
        /// <param name="AnonymousIDRangeSeed">Config Value for changing the MAS</param>
        /// <returns></returns>
        public int AnonymousAccountIDGenerator(int AnonymousIDRangeSeed)
        {
            int result; // eax

            //for integers
            Random r = new Random();
            int rInt = r.Next(-80000000, 0);

            result = rInt;
            return result;
        }
        #endregion

        public string UsingStringJoin(string[] array)
        {
            return string.Join(string.Empty, array);
        }

        #endregion

        #region DmeServerClient

        public Task DmeServerClientIpQuery(int WorldID, int TargetClient, IPAddress IP)
        {

            return Task.CompletedTask;
        }

        public DME_SERVER_RESULT DmeServerMapRtError(uint RtError)
        {
            DME_SERVER_RESULT result;
            if (RtError == 52518)
            {
                result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_MSG_TYPE;
                return result;
            }

            if (RtError > 0xCD26)
            {
                result = DME_SERVER_RESULT.DME_SERVER_MUTEX_ERROR;
                if (RtError != 52528)
                {
                    if (RtError > 0xCD30)
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_UNSECURED_ERROR;
                        if (RtError != 52533)
                        {
                            if (RtError > 0xCD35)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_CONFIG_ERROR;
                                if (RtError != 52535)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_BUFF_OVERFLOW_ERROR;
                                    if (RtError >= 0xCD37)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_PARTIAL_RW_ERROR;
                                        if (RtError != 52536)
                                        {
                                            result = DME_SERVER_RESULT.DME_SERVER_CLIENT_ALREADY_DISCONNECTED;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //if (RtError == 52530)
                                //return 30;
                                result = DME_SERVER_RESULT.DME_SERVER_NO_MORE_WORLDS;
                                if (RtError >= 0xCD32)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_CLIENT_LIMIT;
                                    if (RtError != 52531)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_ENCRYPTED_ERROR;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_MSG_TOO_BIG;
                        if (RtError != 52523)
                        {
                            if (RtError > 0xCD2B)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_PARTIAL_WRITE;
                                if (RtError != 52525)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_MSG_TYPE;
                                    if (RtError >= 0xCD2D)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_RESET_ERROR;
                                        if (RtError != 52526)
                                        {
                                            result = DME_SERVER_RESULT.DME_SERVER_CIRC_BUF_ERROR;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_TCP_GET_WORLD_INDEX;
                                if (RtError != 52520)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_WOULD_BLOCK;
                                    if (RtError >= 0xCD28)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_READ_ERROR;
                                        if (RtError != 52521)
                                        {
                                            result = DME_SERVER_RESULT.DME_SERVER_SOCKET_CLOSED;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (RtError == 52508)
                {
                    result = DME_SERVER_RESULT.DME_SERVER_CONN_MSG_ERROR;
                    return result;
                }
                if (RtError > 0xCD1C)
                {
                    result = DME_SERVER_RESULT.DME_SERVER_SOCKET_BIND_ERROR;
                    if (RtError != 52513)
                    {
                        if (RtError > 0xCD21)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_SOCKET_LISTEN_ERROR;
                            if (RtError != 52515)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_SOCKET_POLL_ERROR;
                                if (RtError >= 0xCD23)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_SOCKET_READ_ERROR;
                                    if (RtError != 52516)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_WRITE_ERROR;
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_STACK_LOAD_ERROR;
                            if (RtError != 52510)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_WORLD_FULL;
                                if (RtError >= 0xCD1E)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_SOCKET_CREATE_ERROR;
                                    if (RtError != 52511)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_OPT_ERROR;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (RtError == 52503)
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_NOT_INITIALIZED;
                        return result;
                    }

                    if (RtError <= 0xCD17)
                    {
                        if (RtError == 52501)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_INVALID_PARAM;
                            return result;
                        }
                        if (RtError > 0xCD15)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_NOT_IMPLEMENTED;
                            return result;
                        }
                    }

                    result = DME_SERVER_RESULT.DME_SERVER_MEM_ALLOC;
                    if (RtError != 52505)
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_RESULT;
                        if (RtError >= 0xCD19)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_SOCKET_LIMIT;
                            if (RtError != 52506)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_CONN_ERROR;
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
