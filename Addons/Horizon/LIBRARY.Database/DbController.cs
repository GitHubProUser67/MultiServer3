using Newtonsoft.Json;
using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.RT.Models;
using MultiServer.Addons.Horizon.LIBRARY.Common;
using MultiServer.Addons.Horizon.LIBRARY.Database.Config;
using MultiServer.Addons.Horizon.LIBRARY.Database.Models;
using System.Text;
using System.Web;

namespace MultiServer.Addons.Horizon.LIBRARY.Database
{
    public class DbController
    {
        public DbSettings _settings = new DbSettings();

        private int _simulatedAccountIdCounter = 1;
        private int _simulatedClanIdCounter = 1;
        private int _simulatedClanMessageIdCounter = 1;
        private int _simulatedClanInvitationIdCounter = 1;
        private string _dbAccessToken = null;
        private string _dbAccountName = null;

        private List<AccountDTO> _simulatedAccounts = new List<AccountDTO>();
        private List<AccountRelationInviteDTO> _simulatedBuddyInvitations = new List<AccountRelationInviteDTO>();
        private List<NpIdDTO> _simulatedNpIdAccounts = new List<NpIdDTO>();
        private List<ClanDTO> _simulatedClans = new List<ClanDTO>();
        private List<MatchmakingSupersetDTO> _simulatedMatchmakingSupersets = new List<MatchmakingSupersetDTO>();
        private List<FileDTO> _simulatedMediusFiles = new List<FileDTO>();
        private List<FileMetaDataDTO> _simulatedFileMetaData = new List<FileMetaDataDTO>();
        private List<FileAttributesDTO> _simulatedFileAttributes = new List<FileAttributesDTO>();

        public DbController(string configFile)
        {
            if (configFile != null)
            {
#pragma warning disable
                Directory.CreateDirectory(Path.GetDirectoryName(configFile));
#pragma warning restore

                if (File.Exists(configFile))
                {
                    // Populate existing object
                    try { JsonConvert.PopulateObject(File.ReadAllText(configFile), _settings); }
                    catch (Exception ex) { ServerConfiguration.LogError(ex); }
                }
                else
                {
                    File.WriteAllText(configFile, JsonConvert.SerializeObject(_settings));
                    // Populate existing object
                    try { JsonConvert.PopulateObject(File.ReadAllText(configFile), _settings); }
                    catch (Exception ex) { ServerConfiguration.LogError(ex); }
                }
            }
        }

        #region Sub Classes
        public class IpBan
        {
            public string IpAddress { get; set; }
        }

        public class MacBan
        {
            public string MacAddress { get; set; }
        }
        #endregion

        /// <summary>
        /// Authenticate with middleware.
        /// </summary>
        public async Task<bool> Authenticate()
        {
            // Succeed in simulated mode#region account
            if (_settings.SimulatedMode)
                return true;

            var response = await Authenticate(_settings.DatabaseUsername, _settings.DatabasePassword);

            // Validate
            if (response == null || response.Roles == null || !response.Roles.Contains("database"))
                return false;

            _dbAccountName = response.AccountName;
            _dbAccessToken = response.Token;

            return !string.IsNullOrEmpty(_dbAccessToken);
        }

        public bool AmIAuthenticated()
        {
            if (_settings.SimulatedMode)
                return true;

            return !string.IsNullOrEmpty(_dbAccessToken);
        }

        public string GetUsername()
        {
            if (_settings.SimulatedMode)
                return _settings.DatabaseUsername;

            return _dbAccountName;
        }

        #region Account

        public async Task<string> GetPlayerList()
        {
            string results = null;

            try
            {
                if (_settings.SimulatedMode) // Deprecated
                    return "[]";
                else
                {
                    HttpResponseMessage Resp = await GetDbAsync($"Account/getOnlineAccounts");
                    results = await Resp.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return results;
        }

        /// <summary>
        /// Get account by name.
        /// </summary>
        /// <param name="name">Case insensitive name of player.</param>
        /// <returns>Returns account.</returns>
        public async Task<AccountDTO> GetAccountByName(string name, int appId)
        {
            AccountDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    /*
                    //Simulated Resistance 2 beta Account for Matchmaking
                    AccountDTO R2PuBeta;
                    _simulatedAccounts.Add(R2PuBeta = new AccountDTO()
                    {
                        AccountId = 2,
                        AccountName = "gameRecorder_r2_pubeta_master",
                        AccountPassword = "",
                        AccountWideStats = new int[Constants.LADDERSTATSWIDE_MAXLEN],
                        AccountCustomWideStats = new int[1000],
                        AppId = 21731,
                        MachineId = "",
                        MediusStats = "",
                        Friends = new AccountRelationDTO[0],
                        Ignored = new AccountRelationDTO[0],
                        IsBanned = false
                    });
                    */

                    AccountDTO ftb3Mod;
                    _simulatedAccounts.Add(ftb3Mod = new AccountDTO()
                    {
                        AccountId = 2,
                        AccountName = "ftb3 Moderator_0",
                        AccountPassword = "",
                        AccountWideStats = new int[Constants.LADDERSTATSWIDE_MAXLEN],
                        AccountCustomWideStats = new int[1000],
                        AppId = 21694,
                        MachineId = "",
                        MediusStats = "",
                        Friends = new AccountRelationDTO[0],
                        Ignored = new AccountRelationDTO[0],
                        IsBanned = false
                    });

                    result = _simulatedAccounts.FirstOrDefault(x => x.AppId == appId && x.AccountName.ToLower() == name.ToLower());
                }
                else
                {
                    name = HttpUtility.UrlEncode(name);
                    string route = $"Account/searchAccountByName?AccountName={name}&AppId={appId}";
                    result = await GetDbAsync<AccountDTO>(route);
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get account by id.
        /// </summary>
        /// <param name="id">Id of player.</param>
        /// <returns>Returns account.</returns>
        public async Task<AccountDTO> GetAccountById(int id)
        {
            AccountDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                    result = _simulatedAccounts.FirstOrDefault(x => x.AccountId == id);
                else
                    result = await GetDbAsync<AccountDTO>($"Account/getAccount?AccountId={id}");
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Creates an account.
        /// </summary>
        /// <param name="createAccount">Account creation parameters.</param>
        /// <returns>Returns created account.</returns>
        public async Task<AccountDTO> CreateAccount(CreateAccountDTO createAccount)
        {
            AccountDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var checkExisting = await GetAccountByName(createAccount.AccountName, createAccount.AppId);
                    if (checkExisting == null)
                    {
                        _simulatedAccounts.Add(result = new AccountDTO()
                        {
                            AccountId = _simulatedAccountIdCounter++,
                            AccountName = createAccount.AccountName,
                            AccountPassword = createAccount.AccountPassword,
                            AccountWideStats = new int[Constants.LADDERSTATSWIDE_MAXLEN],
                            AccountCustomWideStats = new int[1000],
                            AppId = createAccount.AppId,
                            MachineId = createAccount.MachineId,
                            MediusStats = createAccount.MediusStats,
                            Friends = new AccountRelationDTO[0],
                            Ignored = new AccountRelationDTO[0],
                            IsBanned = false
                        });
                    }
                    else
                        ServerConfiguration.LogError($"Account creation failed account name already exists!");
                }
                else
                {
                    var response = await PostDbAsync($"Account/createAccount", JsonConvert.SerializeObject(createAccount));

                    // Deserialize on success
                    if (response.IsSuccessStatusCode)
                        result = JsonConvert.DeserializeObject<AccountDTO>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Delete account by name.
        /// </summary>
        /// <param name="accountName">Case insensitive name of account.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> DeleteAccount(string accountName, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = _simulatedAccounts.RemoveAll(x => x.AccountName.ToLower() == accountName.ToLower() && x.AppId == appId) > 0;
                else
                    result = (await GetDbAsync($"Account/deleteAccount?AccountName={accountName}&AppId={appId}")).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }
            return result;
        }

        public async Task<bool> PostAccountUpdatePassword(int accountId, string oldPassword, string newPassword)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await GetDbAsync($"Account/updateAccountPassword?accountId={accountId}&oldPassowrd={oldPassword}&newPassword={newPassword}")).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts the account sign in date to the database.
        /// </summary>
        /// <param name="accountId">Id to post login date to.</param>
        /// <param name="time">Time logged in.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostAccountSignInDate(int accountId, DateTime time)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await PostDbAsync($"Account/postAccountSignInDate?AccountId={accountId}", $"\"{time.ToUniversalTime()}\"")).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get account status by account id.
        /// </summary>
        /// <param name="accountId">Unique id of account.</param>
        /// <returns>Account status.</returns>
        public async Task<AccountStatusDTO> GetAccountStatus(int accountId)
        {
            AccountStatusDTO result = null;

            try
            {
                if (!_settings.SimulatedMode)
                    result = await GetDbAsync<AccountStatusDTO>($"Account/getAccountStatus?AccountId={accountId}");
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts the current account status.
        /// </summary>
        /// <param name="status">Account status.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostAccountStatus(AccountStatusDTO status)
        {
            bool result = false;

            try
            {
                if (!_settings.SimulatedMode)
                    result = (await PostDbAsync($"Account/postAccountStatusUpdates", JsonConvert.SerializeObject(status))).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        public async Task<bool> ClearAccountStatuses()
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await PostDbAsync($"Account/clearAccountStatuses", null)).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get account metadata by account id.
        /// </summary>
        /// <param name="accountId">Unique id of account.</param>
        /// <returns>Account metadata.</returns>
        public async Task<string> GetAccountMetadata(int accountId)
        {
            string result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = _simulatedAccounts.FirstOrDefault(x => x.AccountId == accountId);
                    result = account?.Metadata;
                }
                else
                    result = await GetDbAsync<string>($"Account/getAccountMetadata?accountId={accountId}");
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts the given metadata to the given account.
        /// </summary>
        /// <param name="accountId">Id of account.</param>
        /// <param name="metadata">Metadata to post.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> PostAccountMetadata(int accountId, string metadata)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = _simulatedAccounts.FirstOrDefault(x => x.AccountId == accountId);
                    if (account != null)
                    {
                        account.Metadata = metadata;
                        result = true;
                    }
                    else
                        result = false;
                }
                else
                    result = (await PostDbAsync($"Account/postAccountMetadata?accountId={accountId}", JsonConvert.SerializeObject(metadata))).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts ip to account.
        /// </summary>
        public async Task<bool> PostAccountIp(int accountId, string ip)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await PostDbAsync($"Account/postAccountIp?AccountId={accountId}", $"\"{ip}\"")).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Gets the total number of active players by app id.
        /// </summary>
        /// <param name="appId">App Id to filter total active accounts by.</param>
        /// <returns>Number of active accounts or null.</returns>
        public async Task<int?> GetActiveAccountCountByAppId(int appId)
        {
            int? result = null;

            try
            {
                if (_settings.SimulatedMode)
                    result = _simulatedAccounts.Count;
                else
                {
                    var response = await GetDbAsync($"Account/getActiveAccountCountByAppId?AppId={appId}");

                    // Deserialize on success
                    if (response.IsSuccessStatusCode && int.TryParse(await response.Content.ReadAsStringAsync(), out int r))
                        result = r;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Gets the total number of active clans by app id.
        /// </summary>
        /// <param name="appId">App Id to filter total active clans by.</param>
        /// <returns>Number of active clans or null.</returns>
        public async Task<int?> GetActiveClanCountByAppId(int appId)
        {
            int? result = null;

            try
            {
                if (_settings.SimulatedMode)
                    result = _simulatedClans.Count(x => !x.IsDisbanded);
                else
                {
                    var response = await GetDbAsync($"Clan/getActiveClanCountByAppId?AppId={appId}");

                    // Deserialize on success
                    if (response.IsSuccessStatusCode && int.TryParse(await response.Content.ReadAsStringAsync(), out int r))
                        result = r;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }



        /// <summary>
        /// Gets whether or not the ip is banned.
        /// </summary>
        public async Task<bool> GetIsIpBanned(string ip)
        {
            bool result = false;

            try
            {
                if (!_settings.SimulatedMode)
                {
                    IpBan IpBanArray = new IpBan
                    {
                        IpAddress = ip
                    };
                    System.Text.Json.JsonSerializer.Serialize(IpBanArray);
                    result = await PostDbAsync<bool>($"Account/getIpIsBanned", IpBanArray);
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Gets whether the mac address is banned.
        /// </summary>
        /// <param name="mac">MAC Address as a Base64 string</param>
        public async Task<bool> GetIsMacBanned(string mac)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = false;
                else
                {
                    MacBan MacBanArray = new MacBan
                    {
                        MacAddress = mac
                    };
                    System.Text.Json.JsonSerializer.Serialize(MacBanArray);
                    result = await PostDbAsync<bool>($"Account/getMacIsBanned", MacBanArray);
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts the given machine id to the database account with the given account id.
        /// </summary>
        /// <param name="accountId">Account id.</param>
        /// <param name="machineId">Machine id.</param>
        public async Task<bool> PostMachineId(int accountId, string machineId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = false;
                else
                    result = (await PostDbAsync($"Account/postMachineId?AccountId={accountId}", $"\"{machineId}\"")).IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        #endregion

        #region Buddy / Ignored

        /// <summary>
        /// Add buddy to buddy list.
        /// </summary>
        /// <param name="addBuddy">Add buddy parameters.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> addBuddyInvitation(AccountRelationInviteDTO addBuddyInvite)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    _simulatedBuddyInvitations.Add(addBuddyInvite);

                    result = true;
                }
                else
                    result = (await PostDbAsync($"Buddy/addBuddyInvitation", JsonConvert.SerializeObject(addBuddyInvite))).IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Add buddy to buddy list.
        /// </summary>
        /// <param name="addBuddy">Add buddy parameters.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> deleteBuddyInvitation(AccountRelationInviteDTO addBuddyInvite)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    _simulatedBuddyInvitations.Remove(addBuddyInvite);

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Buddy/deleteBuddyInvitation", JsonConvert.SerializeObject(addBuddyInvite))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Retrieve pending buddy invites to buddy list.
        /// </summary>
        /// <param name="addBuddy">Add buddy parameters.</param>
        /// <returns>Success or failure.</returns>
        public async Task<List<AccountRelationInviteDTO>> retrieveBuddyInvitations(int appId, int accountId)
        {
            List<AccountRelationInviteDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(accountId);
                    if (account != null)
                    {
                        result = _simulatedBuddyInvitations.Where(x => x.AppId == appId).ToList();
                    }
                }
                else
                {
                    result = await GetDbAsync<List<AccountRelationInviteDTO>>($"Buddy/retrieveBuddyInvitations?appId={appId}&accountId={accountId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Add buddy to buddy list.
        /// </summary>
        /// <param name="addBuddy">Add buddy parameters.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> AddBuddy(BuddyDTO addBuddy)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(addBuddy.AccountId);
                    var buddyAccount = await GetAccountById(addBuddy.BuddyAccountId);
                    if (account != null && buddyAccount != null)
                    {
                        var friends = account.Friends;
                        Array.Resize(ref friends, account.Friends.Length + 1);
                        friends[friends.Length - 1] = new AccountRelationDTO()
                        {
                            AccountId = buddyAccount.AccountId,
                            AccountName = buddyAccount.AccountName
                        };
                        account.Friends = friends;
                        result = true;
                    }
                }
                else
                {
                    result = (await PostDbAsync($"Buddy/addBuddy", JsonConvert.SerializeObject(addBuddy))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Remove buddy from buddy list.
        /// </summary>
        /// <param name="removeBuddy">Remove buddy parameters.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> RemoveBuddy(BuddyDTO removeBuddy)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(removeBuddy.AccountId);
                    var buddyAccount = await GetAccountById(removeBuddy.BuddyAccountId);
                    if (account != null && buddyAccount != null)
                    {
                        var newFriends = new List<AccountRelationDTO>();
                        foreach (var friend in account.Friends)
                        {
                            if (friend.AccountId == buddyAccount.AccountId)
                                continue;

                            newFriends.Add(friend);
                        }
                        account.Friends = newFriends.ToArray();
                        result = true;
                    }
                }
                else
                {
                    result = (await PostDbAsync($"Buddy/removeBuddy", JsonConvert.SerializeObject(removeBuddy))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Add player to ignored list.
        /// </summary>
        /// <param name="addIgnored">Add ignored parameters.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> AddIgnored(IgnoredDTO addIgnored)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(addIgnored.AccountId);
                    var ignoreAccount = await GetAccountById(addIgnored.IgnoredAccountId);
                    if (account != null && ignoreAccount != null)
                    {
                        var ignored = account.Ignored;
                        Array.Resize(ref ignored, account.Ignored.Length + 1);
                        ignored[ignored.Length - 1] = new AccountRelationDTO()
                        {
                            AccountId = ignoreAccount.AccountId,
                            AccountName = ignoreAccount.AccountName
                        };
                        account.Ignored = ignored;
                        result = true;
                    }
                }
                else
                {
                    result = (await PostDbAsync($"Buddy/addIgnored", JsonConvert.SerializeObject(addIgnored))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Remove player from ignored list.
        /// </summary>
        /// <param name="removeIgnored">Remove ignored parameters.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> RemoveIgnored(IgnoredDTO removeIgnored)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(removeIgnored.AccountId);
                    var ignoreAccount = await GetAccountById(removeIgnored.IgnoredAccountId);
                    if (account != null && ignoreAccount != null)
                    {
                        var newIgnored = new List<AccountRelationDTO>();
                        foreach (var ignored in account.Ignored)
                        {
                            if (ignored.AccountId == ignoreAccount.AccountId)
                                continue;

                            newIgnored.Add(ignored);
                        }
                        account.Ignored = newIgnored.ToArray();
                        result = true;
                    }
                }
                else
                {
                    result = (await PostDbAsync($"Buddy/removeIgnored", JsonConvert.SerializeObject(removeIgnored))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }


        #endregion

        #region Stats

        /// <summary>
        /// Get player wide stats.
        /// </summary>
        /// <param name="accountId">Account id of player.</param>
        /// <returns></returns>
        public async Task<StatPostDTO> GetPlayerWideStats(int accountId)
        {
            StatPostDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var stats = _simulatedAccounts.FirstOrDefault(x => x.AccountId == accountId)?.AccountWideStats;
                    if (stats != null)
                    {
                        result = new StatPostDTO()
                        {
                            AccountId = accountId,
                            Stats = stats
                        };
                    }
                }
                else
                    result = await GetDbAsync<StatPostDTO>($"Stats/getStats?AccountId={accountId}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Get clan wide stats.
        /// </summary>
        /// <param name="accountId">Clan id of clan.</param>
        /// <returns></returns>
        public async Task<ClanStatPostDTO> GetClanWideStats(int clanId)
        {
            ClanStatPostDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var stats = _simulatedClans.FirstOrDefault(x => x.ClanId == clanId)?.ClanWideStats;
                    if (stats != null)
                    {
                        result = new ClanStatPostDTO()
                        {
                            ClanId = clanId,
                            Stats = stats
                        };
                    }
                }
                else
                {
                    result = await GetDbAsync<ClanStatPostDTO>($"Stats/getClanStats?ClanId={clanId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get player ranking in a given leaderboard.
        /// </summary>
        /// <param name="accountId">Account id of player.</param>
        /// <param name="statId">Index of stat. Starts at 1.</param>
        /// <returns>Leaderboard result for player.</returns>
        public async Task<LeaderboardDTO> GetPlayerLeaderboard(int accountId)
        {
            LeaderboardDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(accountId);
                    if (account == null)
                        return null;

                    return new LeaderboardDTO()
                    {
                        AccountId = accountId,
                        AccountName = account.AccountName,
                        Index = 1,
                        MediusStats = account.MediusStats,
                        TotalRankedAccounts = 1
                    };
                }
                else
                {
                    result = await GetDbAsync<LeaderboardDTO>($"Stats/getPlayerLeaderboard?AccountId={accountId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get player ranking in a given leaderboard.
        /// </summary>
        /// <param name="accountId">Account id of player.</param>
        /// <param name="statId">Index of stat. Starts at 1.</param>
        /// <returns>Leaderboard result for player.</returns>
        public async Task<LeaderboardDTO> GetPlayerLeaderboardIndex(int accountId, int statId, int appId)
        {
            LeaderboardDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(accountId);
                    if (account == null)
                        return null;

                    return new LeaderboardDTO()
                    {
                        AccountId = accountId,
                        AccountName = account.AccountName,
                        Index = 1,
                        MediusStats = account.MediusStats,
                        StatValue = account.AccountWideStats[statId - 1],
                        TotalRankedAccounts = 1
                    };
                }
                else
                {
                    result = await GetDbAsync<LeaderboardDTO>($"Stats/getPlayerLeaderboardIndex?AccountId={accountId}&StatId={statId}&AppId={appId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get clan ranking in a given leaderboard.
        /// </summary>
        /// <param name="clanId">Clan id of clan.</param>
        /// <param name="statId">Index of stat. Starts at 1.</param>
        /// <returns>Leaderboard result for clan.</returns>
        public async Task<ClanLeaderboardDTO> GetClanLeaderboardIndex(int clanId, int statId, int appId)
        {
            ClanLeaderboardDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null)
                        return null;

                    var ordered = _simulatedClans.Where(x => !x.IsDisbanded).OrderByDescending(x => x.ClanWideStats[statId]).ToList();
                    return new ClanLeaderboardDTO()
                    {
                        ClanId = clan.ClanId,
                        ClanName = clan.ClanName,
                        Index = ordered.FindIndex(0, ordered.Count, x => x.ClanId == clanId),
                        MediusStats = clan.ClanMediusStats,
                        StatValue = clan.ClanWideStats[statId],
                        TotalRankedClans = _simulatedClans.Count(x => !x.IsDisbanded)
                    };
                }
                else
                {
                    result = await GetDbAsync<ClanLeaderboardDTO>($"Stats/getClanLeaderboardIndex?ClanId={clanId}&StatId={statId + 1}&AppId={appId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get clan leaderboard for a given stat by page and size.
        /// </summary>
        /// <param name="statId">Stat id. Starts at 1.</param>
        /// <param name="startIndex">Position to start gathering results from. Starts at 0.</param>
        /// <param name="size">Max number of items to retrieve.</param>
        /// <returns>Collection of leaderboard results for each player in page.</returns>
        public async Task<ClanLeaderboardDTO[]> GetClanLeaderboard(int statId, int startIndex, int size, int appId)
        {
            ClanLeaderboardDTO[] result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var ordered = _simulatedClans.Where(x => x.AppId == appId).Where(x => !x.IsDisbanded).OrderByDescending(x => x.ClanWideStats[statId]).Skip(startIndex).Take(size).ToList();
                    result = ordered.Select(x => new ClanLeaderboardDTO()
                    {
                        ClanId = x.ClanId,
                        ClanName = x.ClanName,
                        MediusStats = x.ClanMediusStats,
                        StatValue = x.ClanWideStats[statId],
                        TotalRankedClans = _simulatedClans.Count(y => !y.IsDisbanded),
                        Index = startIndex + ordered.IndexOf(x)
                    }).ToArray();
                }
                else
                {
                    result = await GetDbAsync<ClanLeaderboardDTO[]>($"Stats/getClanLeaderboard?StatId={statId + 1}&StartIndex={startIndex}&Size={size}&AppId={appId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get leaderboard for a given stat by page and size.
        /// </summary>
        /// <param name="statId">Stat id. Starts at 1.</param>
        /// <param name="startIndex">Position to start gathering results from. Starts at 0.</param>
        /// <param name="size">Max number of items to retrieve.</param>
        /// <returns>Collection of leaderboard results for each player in page.</returns>
        public async Task<LeaderboardDTO[]> GetLeaderboard(int statId, int startIndex, int size, int appId)
        {
            LeaderboardDTO[] result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var ordered = _simulatedAccounts.Where(x => x.AppId == appId).OrderByDescending(x => x.AccountWideStats[statId]).Skip(startIndex).Take(size).ToList();
                    result = ordered.Select(x => new LeaderboardDTO()
                    {
                        AccountId = x.AccountId,
                        AccountName = x.AccountName,
                        MediusStats = x.MediusStats,
                        StatValue = x.AccountWideStats[statId],
                        TotalRankedAccounts = 0,
                        Index = startIndex + ordered.IndexOf(x)
                    }).ToArray();
                }
                else
                {
                    result = await GetDbAsync<LeaderboardDTO[]>($"Stats/getLeaderboard?StatId={statId}&StartIndex={startIndex}&Size={size}&AppId={appId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get leaderboard for a given stat by page and size.
        /// </summary>
        /// <param name="startIndex">Position to start gathering results from. Starts at 0.</param>
        /// <param name="size">Max number of items to retrieve.</param>
        /// <returns>Collection of leaderboard results for each player in page.</returns>
        public async Task<LeaderboardDTO[]> GetLeaderboardList(int startIndex, int size, int appId)
        {
            LeaderboardDTO[] result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var ordered = _simulatedAccounts.Where(x => x.AppId == appId).OrderByDescending(x => x.AccountWideStats[0]).Skip(startIndex).Take(size).ToList();
                    result = ordered.Select(x => new LeaderboardDTO()
                    {
                        AccountId = x.AccountId,
                        AccountName = x.AccountName,
                        MediusStats = x.MediusStats,
                        TotalRankedAccounts = 0,
                        Index = startIndex + ordered.IndexOf(x)
                    }).ToArray();
                }
                else
                {
                    result = await GetDbAsync<LeaderboardDTO[]>($"Stats/getLeaderboard?Size={size}&AppId={appId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts ladder stats to account id.
        /// </summary>
        /// <param name="statPost">Model containing account id and ladder stats collection.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostAccountLadderStats(StatPostDTO statPost)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(statPost.AccountId);
                    if (account == null)
                        return false;

                    account.AccountWideStats = statPost.Stats;
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Stats/postStats", JsonConvert.SerializeObject(statPost))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts custom ladder stats to account id.
        /// </summary>
        /// <param name="statPost">Model containing account id and ladder stats collection.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostAccountLadderCustomStats(StatPostDTO statPost)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(statPost.AccountId);
                    if (account == null)
                        return false;

                    account.AccountCustomWideStats = statPost.Stats;
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Stats/postStatsCustom", JsonConvert.SerializeObject(statPost))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts ladder stats to clan id.
        /// </summary>
        /// <param name="statPost">Model containing clan id and ladder stats collection.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostClanLadderStats(int accountId, int? clanId, int[] stats, int appId)
        {
            bool result = false;
            if (!clanId.HasValue)
                return false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(accountId);
                    if (account.ClanId != clanId)
                        return false;

                    var clan = await GetClanById(account.ClanId.Value, appId);
                    if (clan == null)
                        return false;

                    clan.ClanWideStats = stats;
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Stats/postClanStats", JsonConvert.SerializeObject(new ClanStatPostDTO()
                    {
                        ClanId = clanId.Value,
                        Stats = stats
                    }))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Posts custom ladder stats to clan id.
        /// </summary>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostClanLadderCustomStats(int accountId, int? clanId, int[] stats, int appId)
        {
            bool result = false;
            if (!clanId.HasValue)
                return false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(accountId);
                    if (account.ClanId != clanId)
                        return false;

                    var clan = await GetClanById(account.ClanId.Value, appId);
                    if (clan == null)
                        return false;

                    clan.ClanCustomWideStats = stats;
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Stats/postClanStatsCustom", JsonConvert.SerializeObject(new ClanStatPostDTO()
                    {
                        ClanId = clanId.Value,
                        Stats = stats
                    }))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Post medius stats to account.
        /// </summary>
        /// <param name="accountId">Account id to post stats to.</param>
        /// <param name="stats">Stats to post encoded as a Base64 string.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostMediusStats(int accountId, string stats)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var account = await GetAccountById(accountId);
                    if (account == null)
                        return false;

                    account.MediusStats = stats;
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Account/postMediusStats?AccountId={accountId}", $"\"{stats}\""))?.IsSuccessStatusCode ?? false;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Post medius stats to clan.
        /// </summary>
        /// <param name="clanId">Clan id to post stats to.</param>
        /// <param name="stats">Stats to post encoded as a Base64 string.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> PostClanMediusStats(int clanId, string stats, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null)
                        return false;

                    clan.ClanMediusStats = stats;
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/postClanMediusStats?ClanId={clanId}", $"\"{stats}\""))?.IsSuccessStatusCode ?? false;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        #endregion

        #region Clan

        /// <summary>
        /// Get clan by name.
        /// </summary>
        /// <param name="name">Case insensitive name of clan.</param>
        /// <returns>Returns clan.</returns>
        public async Task<ClanDTO> GetClanByName(string name, int appId)
        {
            ClanDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                    result = _simulatedClans.FirstOrDefault(x => x.AppId == appId && x.ClanName.ToLower() == name.ToLower());
                else
                    result = await GetDbAsync<ClanDTO>($"Clan/searchClanByName?clanName={name}&appId={appId}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Get clan by id.
        /// </summary>
        /// <param name="id">Id of clan.</param>
        /// <returns>Returns clan.</returns>
        public async Task<ClanDTO> GetClanById(int id, int appId)
        {
            ClanDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    result = _simulatedClans.FirstOrDefault(x => x.AppId == appId && x.ClanId == id);
                    /*
                    _simulatedClans.Add(result = new ClanDTO()
                    {
                        ClanId = _simulatedClanIdCounter++,
                        ClanName = "RTIME GROUP",
                        ClanLeaderAccount = creatorAccount,
                        ClanMemberAccounts = new List<AccountDTO>(new AccountDTO[] { creatorAccount }),
                        ClanMemberInvitations = new List<ClanInvitationDTO>(),
                        ClanMessages = new List<ClanMessageDTO>(),
                        ClanMediusStats = Convert.ToBase64String(new byte[Constants.CLANSTATS_MAXLEN]),
                        ClanWideStats = new int[Constants.LADDERSTATSWIDE_MAXLEN],
                        AppId = 1
                    });
                    result.ClanMediusStats = "1:2:3";
                    creatorAccount.ClanId = result.ClanId;
                    */

                }
                else
                {
                    result = await GetDbAsync<ClanDTO>($"Clan/getClan?clanId={id}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Get clan by id.
        /// </summary>
        /// <param name="id">Id of clan.</param>
        /// <returns>Returns clan.</returns>
        public async Task<List<ClanDTO>> GetClans(int appId)
        {
            List<ClanDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    result = _simulatedClans.Where(x => x.AppId == appId).ToList();
                    /*
                    _simulatedClans.Add(result = new ClanDTO()
                    {
                        ClanId = _simulatedClanIdCounter++,
                        ClanName = "RTIME GROUP",
                        ClanLeaderAccount = creatorAccount,
                        ClanMemberAccounts = new List<AccountDTO>(new AccountDTO[] { creatorAccount }),
                        ClanMemberInvitations = new List<ClanInvitationDTO>(),
                        ClanMessages = new List<ClanMessageDTO>(),
                        ClanMediusStats = Convert.ToBase64String(new byte[Constants.CLANSTATS_MAXLEN]),
                        ClanWideStats = new int[Constants.LADDERSTATSWIDE_MAXLEN],
                        AppId = 1
                    });
                    result.ClanMediusStats = "1:2:3";
                    creatorAccount.ClanId = result.ClanId;
                    */

                }
                else
                {
                    result = await GetDbAsync<List<ClanDTO>>($"Clan/getClans?appId={appId}");
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Creates a clan.
        /// </summary>
        /// <param name="createClan">Clan creation parameters.</param>
        /// <returns>Returns created clan.</returns>
        public async Task<ClanDTO> CreateClan(int creatorAccountId, string clanName, int appId, string mediusStats)
        {
            ClanDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var checkExisting = await GetClanByName(clanName, appId);
                    if (checkExisting == null)
                    {
                        var creatorAccount = await GetAccountById(creatorAccountId);
                        _simulatedClans.Add(result = new ClanDTO()
                        {
                            ClanId = _simulatedClanIdCounter++,
                            ClanName = clanName,
                            ClanLeaderAccount = creatorAccount,
                            ClanMemberAccounts = new List<AccountDTO>(new AccountDTO[] { creatorAccount }),
                            ClanMemberInvitations = new List<ClanInvitationDTO>(),
                            ClanMessages = new List<ClanMessageDTO>(),
                            ClanMediusStats = Convert.ToBase64String(new byte[Constants.CLANSTATS_MAXLEN]),
                            ClanStats = new int[Constants.LADDERSTATSWIDE_MAXLEN],
                            ClanWideStats = new int[Constants.LADDERSTATSWIDE_MAXLEN],
                            AppId = appId
                        });

                        creatorAccount.ClanId = result.ClanId;
                    }
                    else
                        ServerConfiguration.LogError($"Clan creation failed clan name already exists!");
                }
                else
                {
                    var response = await PostDbAsync($"Clan/createClan?accountId={creatorAccountId}&clanName={clanName}&appId={appId}&mediusStats={mediusStats}", null);

                    // Deserialize on success
                    if (response.IsSuccessStatusCode)
                        result = JsonConvert.DeserializeObject<ClanDTO>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            { 
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Delete clan by id.
        /// </summary>
        /// <param name="clanId">Id of clan.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> DeleteClan(int accountId, int clanId, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null || clan.ClanLeaderAccount.AccountId != accountId)
                        return false;

                    // remove members
                    foreach (var member in clan.ClanMemberAccounts)
                        member.ClanId = null;

                    // revoke invitations
                    foreach (var inv in clan.ClanMemberInvitations)
                        inv.ResponseStatus = 3;

                    // remove
                    return _simulatedClans.Remove(clan);
                }
                else
                {
                    result = (await GetDbAsync($"Clan/deleteClan?accountId={accountId}&clanId={clanId}")).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Transfers leadership of a clan to a new leader.
        /// </summary>
        /// <param name="leaderAccountId">Account id of leader.</param>
        /// <param name="clanId">Id of clan.</param>
        /// <param name="newLeaderAccountId">Account id of new leader.</param>
        /// <returns>Returns created clan.</returns>
        public async Task<bool> ClanTransferLeadership(int leaderAccountId, int clanId, int newLeaderAccountId, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null || clan.ClanLeaderAccount.AccountId != leaderAccountId)
                        return false;

                    var newLeaderAccount = await GetAccountById(newLeaderAccountId);
                    if (newLeaderAccount == null)
                        return false;

                    // must be a member
                    if (newLeaderAccount.ClanId != clanId)
                        return false;

                    clan.ClanLeaderAccount = newLeaderAccount;
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/transferLeadership", JsonConvert.SerializeObject(new ClanTransferLeadershipDTO()
                    {
                        AccountId = leaderAccountId,
                        ClanId = clanId,
                        NewLeaderAccountId = newLeaderAccountId
                    }))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Transfers leadership of a clan to a new leader.
        /// </summary>
        /// <param name="leaderAccountId">Account id of leader.</param>
        /// <param name="clanId">Id of clan.</param>
        /// <param name="newLeaderAccountId">Account id of new leader.</param>
        /// <returns>Returns created clan.</returns>
        public async Task<bool> ClanLeave(int fromAccountId, int clanId, int accountId, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null)
                        return false;

                    // only allow leader or player remove player
                    if (fromAccountId != accountId && clan.ClanLeaderAccount.AccountId != fromAccountId)
                        return false;

                    // prevent leader from leaving -- must transfer or disband
                    if (clan.ClanLeaderAccount.AccountId == accountId)
                        return false;

                    var account = clan.ClanMemberAccounts.FirstOrDefault(x => x.AccountId == accountId);
                    if (account != null)
                    {
                        account.ClanId = null;
                        clan.ClanMemberAccounts.Remove(account);
                    }

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/leaveClan?fromAccountId={fromAccountId}&clanId={clanId}&accountId={accountId}", null))?.IsSuccessStatusCode ?? false;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Creates a new clan invitation for the given player.
        /// </summary>
        /// <param name="fromAccountId">Id of account sending invite.</param>
        /// <param name="clanId">Id of clan.</param>
        /// <param name="accountId">Id of target player.</param>
        /// <param name="message">Invite message.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> CreateClanInvitation(int fromAccountId, int clanId, int accountId, string message)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = _simulatedClans.FirstOrDefault(x => x.ClanId == clanId);
                    if (clan == null)
                        return false;

                    // validate from is leader
                    if (clan.ClanLeaderAccount.AccountId != fromAccountId)
                        return false;

                    // get target account
                    var account = _simulatedAccounts.FirstOrDefault(x => x.AccountId == accountId);
                    if (account == null)
                        return false;

                    // check if invitations already made
                    if (clan.ClanMemberInvitations.Any(x => x.TargetAccountId == accountId && x.ResponseStatus == 0))
                        return false;

                    // add
                    clan.ClanMemberInvitations.Add(new ClanInvitationDTO()
                    {
                        InvitationId = _simulatedClanInvitationIdCounter++,
                        AppId = clan.AppId,
                        ClanId = clanId,
                        ClanName = clan.ClanName,
                        TargetAccountId = accountId,
                        TargetAccountName = account.AccountName,
                        Message = message
                    });

                    return true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/createInvitation?accountId={fromAccountId}", new ClanInvitationDTO()
                    {
                        ClanId = clanId,
                        TargetAccountId = accountId,
                        Message = message
                    })).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }


        /// <summary>
        /// Returns all clan invitations for the given player.
        /// </summary>
        /// <param name="accountId">Id of target player.</param>
        /// <returns>Success or failure.</returns>
        public async Task<List<AccountClanInvitationDTO>> GetClanInvitationsByAccount(int accountId)
        {
            List<AccountClanInvitationDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clans
                    var clans = _simulatedClans.Where(x => x.ClanMemberInvitations.Any(y => y.TargetAccountId == accountId));

                    result = clans
                        .Select(x => new AccountClanInvitationDTO()
                        {
                            LeaderAccountId = x.ClanLeaderAccount.AccountId,
                            LeaderAccountName = x.ClanLeaderAccount.AccountName,
                            Invitation = x.ClanMemberInvitations.FirstOrDefault(y => y.TargetAccountId == accountId)
                        })
                        .Where(x => x.Invitation != null)
                        .ToList();
                }
                else
                {
                    result = (await GetDbAsync<List<AccountClanInvitationDTO>>($"Clan/invitations?accountId={accountId}"));
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Sets the response to the given clan invitation.
        /// </summary>
        /// <param name="accountId">Id of account.</param>
        /// <param name="inviteId">Id of clan invitation.</param>
        /// <param name="message">Response message to record.</param>
        /// <param name="responseStatus">Response to invitation.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> RespondToClanInvitation(int accountId, int inviteId, string message, int responseStatus)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // find invitation
                    var invite = _simulatedClans.Select(x => x.ClanMemberInvitations.FirstOrDefault(y => y.InvitationId == inviteId)).FirstOrDefault(x => x != null);
                    if (invite == null)
                        return false;

                    // get clan
                    var clan = _simulatedClans.FirstOrDefault(x => x.ClanMemberInvitations.Contains(invite));
                    if (clan == null)
                        return false;

                    // get account
                    var account = _simulatedAccounts.FirstOrDefault(x => x.AccountId == accountId);
                    if (account == null)
                        return false;

                    // validate its for user
                    if (invite.TargetAccountId != accountId)
                        return false;

                    // validate it's not already been decided
                    if (invite.ResponseStatus != 0)
                        return false;

                    invite.ResponseMessage = message;
                    invite.ResponseStatus = responseStatus;
                    invite.ResponseTime = (int)Utils.GetUnixTime();

                    // handle accept
                    if (responseStatus == 1)
                    {
                        account.ClanId = invite.ClanId;
                        clan.ClanMemberAccounts.Add(account);
                    }

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/respondInvitation", new ClanInvitationResponseDTO()
                    {
                        AccountId = accountId,
                        InvitationId = inviteId,
                        Response = responseStatus,
                        ResponseMessage = message,
                        ResponseTime = (int)Utils.GetUnixTime()
                    })).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Revokes a given clan's invitation to the target account.
        /// </summary>
        /// <param name="fromAccountId">Id of account requesting revoke.</param>
        /// <param name="clanId">Id of clan.</param>
        /// <param name="targetAccountId">Target account to revoke invitation to.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> RevokeClanInvitation(int fromAccountId, int clanId, int targetAccountId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = _simulatedClans.FirstOrDefault(x => x.ClanId == clanId);
                    if (clan == null)
                        return false;

                    // validate leader is fromAccount
                    if (clan.ClanLeaderAccount.AccountId != fromAccountId)
                        return false;

                    // find invitation
                    var invite = clan.ClanMemberInvitations.FirstOrDefault(x => x.TargetAccountId == targetAccountId);
                    if (invite == null)
                        return false;

                    // validate it's not already been decided
                    if (invite.ResponseStatus != 0)
                        return false;

                    invite.ResponseStatus = 3;

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/revokeInvitation?fromAccountId={fromAccountId}&clanId={clanId}&targetAccountId={targetAccountId}", null)).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Returns all clan invitations for the given player.
        /// </summary>
        /// <param name="accountId">Id of target player.</param>
        /// <returns>Success or failure.</returns>
        public async Task<List<ClanMessageDTO>> GetClanMessages(int accountId, int clanId, int startIndex, int pageSize, int appId)
        {
            List<ClanMessageDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = await GetClanById(clanId, appId);
                    if (clan != null)
                    {
                        result = clan.ClanMessages
                            .Skip(startIndex * pageSize)
                            .Take(pageSize)
                            .ToList();
                    }
                }
                else
                    result = await GetDbAsync<List<ClanMessageDTO>>($"Clan/messages?accountId={accountId}&clanId={clanId}&start={startIndex}&pageSize={pageSize}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Adds a clan message to the clan.
        /// </summary>
        /// <param name="accountId">Id of sender account.</param>
        /// <param name="clanId">Id of clan.</param>
        /// <param name="message">Message to add.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> ClanAddMessage(int accountId, int clanId, string message, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null)
                        return false;

                    // validate leader
                    if (clan.ClanLeaderAccount.AccountId != accountId)
                        return false;

                    clan.ClanMessages.Add(new ClanMessageDTO()
                    {
                        Id = _simulatedClanMessageIdCounter++,
                        Message = message
                    });

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/addMessage?accountId={accountId}&clanId={clanId}", new ClanMessageDTO()
                    {
                        Message = message
                    })).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Edits an existing clan message.
        /// </summary>
        /// <param name="accountId">Id of sender account.</param>
        /// <param name="clanId">Id of clan.</param>
        /// <param name="messageId">Id of clan message to edit.</param>
        /// <param name="message">Message to add.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> ClanEditMessage(int accountId, int clanId, int messageId, string message, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null)
                        return false;

                    // validate leader
                    if (clan.ClanLeaderAccount.AccountId != accountId)
                        return false;

                    // find message
                    var clanMessage = clan.ClanMessages.FirstOrDefault(x => x.Id == messageId);
                    if (clanMessage == null)
                        return false;

                    clanMessage.Message = message;

                    result = true;
                }
                else
                {
                    result = (await PutDbAsync($"Clan/editMessage?accountId={accountId}&clanId={clanId}", new ClanMessageDTO()
                    {
                        Id = messageId,
                        Message = message
                    })).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Edits an existing clan message.
        /// </summary>
        /// <param name="accountId">Id of sender account.</param>
        /// <param name="clanId">Id of clan.</param>
        /// <param name="messageId">Id of clan message to delete.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> ClanDeleteMessage(int accountId, int clanId, int messageId, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null)
                        return false;

                    // validate leader
                    if (clan.ClanLeaderAccount.AccountId != accountId)
                        return false;

                    // find message
                    var clanMessage = clan.ClanMessages.FirstOrDefault(x => x.Id == messageId);
                    if (clanMessage == null)
                        return false;

                    clanMessage.Message = null;

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"Clan/deleteMessage?accountId={accountId}&clanId={clanId}", new ClanMessageDTO()
                    {
                        Id = messageId
                    })).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Request Clan Team Challenge from client
        /// </summary>
        /// <param name="challengerClanId">The ClanId of the clan to request a team challenge on.</param>
        /// <param name="accountId">Is this accountId the Clan Leader?</param>
        /// <param name="appId">AppId of the game to filter by</param>
        /// <param name="message">Message by the game to send for requesting the clan team challenge</param>
        /// <returns>Returns clan.</returns>
        public async Task<ClanTeamChallengeDTO> RequestClanTeamChallenge(int challengerClanId, int againstClanId, int accountId, string message, int appId)
        {
            ClanTeamChallengeDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = await GetClanById((int)challengerClanId, appId);
                    if (clan == null)
                        return null;

                    // validate leader
                    if (clan.ClanLeaderAccount.AccountId != accountId)
                        return null;

                    result = null; //_simulatedClans.FirstOrDefault(x => x.AppId == appId && x.ClanId == clanId);
                }
                else
                {
                    result = (await PostDbAsync<ClanTeamChallengeDTO>($"Clan/requestClanTeamChallenge?challengerClanId={challengerClanId}&againstClanId={againstClanId}&accountId={accountId}&message={message}&appId={appId}", new ClanTeamChallengeDTO()
                    {

                    }));
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        public async Task<List<ClanTeamChallengeDTO>> GetClanTeamChallenges(int clanId, int accountId, MediusClanChallengeStatus clanChallengeStatus, int startIdx, int pageSize, int appId)
        {
            List<ClanTeamChallengeDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clan
                    var clan = await GetClanById(clanId, appId);
                    if (clan == null)
                        return null;

                    // validate leader
                    if (clan.ClanLeaderAccount.AccountId != accountId)
                        return null;

                    result = null; //_simulatedClans.FirstOrDefault(x => x.AppId == appId && x.ClanId == clanId);
                }
                else
                    result = await GetDbAsync<List<ClanTeamChallengeDTO>>($"Clan/getClanTeamChallenges?clanId={clanId}&accountId={accountId}&clanChallengeStatus={(int)clanChallengeStatus}&appId={appId}&start={startIdx}&pageSize={pageSize}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        public async Task<bool> RespondClanTeamChallenge(int clanChallengeId, MediusClanChallengeStatus clanChallengeStatus, int accountId, string message, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = false; //_simulatedClans.FirstOrDefault(x => x.AppId == appId && x.ClanId == clanId);
                else
                {
                    result = (await PostDbAsync($"Clan/respondClanTeamChallenge?clanChallengeId={clanChallengeId}&clanChallengeStatus={(int)clanChallengeStatus}&accountId={accountId}&message={message}&appId={appId}", new ClanTeamChallengeDTO()
                    {

                    })).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        public async Task<bool> RevokeClanTeamChallenge(int clanChallengeId, int accountId, int appId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = false; //_simulatedClans.FirstOrDefault(x => x.AppId == appId && x.ClanId == clanId);
                else
                {
                    result = (await PostDbAsync($"Clan/revokeClanTeamChallenge?clanChallengeId={clanChallengeId}&accountId={accountId}&appId={appId}", new ClanTeamChallengeDTO()
                    {

                    })).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        #endregion

        #region Announcements / Policy

        /// <summary>
        /// Gets the latest announcement.
        /// </summary>
        public async Task<DimAnnouncements> GetLatestAnnouncement(int appId)
        {
            DimAnnouncements result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    /*
                    return new DimAnnouncements()
                    {
                        Id = 1,
                        AnnouncementTitle = "Announcement Title",
                        AnnouncementBody = "Announcement Body",
                        CreateDt = DateTime.UtcNow,
                    };
                    */
                }
                else
                    result = await GetDbAsync<DimAnnouncements>($"api/Keys/getAnnouncements?fromDt={DateTime.UtcNow}&AppId={appId}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Gets the latest announcements.
        /// </summary>
        public async Task<DimAnnouncements[]> GetLatestAnnouncements(int appId, int size = 10)
        {
            DimAnnouncements[] result = null;

            try
            {
                if (_settings.SimulatedMode)
                {/*
                    return new DimAnnouncements[]
                    {
                        new DimAnnouncements()
                        {
                            Id = 1,
                            AnnouncementTitle = "Announcement Title",
                            AnnouncementBody = "Announcement Body",
							CreateDt = DateTime.UtcNow,

                        }
                    };
                    */
                }
                else
                    result = await GetDbAsync<DimAnnouncements[]>($"api/Keys/getAnnouncementsList?Dt={DateTime.UtcNow}&TakeSize={size}&AppId={appId}");
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Gets the Usage/Privacy policy.
        /// </summary>
        public async Task<DimEula> GetPolicy(int policyType, int appId)
        {
            DimEula result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    /*
                    return new DimEula()
                    {
                        Id = 1,
                        AppId = 0,
                        PolicyType = policyType,
                        EulaTitle = "Eula Test",
                        EulaBody = "Eula Body",

                    };
                    */
                }
                else
                    result = await GetDbAsync<DimEula>($"api/Keys/getEULA?policyType={policyType}&appId={appId}&fromDt={DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        #endregion

        #region Locations

        /// <summary>
        /// Get Locations
        /// </summary>
        public async Task<DimLocations> GetDimLocations(int LocationId, int appId)
        {
            DimLocations result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    return new DimLocations()
                    {
                        Id = 1,
                        LocationId = 1,
                        LocationName = "Default",

                    };
                }
                else
                    result = await GetDbAsync<DimLocations>($"api/keys/getLocations?LocationId={LocationId}&AppId={appId}&fromDt={DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        #endregion

        #region Medius File Services

        #region createFile
        public async Task<bool> createFile(FileDTO createFile)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    _simulatedMediusFiles.Add(createFile);
                    _simulatedFileAttributes.Add(new FileAttributesDTO
                    {
                        AppId = createFile.AppId,
                        FileID = createFile.FileID,
                        LastChangedByUserID = createFile.fileAttributesDTO.LastChangedByUserID,
                        LastChangedTimeStamp = createFile.fileAttributesDTO.LastChangedTimeStamp,
                        Description = createFile.fileAttributesDTO.Description,
                        StreamableFlag = createFile.fileAttributesDTO.StreamableFlag,
                        StreamingDataRate = createFile.fileAttributesDTO.StreamingDataRate,
                    });

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"FileServices/addFile?AppId={createFile.AppId}&File={createFile}", JsonConvert.SerializeObject(new FileDTO
                    {
                        AppId = createFile.AppId,
                        FileName = createFile.FileName,
                        ServerChecksum = createFile.ServerChecksum,
                        FileID = createFile.FileID,
                        FileSize = createFile.FileSize,
                        CreationTimeStamp = createFile.CreationTimeStamp,
                        OwnerID = createFile.OwnerID,
                        GroupID = createFile.GroupID,
                        OwnerPermissionRWX = createFile.OwnerPermissionRWX,
                        GroupPermissionRWX = createFile.GroupPermissionRWX,
                        GlobalPermissionRWX = createFile.GlobalPermissionRWX,
                        ServerOperationID = createFile.ServerOperationID,
                        fileAttributesDTO = new FileAttributesDTO()
                        {
                            AppId = createFile.AppId,
                            FileID = createFile.FileID,
                            FileName = createFile.FileName,
                            LastChangedByUserID = createFile.fileAttributesDTO.LastChangedByUserID,
                            LastChangedTimeStamp = createFile.fileAttributesDTO.LastChangedTimeStamp,
                            Description = createFile.fileAttributesDTO.Description,
                            StreamableFlag = createFile.fileAttributesDTO.StreamableFlag,
                            StreamingDataRate = createFile.fileAttributesDTO.StreamingDataRate,
                        }
                    }))).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region deleteFile
        public async Task<bool> deleteFile(FileDTO file)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    _simulatedMediusFiles.Remove(file);

                    result = true;
                }
                else
                    result = (await PostDbAsync($"FileServices/deleteFile", file)).IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region getFileList
        public async Task<List<FileDTO>> getFileList(int appId, string FileNameBeginsWith, uint OwnerByID)
        {
            List<FileDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    if (FileNameBeginsWith == "*")
                    {
                        _simulatedMediusFiles.Find(x => x.AppId == appId && x.OwnerID == OwnerByID);

                        result = _simulatedMediusFiles;
                    }
                    else
                    {
                        _simulatedMediusFiles.Find(x => x.AppId == appId && x.OwnerID == OwnerByID && x.FileName.StartsWith(FileNameBeginsWith));

                        result = _simulatedMediusFiles;
                    }
                }
                else
                {
                    if (OwnerByID > 2147483647)
                    {
                        OwnerByID = 2147483646;

                        ServerConfiguration.LogWarn($"FileNameBeginsWith: {FileNameBeginsWith.Remove(FileNameBeginsWith.Length - 1)} OwnerByID: {OwnerByID}");
                        result = await GetDbAsync<List<FileDTO>>($"FileServices/getFileList?AppId={appId}&FileNameBeginsWith={FileNameBeginsWith.Remove(FileNameBeginsWith.Length - 1)}&OwnerByID={OwnerByID}");

                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region getFileListExt
        public async Task<List<FileDTO>> getFileListExt(int appId, string FileNameBeginsWith, int OwnerByID, MediusFileMetaData fileMetaData)
        {
            List<FileDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    if (FileNameBeginsWith == "*")
                    {

                        FileMetaDataDTO fileMetaDataDTO = new FileMetaDataDTO()
                        {
                            AppId = appId,
                            Key = fileMetaData.Key,
                            Value = fileMetaData.Value,
                        };
                        _simulatedMediusFiles.Find(x => x.AppId == appId && x.OwnerID == OwnerByID && x.fileMetaDataDTO == _simulatedFileMetaData.Find(x => x == fileMetaDataDTO));

                        result = _simulatedMediusFiles;
                    }
                    else
                    {
                        FileMetaDataDTO fileMetaDataDTO = new FileMetaDataDTO()
                        {
                            AppId = appId,
                            Key = fileMetaData.Key,
                            Value = fileMetaData.Value,
                        };
                        _simulatedMediusFiles.Find(x => x.AppId == appId && x.OwnerID == OwnerByID && x.FileName.StartsWith(FileNameBeginsWith) && x.fileMetaDataDTO == _simulatedFileMetaData.Find(x => x == fileMetaDataDTO));

                        result = _simulatedMediusFiles;
                    }
                }
                else
                {
                    ServerConfiguration.LogWarn($"FileNameBeginsWith: {FileNameBeginsWith} OwnerByID: {OwnerByID} metaKey: {fileMetaData.Key}");
                    result = await GetDbAsync<List<FileDTO>>($"FileServices/getFileListExt?AppId={appId}&FileNameBeginsWith={FileNameBeginsWith}&OwnerByID={OwnerByID}&metaKey={fileMetaData.Key}&metaValue={fileMetaData.Value}");
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region UpdateFileAttributes
        /// <summary>
        /// Posts the given key and value to the database for that given file
        /// </summary>
        /// <param name="appId">appId for MFS</param>
        /// <param name="mediusFile">MediusFile specified by UpdateFileMetaRequest</param>
        /// <param name="mediusFileAttributes">mediusFileAttributes specified by Game Developer.</param>
        public async Task<List<FileAttributesDTO>> UpdateFileAttributes(FileDTO file)
        {
            List<FileAttributesDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    if (_simulatedFileMetaData == null)
                        return result;

                    _simulatedMediusFiles.Add(file);
                    _simulatedFileAttributes.Add(new FileAttributesDTO
                    {
                        AppId = file.AppId,
                        FileID = file.FileID,
                        FileName = file.FileName,
                        LastChangedByUserID = file.fileAttributesDTO.LastChangedByUserID,
                        LastChangedTimeStamp = file.fileAttributesDTO.LastChangedTimeStamp,
                        Description = file.fileAttributesDTO.Description,
                        StreamableFlag = file.fileAttributesDTO.StreamableFlag,
                        StreamingDataRate = file.fileAttributesDTO.StreamingDataRate,
                    });


                    result = _simulatedFileAttributes;
                }
                else
                    result = await PostDbAsync<List<FileAttributesDTO>>($"FileServices/updateFileAttributes?File={file}", file);
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region GetFileAttributes
        /// <summary>
        /// Posts the given key and value to the database for that given file
        /// </summary>
        /// <param name="appId">appId for MFS</param>
        /// <param name="mediusFile">MediusFile specified by UpdateFileMetaRequest</param>
        public async Task<List<FileAttributesDTO>> GetFileAttributes(FileDTO mediusFile)
        {
            List<FileAttributesDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    if (_simulatedFileMetaData == null)
                        return result;

                    _simulatedMediusFiles.Contains(mediusFile);
                    _simulatedFileAttributes.Find(x => x.FileID == mediusFile.FileID);


                    result = _simulatedFileAttributes;
                }
                else
                    result = await GetDbAsync<List<FileAttributesDTO>>($"FileServices/getFileAttributes?AppId={mediusFile.AppId}&File={mediusFile}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region UpdateFileMetaData
        /// <summary>
        /// Posts the given key and value to the database for that given file
        /// </summary>
        /// <param name="appId">appId for MFS</param>
        /// <param name="mediusFile">MediusFile specified by UpdateFileMetaRequest</param>
        /// <param name="mediusFileMetaData">MediusFileMetaData specified by Game Developer.</param>
        public async Task<bool> UpdateFileMetaData(FileDTO file)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    if (!_simulatedMediusFiles.Contains(file))
                    {
                        _simulatedMediusFiles.Add(file);
                        _simulatedFileMetaData.Add(file.fileMetaDataDTO);
                    }

                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"FileServices/updateFileMetaData?AppId={file.AppId}", JsonConvert.SerializeObject(new FileMetaDataDTO
                    {
                        AppId = file.AppId,
                        FileID = file.FileID,
                        FileName = file.FileName,
                        Key = file.fileMetaDataDTO.Key,
                        Value = file.fileMetaDataDTO.Value,
                        CreateDt = DateTime.Now,
                    }))).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region GetFileMetaData
        /// <summary>
        /// Posts the given key and value to the database for that given file
        /// </summary>
        /// <param name="appId">appId for MFS</param>
        /// <param name="mediusFile">MediusFile specified by UpdateFileMetaRequest</param>
        /// <param name="mediusFileMetaData">MediusFileMetaData specified by Game Developer.</param>
        public async Task<List<FileMetaDataDTO>> GetFileMetaData(int appId, string fileName, string Key)
        {
            List<FileMetaDataDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    if (_simulatedFileMetaData == null)
                        return result;

                    result = _simulatedFileMetaData;
                }
                else
                    result = await GetDbAsync<List<FileMetaDataDTO>>($"FileServices/getFileMetaData?appId={appId}&FileName={fileName}&Key={Key}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #endregion

        #region NpIds
        /// <summary>
        /// Post the NpId to the database
        /// </summary>
        public async Task<bool> PostNpId(NpIdDTO NpId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    ServerConfiguration.LogWarn("Simulated DB NpID Success");
                    _simulatedNpIdAccounts.Add(new NpIdDTO()
                    {
                        AppId = NpId.AppId,
                        data = NpId.data,
                        term = NpId.term,
                        dummy = NpId.dummy,

                        opt = NpId.opt,
                        reserved = NpId.reserved,
                        CreateDt = DateTime.UtcNow
                    });

                    result = true;

                    return result;
                }
                else
                {
                    result = (await PostDbAsync($"Account/postNpId?&AppId={NpId.AppId}&SceNpId={NpId}&createDt={DateTime.UtcNow}", JsonConvert.SerializeObject(new NpIdDTO
                    {
                        AppId = NpId.AppId,
                        data = NpId.data,
                        term = NpId.term,
                        dummy = NpId.dummy,

                        opt = NpId.opt,
                        reserved = NpId.reserved,
                        CreateDt = DateTime.UtcNow
                    }))).IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Post the NpId to the database
        /// </summary>
        public async Task<List<NpIdDTO>> NpIdSearchByAccountNames(NpIdDTO NpId, int appId)
        {
            List<NpIdDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    _simulatedNpIdAccounts.FirstOrDefault(simulatedNpId => simulatedNpId == NpId && simulatedNpId.AppId == appId);

                    result = _simulatedNpIdAccounts.ToList();

                    return result;
                }
                else
                    result = await GetDbAsync<List<NpIdDTO>>($"Account/searchNpIdByAccountName?SceNpId={NpId}&AppId={appId}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }
        #endregion

        #region Game

        public async Task<string> GetGameList()
        {
            string results = null;

            HttpResponseMessage Resp = null;
            try
            {
                if (_settings.SimulatedMode) // Deprecated
                    return "[]";
                else
                {
                    Resp = await GetDbAsync($"api/Game/list");
                    results = await Resp.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return results;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public async Task<bool> CreateGame(GameDTO game)
        {
            bool result = false;
            try
            {
                if (_settings.SimulatedMode)
                {
                    result = true;
                }
                else
                {
                    result = (await PostDbAsync($"api/Game/create", JsonConvert.SerializeObject(game))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public async Task<bool> UpdateGame(GameDTO game)
        {
            bool result = false;
            try
            {
                if (_settings.SimulatedMode)
                {
                    result = true;
                }
                else
                {
                    result = (await PutDbAsync($"api/Game/update/{game.GameId}", JsonConvert.SerializeObject(game))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<bool> UpdateGameMetadata(int gameId, string metadata)
        {
            bool result = false;
            try
            {
                if (_settings.SimulatedMode)
                {
                    result = true;
                }
                else
                {
                    result = (await PutDbAsync($"api/Game/updateMetaData/{gameId}", JsonConvert.SerializeObject(metadata))).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Delete game by game id.
        /// </summary>
        /// <param name="gameId">Game id.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> DeleteGame(int gameId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    result = true;
                }
                else
                {
                    result = (await DeleteDbAsync($"api/Game/delete/{gameId}")).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        /// <summary>
        /// Clear the active games table.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearActiveGames()
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                {
                    result = true;
                }
                else
                {
                    result = (await DeleteDbAsync($"api/Game/clear")).IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError(e);
            }

            return result;
        }

        #endregion

        #region World

        public async Task<ChannelDTO[]> GetChannels()
        {
            ChannelDTO[] results = null;

            try
            {
                if (_settings.SimulatedMode == true)
                {

                    return new ChannelDTO[]
                    {   
                        /*
                        new ChannelDTO()
                        {
                            AppId = 10540,
                            Id = 1,
                            Name = "Rank1",
                            MaxPlayers = 256,
                            GenericField1 = 1,
                            GenericFieldFilter = 1
                        },
                        */

                        // Ratchet UYA PS2

                        new ChannelDTO()
                        {
                            AppId = 10683,
                            Id = 1,
                            Name = "CY00000000-00",
                            MaxPlayers = 256,
                            GenericField1 = 0,
                            GenericField2 = 0,
                            GenericField3 = 0,
                            GenericField4 = 0,
                            GenericFieldFilter = 32
                        },

                        new ChannelDTO()
                        {
                            AppId = 10684,
                            Id = 1,
                            Name = "CY00000000-00",
                            MaxPlayers = 256,
                            GenericField1 = 0,
                            GenericField2 = 0,
                            GenericField3 = 0,
                            GenericField4 = 0,
                            GenericFieldFilter = 32
                        },

                        new ChannelDTO()
                        {
                            AppId = 20624,
                            Id = 1,
                            Name = "US",
                            MaxPlayers = 512,
                            GenericField1 = 1,
                            GenericField2 = 1,
                            GenericFieldFilter = 64
                        },
                        new ChannelDTO()
                        {
                            AppId = 20244,
                            Id = 1,
                            Name = "NBA 07",
                            MaxPlayers = 256,
                            GenericField1 = 1,
                            GenericFieldFilter = 64
                        },

                        new ChannelDTO()
                        {
                            AppId = 10540,
                            Id = 2,
                            Name = "Rank2",
                            MaxPlayers = 256,
                            GenericField1 = 16,
                            GenericFieldFilter = 1
                        },

                        //Arc the Lad: End of Darkness
                        new ChannelDTO()
                        {
                            AppId = 10984,
                            Id = 1,
                            Name = "Yewbell",
                            MaxPlayers = 256,
                        },
                        new ChannelDTO()
                        {
                            AppId = 10984,
                            Id = 2,
                            Name = "Rueloon",
                            MaxPlayers = 256,
                        },

                        //WRC 04
                        new ChannelDTO()
                        {
                            AppId = 10394,
                            Id = 1,
                            Name = "Internal 1",
                            MaxPlayers = 256,
                            GenericField1 = 4096,
                            GenericFieldFilter = 1
                        },
                        new ChannelDTO()
                        {
                            AppId = 10394,
                            Id = 2,
                            Name = "Internal 2",
                            MaxPlayers = 256,
                            GenericField1 = 8192,
                            GenericFieldFilter = 1
                        },

                        //WRC05 Beta
                        new ChannelDTO()
                        {
                            AppId = 10933,
                            Id = 1,
                            Name = "Internal 1",
                            MaxPlayers = 256,
                            GenericField1 = 4096,
                            GenericFieldFilter = 1
                        },
                        new ChannelDTO()
                        {
                            AppId = 10933,
                            Id = 2,
                            Name = "Internal 2",
                            MaxPlayers = 256,
                            GenericField1 = 8192,
                            GenericFieldFilter = 1
                        },
                        new ChannelDTO()
                        {
                            AppId = 10933,
                            Id = 3,
                            Name = "Internal 3",
                            MaxPlayers = 256,
                            GenericField1 = 16384,
                            GenericFieldFilter = 1
                        },

                        //WRC Rally Evolved
                        new ChannelDTO()
                        {
                            AppId = 10934,
                            Id = 1,
                            Name = "Internal 1",
                            MaxPlayers = 256,
                            GenericField1 = 4096,
                            GenericFieldFilter = 1
                        },
                        new ChannelDTO()
                        {
                            AppId = 10934,
                            Id = 2,
                            Name = "Internal 2",
                            MaxPlayers = 256,
                            GenericField1 = 8192,
                            GenericFieldFilter = 1
                        },
                        new ChannelDTO()
                        {
                            AppId = 10934,
                            Id = 3,
                            Name = "Internal 3",
                            MaxPlayers = 256,
                            GenericField1 = 16384,
                            GenericFieldFilter = 1
                        }

                    };
                }
                else
                {
                    results = await GetDbAsync<ChannelDTO[]>($"api/World/getChannels");
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return results;
        }

        public async Task<LocationDTO[]> GetLocations()
        {
            LocationDTO[] results = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    return new LocationDTO[]
                    {
                        new LocationDTO()
                        {
                            AppId = 0,
                            Id = 0,
                            Name = "Location 1"
                        },
                        new LocationDTO()
                        {
                            AppId = 24000,
                            Id = 0,
                            Name = "Aquatos"
                        },
                        new LocationDTO()
                        {
                            AppId = 23044,
                            Id = 0,
                            Name = "US"
                        },
                        new LocationDTO()
                        {
                            AppId = 10680,
                            Id = 40,
                            Name = "Aquatos"
                        },
                        new LocationDTO()
                        {
                            AppId = 10683,
                            Id = 40,
                            Name = "Aquatos"
                        },
                        new LocationDTO()
                        {
                            AppId = 10684,
                            Id = 40,
                            Name = "Aquatos"
                        }
                    };
                }
                else
                {
                    results = await GetDbAsync<LocationDTO[]>($"api/World/getLocations");
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return results;
        }

        public async Task<LocationDTO[]> GetLocations(int appId)
        {
            LocationDTO[] results = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    if (appId == 10680 || appId == 10683 || appId == 10684)
                    {
                        return new LocationDTO[]
                        {
                            new LocationDTO()
                            {
                                AppId = appId,
                                Id = 40,
                                Name = "Aquatos"
                            }
                        };
                    }
                    else if (appId == 24000)
                    {
                        return new LocationDTO[]
                        {
                            new LocationDTO()
                            {
                                AppId = appId,
                                Id = 0,
                                Name = "Aquatos"
                            }
                        };
                    }
                    else if (appId == 23044)
                    {
                        return new LocationDTO[]
                        {
                            new LocationDTO()
                            {
                                AppId = appId,
                                Id = 40,
                                Name = "US"
                            }
                        };
                    }
                    else
                    {
                        return new LocationDTO[]
                        {
                            new LocationDTO()
                            {
                                AppId = 0,
                                Id = 0,
                                Name = "Location 1"
                            }
                        };
                    }
                }
                else
                    results = await GetDbAsync<LocationDTO[]>($"api/World/getLocations/{appId}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return results;
        }

        #endregion

        #region Matchmaking 

        /// <summary>
        /// Returns all clan invitations for the given player.
        /// </summary>
        /// <param name="accountId">Id of target player.</param>
        /// <returns>Success or failure.</returns>
        public async Task<List<MatchmakingSupersetDTO>> GetMatchmakingSupersets(int appId)
        {
            List<MatchmakingSupersetDTO> result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    // get clans
                    var supersets = _simulatedMatchmakingSupersets.Where(x => x.AppId == appId);

                    result = supersets.Select(x => new MatchmakingSupersetDTO()
                    {
                        SupersetID = 1,
                        SupersetName = "Casual",
                        SupersetDescription = "M:PR Matchmaking",
                        AppId = 21624
                    }).Where(x => x.SupersetID != 0).ToList();
                }
                else
                    result = (await GetDbAsync<List<MatchmakingSupersetDTO>>($"Matchmaking/supersets?appId={appId}"));

            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        #endregion

        #region Party

        /// <summary>
        /// 
        /// </summary>
        /// <param name="party"></param>
        /// <returns></returns>
        public async Task<bool> CreateParty(PartyDTO party)
        {
            bool result = false;
            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await PostDbAsync($"api/Party/create", JsonConvert.SerializeObject(party))).IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="party"></param>
        /// <returns></returns>
        public async Task<bool> UpdateParty(PartyDTO party)
        {
            bool result = false;
            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await PutDbAsync($"api/Game/update/{party.PartyId}", JsonConvert.SerializeObject(party))).IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePartyMetadata(int partyId, string metadata)
        {
            bool result = false;
            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await PutDbAsync($"api/Party/updateMetaData/{partyId}", JsonConvert.SerializeObject(metadata))).IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Delete party by party id.
        /// </summary>
        /// <param name="partyId">Party id.</param>
        /// <returns>Success or failure.</returns>
        public async Task<bool> DeleteParty(int partyId)
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await DeleteDbAsync($"api/Party/delete/{partyId}")).IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Clear the active Parties table.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearActiveParties()
        {
            bool result = false;

            try
            {
                if (_settings.SimulatedMode)
                    result = true;
                else
                    result = (await DeleteDbAsync($"api/Party/clear")).IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        #endregion

        #region Universes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId">appid of that universe to find</param>
        /// <returns></returns>
        public async Task<List<UniverseDTO>> GetUniverses(int appId)
        {
            List<UniverseDTO> result = null;
            try
            {
                if (_settings.SimulatedMode)
                    result = null;
                else
                    result = await GetDbAsync<List<UniverseDTO>>($"Universes/getUniverses?appId={appId}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId">appid of that universe to find</param>
        /// <returns></returns>
        public async Task<List<UniverseNewsDTO>> GetUniverseNews(int appId)
        {
            List<UniverseNewsDTO> result = null;
            try
            {
                if (_settings.SimulatedMode)
                {
                    result = null;
                }
                else
                {
                    result = await GetDbAsync<List<UniverseNewsDTO>>($"Universes/getUniverseNews?appId={appId}");
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        #endregion

        #region Key

        public async Task<AppIdDTO[]> GetAppIds()
        {
            AppIdDTO[] results = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    return new AppIdDTO[]
                    {
                        new AppIdDTO()
                        {
                            Name = "All",
                            AppIds = Enumerable.Range(0, 100000).ToList()
                        }
                    };
                }
                else
                {
                    results = await GetDbAsync<AppIdDTO[]>($"api/Keys/getAppIds");
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return results;
        }

        public async Task<Dictionary<string, string>> GetServerSettings(int appId)
        {
            Dictionary<string, string> result = null;

            try
            {
                if (_settings.SimulatedMode)
                    return new Dictionary<string, string>();
                else
                    result = await GetDbAsync<Dictionary<string, string>>($"api/Keys/getSettings?appId={appId}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        public async Task SetServerSettings(int appId, Dictionary<string, string> settings)
        {
            try
            {
                if (_settings.SimulatedMode)
                {

                }
                else
                {
                    await PostDbAsync($"api/Keys/setSettings?appId={appId}", settings);
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }
        }

        public async Task<ServerFlagsDTO> GetServerFlags()
        {
            ServerFlagsDTO result = null;

            try
            {
                if (_settings.SimulatedMode)
                {
                    return new ServerFlagsDTO()
                    {
                        MaintenanceMode = new MaintenanceDTO()
                        {
                            IsActive = false,
                            FromDt = DateTime.UtcNow - TimeSpan.FromSeconds(10),
                            ToDt = DateTime.UtcNow + TimeSpan.FromSeconds(1)
                        }
                    };
                }
                else
                {
                    result = await GetDbAsync<ServerFlagsDTO>($"api/Keys/getServerFlags");
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        #endregion

        #region Auth

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<AuthenticationResponse> Authenticate(string username, string password)
        {
            AuthenticationResponse result = null;

            try
            {
                result = await PostDbAsync<AuthenticationResponse>($"Account/authenticate", new AuthenticationRequest()
                {
                    AccountName = username,
                    Password = password
                });
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }

            return result;
        }

        #endregion

        #region Http

        private async Task<HttpResponseMessage> DeleteDbAsync(string route)
        {
            HttpResponseMessage result = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        result = await client.DeleteAsync($"{_settings.DatabaseUrl}/{route}");
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = null;
                    }
                }
            }

            return result;
        }

        private async Task<HttpResponseMessage> GetDbAsync(string route)
        {
            HttpResponseMessage result = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        result = await client.GetAsync($"{_settings.DatabaseUrl}/{route}");
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = null;
                    }
                }
            }

            return result;
        }

        private async Task<T> GetDbAsync<T>(string route)
        {
            T result = default(T);

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        if (!string.IsNullOrEmpty(_dbAccessToken))
                            client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                        var response = await client.GetAsync($"{_settings.DatabaseUrl}/{route}");

                        // Deserialize on success
                        if (response.IsSuccessStatusCode)
                            result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = default(T);
                    }
                }
            }

            return result;
        }

        private async Task<HttpResponseMessage> PostDbAsync(string route, string body)
        {
            HttpResponseMessage result = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        result = await client.PostAsync($"{_settings.DatabaseUrl}/{route}", string.IsNullOrEmpty(body) ? null : new StringContent(body, Encoding.UTF8, "application/json"));
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = null;
                    }
                }
            }

            return result;
        }

        private async Task<HttpResponseMessage> PostDbAsync(string route, object body)
        {
            HttpResponseMessage result = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        result = await client.PostAsync($"{_settings.DatabaseUrl}/{route}", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = null;
                    }
                }
            }

            return result;
        }

        private async Task<T> PostDbAsync<T>(string route, object body)
        {
            T result = default(T);

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        var response = await client.PostAsync($"{_settings.DatabaseUrl}/{route}", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

                        // Deserialize on success
                        if (response.IsSuccessStatusCode)
                            result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = default(T);
                    }
                }
            }

            return result;
        }

        private async Task<HttpResponseMessage> PutDbAsync(string route, string body)
        {
            HttpResponseMessage result = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        result = await client.PutAsync($"{_settings.DatabaseUrl}/{route}", string.IsNullOrEmpty(body) ? null : new StringContent(body, Encoding.UTF8, "application/json"));
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = null;
                    }
                }
            }

            return result;
        }

        private async Task<HttpResponseMessage> PutDbAsync(string route, object body)
        {
            HttpResponseMessage result = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        result = await client.PutAsync($"{_settings.DatabaseUrl}/{route}", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = null;
                    }
                }
            }

            return result;
        }

        private async Task<T> PutDbAsync<T>(string route, object body)
        {
            T result = default(T);

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (var client = new HttpClient(handler))
                {
                    if (!string.IsNullOrEmpty(_dbAccessToken))
                        client.DefaultRequestHeaders.Add("Authorization", _dbAccessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    try
                    {
                        var response = await client.PutAsync($"{_settings.DatabaseUrl}/{route}", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

                        // Deserialize on success
                        if (response.IsSuccessStatusCode)
                            result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError(ex);
                        result = default(T);
                    }
                }
            }

            return result;
        }


        #endregion
    }
}
