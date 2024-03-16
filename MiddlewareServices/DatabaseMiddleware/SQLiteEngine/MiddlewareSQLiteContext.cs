using Horizon.LIBRARY.Database.Entities;
using SQLite.CodeFirst;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using FileAttributes = Horizon.LIBRARY.Database.Entities.FileAttributes;

namespace DatabaseMiddleware.SQLiteEngine
{
    public class MiddlewareSQLiteContext : DbContext
    {
        public MiddlewareSQLiteContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
            Configure();
        }

        private void Configure()
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
            Database.Connection.StateChange += Connection_StateChange;
        }

        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open)
            {
                DbCommand sqlite_cmd = ((DbConnection)sender).CreateCommand();
                sqlite_cmd.CommandText = "PRAGMA foreign_keys = ON;";
                sqlite_cmd.ExecuteNonQuery();
            }
        }

        public virtual DbSet<Account>? Account { get; set; }
        public virtual DbSet<AccountFriend>? AccountFriend { get; set; }
        public virtual DbSet<AccountFriendInvitations>? AccountFriendInvitations { get; set; }
        public virtual DbSet<AccountIgnored>? AccountIgnored { get; set; }
        public virtual DbSet<AccountStat>? AccountStat { get; set; }
        public virtual DbSet<AccountCustomStat>? AccountCustomStat { get; set; }
        public virtual DbSet<AccountStatus>? AccountStatus { get; set; }
        public virtual DbSet<Banned>? Banned { get; set; }
        public virtual DbSet<BannedIp>? BannedIp { get; set; }
        public virtual DbSet<BannedMac>? BannedMac { get; set; }
        public virtual DbSet<Clan>? Clan { get; set; }
        public virtual DbSet<ClanInvitation>? ClanInvitation { get; set; }
        public virtual DbSet<ClanMember>? ClanMember { get; set; }
        public virtual DbSet<ClanMessage>? ClanMessage { get; set; }
        public virtual DbSet<ClanStat>? ClanStat { get; set; }
        public virtual DbSet<ClanTeamChallenge>? ClanTeamChallenge { get; set; }
        public virtual DbSet<ClanCustomStat>? ClanCustomStat { get; set; }
        public virtual DbSet<PostDebugInfo>? DebugInfo { get; set; }
        public virtual DbSet<DimAnnouncements>? DimAnnouncements { get; set; }
        public virtual DbSet<DimAppGroups>? DimAppGroups { get; set; }
        public virtual DbSet<DimAppIds>? DimAppIds { get; set; }
        public virtual DbSet<DimEula>? DimEula { get; set; }
        public virtual DbSet<DimStats>? DimStats { get; set; }
        public virtual DbSet<DimClanStats>? DimClanStats { get; set; }
        public virtual DbSet<DimCustomStats>? DimCustomStats { get; set; }
        public virtual DbSet<DimClanCustomStats>? DimClanCustomStats { get; set; }
        public virtual DbSet<Files>? Files { get; set; }
        public virtual DbSet<FileAttributes>? FileAttributes { get; set; }
        public virtual DbSet<FileMetaData>? FileMetaDatas { get; set; }
        public virtual DbSet<Game>? Game { get; set; }
        public virtual DbSet<GameHistory>? GameHistory { get; set; }
        public virtual DbSet<Channels>? Channels { get; set; }
        public virtual DbSet<Locations>? Locations { get; set; }
        public virtual DbSet<NpId>? NpIds { get; set; }
        public virtual DbSet<Roles>? Roles { get; set; }
        public virtual DbSet<ServerFlags>? ServerFlags { get; set; }
        public virtual DbSet<ServerLog>? ServerLog { get; set; }
        public virtual DbSet<ServerSetting>? ServerSettings { get; set; }
        public virtual DbSet<UserRole>? UserRole { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .ToTable("account", "ACCOUNTS");

            modelBuilder.Entity<Account>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<Account>()
                .Property(e => e.AccountName)
                .IsRequired()
                .HasColumnName("account_name")
                .HasMaxLength(32);

            modelBuilder.Entity<Account>()
                .Property(e => e.AccountPassword)
                .IsRequired()
                .HasColumnName("account_password")
                .HasMaxLength(200);

            modelBuilder.Entity<Account>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<Account>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<Account>()
                .Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            modelBuilder.Entity<Account>()
                .Property(e => e.LastSignInDt)
                .HasColumnName("last_sign_in_dt");

            modelBuilder.Entity<Account>()
                .Property(e => e.LastSignInIp)
                .HasColumnName("last_sign_in_ip")
                .HasMaxLength(50);

            modelBuilder.Entity<Account>()
                .Property(e => e.MachineId)
                .HasColumnName("machine_id")
                .HasMaxLength(100);

            modelBuilder.Entity<Account>()
                .Property(e => e.MediusStats)
                .HasColumnName("medius_stats")
                .HasMaxLength(350);

            modelBuilder.Entity<Account>()
                .Property(e => e.ResetPasswordOnNextLogin)
                .IsRequired()
                .HasColumnName("reset_password_on_next_login");

            modelBuilder.Entity<Account>()
                .Property(e => e.Metadata)
                .HasColumnName("metadata");

            modelBuilder.Entity<Account>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<AccountFriendInvitations>()
                .ToTable("account_friend_invitations", "ACCOUNTS");

            modelBuilder.Entity<AccountFriendInvitations>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<AccountFriendInvitations>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<AccountFriendInvitations>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            modelBuilder.Entity<AccountFriendInvitations>()
                .Property(e => e.AccountName)
                .HasColumnName("account_name")
                .HasMaxLength(32)
                .IsRequired();

            modelBuilder.Entity<AccountFriendInvitations>()
                .Property(e => e.FriendAccountId)
                .HasColumnName("friend_account_id");

            modelBuilder.Entity<AccountFriendInvitations>()
                .Property(e => e.MediusBuddyAddType)
                .HasColumnName("buddy_add_type");

            modelBuilder.Entity<AccountFriendInvitations>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<AccountFriend>()
                .ToTable("account_friend", "ACCOUNTS");

            modelBuilder.Entity<AccountFriend>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<AccountFriend>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<AccountFriend>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<AccountFriend>()
                .Property(e => e.FriendAccountId)
                .HasColumnName("friend_account_id");

            modelBuilder.Entity<AccountFriend>()
                .HasRequired(d => d.Account)
                .WithMany(p => p.AccountFriend)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountIgnored>()
                .ToTable("account_ignored", "ACCOUNTS");

            modelBuilder.Entity<AccountIgnored>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<AccountIgnored>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<AccountIgnored>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<AccountIgnored>()
                .Property(e => e.IgnoredAccountId)
                .HasColumnName("ignored_account_id");

            modelBuilder.Entity<AccountIgnored>()
                .HasRequired(d => d.Account)
                .WithMany(p => p.AccountIgnored)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountStat>()
                .ToTable("account_stat", "STATS");

            modelBuilder.Entity<AccountStat>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<AccountStat>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<AccountStat>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<AccountStat>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<AccountStat>()
                .Property(e => e.StatValue)
                .HasColumnName("stat_value");

            modelBuilder.Entity<AccountStat>()
                .HasRequired(d => d.Account)
                .WithMany(p => p.AccountStat)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountStat>()
                .HasRequired(d => d.Stat)
                .WithMany(p => p.AccountStat)
                .HasForeignKey(d => d.StatId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountCustomStat>()
                .ToTable("account_custom_stat", "STATS");

            modelBuilder.Entity<AccountCustomStat>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<AccountCustomStat>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<AccountCustomStat>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<AccountCustomStat>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<AccountCustomStat>()
                .Property(e => e.StatValue)
                .HasColumnName("stat_value");

            modelBuilder.Entity<AccountCustomStat>()
                .HasRequired(d => d.Account)
                .WithMany(p => p.AccountCustomStat)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountCustomStat>()
                .HasRequired(d => d.Stat)
                .WithMany(p => p.AccountCustomStat)
                .HasForeignKey(d => d.StatId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountStatus>()
                .ToTable("account_status", "ACCOUNTS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.ChannelId)
                .HasColumnName("channel_id");

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.GameId)
                .HasColumnName("game_id");

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.GameName)
                .HasColumnName("game_name")
                .HasMaxLength(32);

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.LoggedIn)
                .HasColumnName("logged_in");

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.WorldId)
                .HasColumnName("world_id");

            modelBuilder.Entity<AccountStatus>()
                .Property(e => e.DatabaseUser)
                .HasColumnName("database_user")
                .HasMaxLength(32);

            modelBuilder.Entity<Banned>()
                .ToTable("banned", "ACCOUNTS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<Banned>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Banned>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<Banned>()
                .Property(e => e.FromDt)
                .HasColumnName("from_dt");

            modelBuilder.Entity<Banned>()
                .Property(e => e.ToDt)
                .HasColumnName("to_dt");

            modelBuilder.Entity<BannedIp>()
                .ToTable("banned_ip", "ACCOUNTS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<BannedIp>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<BannedIp>()
                .Property(e => e.FromDt)
                .HasColumnName("from_dt");

            modelBuilder.Entity<BannedIp>()
                .Property(e => e.IpAddress)
                .IsRequired()
                .HasColumnName("ip_address")
                .HasMaxLength(50);

            modelBuilder.Entity<BannedIp>()
                .Property(e => e.ToDt)
                .HasColumnName("to_dt");

            modelBuilder.Entity<BannedMac>()
                .ToTable("banned_mac", "ACCOUNTS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<BannedMac>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<BannedMac>()
                .Property(e => e.FromDt)
                .HasColumnName("from_dt");

            modelBuilder.Entity<BannedMac>()
                .Property(e => e.MacAddress)
                .IsRequired()
                .HasColumnName("mac_address")
                .HasMaxLength(50);

            modelBuilder.Entity<BannedMac>()
                .Property(e => e.ToDt)
                .HasColumnName("to_dt");

            modelBuilder.Entity<Clan>()
                .ToTable("clan", "CLANS")
                .HasKey(e => e.ClanId);

            modelBuilder.Entity<Clan>()
                .Property(e => e.ClanId)
                .HasColumnName("clan_id");

            modelBuilder.Entity<Clan>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<Clan>()
                .Property(e => e.ClanLeaderAccountId)
                .HasColumnName("clan_leader_account_id");

            modelBuilder.Entity<Clan>()
                .Property(e => e.ClanName)
                .IsRequired()
                .HasColumnName("clan_name")
                .HasMaxLength(32);

            modelBuilder.Entity<Clan>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<Clan>()
                .Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            modelBuilder.Entity<Clan>()
                .Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            modelBuilder.Entity<Clan>()
                .Property(e => e.MediusStats)
                .HasColumnName("medius_stats")
                .HasMaxLength(350);

            modelBuilder.Entity<Clan>()
                .Property(e => e.ModifiedBy)
                .HasColumnName("modified_by");

            modelBuilder.Entity<Clan>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<Clan>()
                .HasRequired(d => d.ClanLeaderAccount)
                .WithMany(p => p.Clan)
                .HasForeignKey(d => d.ClanLeaderAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanInvitation>()
                .ToTable("clan_invitation", "CLANS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.ClanId)
                .HasColumnName("clan_id");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.ModifiedBy)
                .HasColumnName("modified_by");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.ResponseDt)
                .HasColumnName("response_dt");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.ResponseId)
                .HasColumnName("response_id");

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.ResponseMsg)
                .HasColumnName("response_msg")
                .HasMaxLength(50);

            modelBuilder.Entity<ClanInvitation>()
                .Property(e => e.InviteMsg)
                .HasColumnName("invite_msg")
                .HasMaxLength(512);

            modelBuilder.Entity<ClanInvitation>()
                .HasRequired(d => d.Account)
                .WithMany(p => p.ClanInvitation)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanInvitation>()
                .HasRequired(d => d.Clan)
                .WithMany(p => p.ClanInvitation)
                .HasForeignKey(d => d.ClanId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanMember>()
                .ToTable("clan_member", "CLANS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<ClanMember>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ClanMember>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<ClanMember>()
                .Property(e => e.ClanId)
                .HasColumnName("clan_id");

            modelBuilder.Entity<ClanMember>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<ClanMember>()
                .Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            modelBuilder.Entity<ClanMember>()
                .Property(e => e.ModifiedBy)
                .HasColumnName("modified_by");

            modelBuilder.Entity<ClanMember>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<ClanMember>()
                .HasRequired(d => d.Account)
                .WithMany(p => p.ClanMember)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanMember>()
                .HasRequired(d => d.Clan)
                .WithMany(p => p.ClanMember)
                .HasForeignKey(d => d.ClanId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanMessage>()
                .ToTable("clan_message", "CLANS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<ClanMessage>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ClanMessage>()
                .Property(e => e.ClanId)
                .HasColumnName("clan_id");

            modelBuilder.Entity<ClanMessage>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<ClanMessage>()
                .Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            modelBuilder.Entity<ClanMessage>()
                .Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            modelBuilder.Entity<ClanMessage>()
                .Property(e => e.Message)
                .IsRequired()
                .HasColumnName("message")
                .HasMaxLength(200);

            modelBuilder.Entity<ClanMessage>()
                .HasRequired(d => d.Clan)
                .WithMany(p => p.ClanMessage)
                .HasForeignKey(d => d.ClanId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanTeamChallenge>()
                .ToTable("clan_team_challenge", "CLANS");

            modelBuilder.Entity<ClanTeamChallenge>()
                .HasKey(e => e.ClanChallengeId);

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.ClanChallengeId)
                .HasColumnName("clan_challenge_id");

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.ChallengerClanID)
                .HasColumnName("challenger_clan_id");

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.AgainstClanID)
                .HasColumnName("against_clan_id");

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("int");

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.ResponseTime)
                .HasColumnName("response_time")
                .HasColumnType("int");

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.ChallengeMsg)
                .HasColumnName("challenge_msg")
                .HasMaxLength(200);

            modelBuilder.Entity<ClanTeamChallenge>()
                .Property(e => e.ResponseMessage)
                .HasColumnName("response_msg")
                .HasMaxLength(200);

            modelBuilder.Entity<ClanStat>()
                .ToTable("clan_stat", "STATS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<ClanStat>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ClanStat>()
                .Property(e => e.ClanId)
                .HasColumnName("clan_id");

            modelBuilder.Entity<ClanStat>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<ClanStat>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<ClanStat>()
                .Property(e => e.StatValue)
                .HasColumnName("stat_value");

            modelBuilder.Entity<ClanStat>()
                .HasRequired(d => d.Clan)
                .WithMany(p => p.ClanStat)
                .HasForeignKey(d => d.ClanId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanStat>()
                .HasRequired(d => d.Stat)
                .WithMany(p => p.ClanStat)
                .HasForeignKey(d => d.StatId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanCustomStat>()
                .ToTable("clan_custom_stat", "STATS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<ClanCustomStat>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ClanCustomStat>()
                .Property(e => e.ClanId)
                .HasColumnName("clan_id");

            modelBuilder.Entity<ClanCustomStat>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<ClanCustomStat>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<ClanCustomStat>()
                .Property(e => e.StatValue)
                .HasColumnName("stat_value");

            modelBuilder.Entity<ClanCustomStat>()
                .HasRequired(d => d.Clan)
                .WithMany(p => p.ClanCustomStat)
                .HasForeignKey(d => d.ClanId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClanCustomStat>()
                .HasRequired(d => d.Stat)
                .WithMany(p => p.ClanCustomStat)
                .HasForeignKey(d => d.StatId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DimAnnouncements>()
                .ToTable("dim_announcements", "KEYS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.AnnouncementBody)
                .IsRequired()
                .HasColumnName("announcement_body")
                .HasMaxLength(1000);

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.AnnouncementTitle)
                .IsRequired()
                .HasColumnName("announcement_title")
                .HasMaxLength(50);

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.FromDt)
                .HasColumnName("from_dt");

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.ToDt)
                .HasColumnName("to_dt");

            modelBuilder.Entity<DimAnnouncements>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<DimAppGroups>()
                .HasKey(e => e.GroupId)
                .ToTable("dim_app_groups", "KEYS");

            modelBuilder.Entity<DimAppGroups>()
                .Property(e => e.GroupId)
                .HasColumnName("group_id");

            modelBuilder.Entity<DimAppGroups>()
                .Property(e => e.GroupName)
                .IsRequired()
                .HasColumnName("group_name")
                .HasMaxLength(250);

            modelBuilder.Entity<DimAppIds>()
                .HasKey(e => e.AppId)
                .ToTable("dim_app_ids", "KEYS");

            modelBuilder.Entity<DimAppIds>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<DimAppIds>()
                .Property(e => e.AppName)
                .IsRequired()
                .HasColumnName("app_name")
                .HasMaxLength(250);

            modelBuilder.Entity<DimAppIds>()
                .Property(e => e.GroupId)
                .HasColumnName("group_id");

            modelBuilder.Entity<Files>()
                .ToTable("files", "FILESERVICES")
                .HasKey(e => e.Id);

            modelBuilder.Entity<Files>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Files>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<Files>()
                .Property(e => e.FileName)
                .IsRequired()
                .HasColumnName("FileName")
                .HasMaxLength(128);

            modelBuilder.Entity<Files>()
                .Property(e => e.ServerChecksum)
                .HasColumnName("ServerChecksum")
                .HasMaxLength(48);

            modelBuilder.Entity<Files>()
                .Property(e => e.FileID)
                .HasColumnName("FileID");

            modelBuilder.Entity<Files>()
                .Property(e => e.FileSize)
                .HasColumnName("FileSize");

            modelBuilder.Entity<Files>()
                .Property(e => e.CreationTimeStamp)
                .HasColumnName("CreationTimeStamp");

            modelBuilder.Entity<Files>()
                .Property(e => e.OwnerID)
                .HasColumnName("OwnerID");

            modelBuilder.Entity<Files>()
                .Property(e => e.GroupID)
                .HasColumnName("GroupID");

            modelBuilder.Entity<Files>()
                .Property(e => e.OwnerPermissionRWX)
                .HasColumnName("OwnerPermissionRWX");

            modelBuilder.Entity<Files>()
                .Property(e => e.GroupPermissionRWX)
                .HasColumnName("GroupPermissionRWX");

            modelBuilder.Entity<Files>()
                .Property(e => e.GlobalPermissionRWX)
                .HasColumnName("GlobalPermissionRWX");

            modelBuilder.Entity<Files>()
                .Property(e => e.ServerOperationID)
                .HasColumnName("ServerOperationID");

            modelBuilder.Entity<Files>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<Files>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<FileAttributes>()
                .ToTable("files_attributes", "FILESERVICES")
                .HasKey(e => e.FileID);

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.ID)
                .HasColumnName("id");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<FileAttributes>()
               .Property(e => e.FileID)
               .HasColumnName("FileID");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.FileName)
                .IsRequired()
                .HasColumnName("FileName")
                .HasMaxLength(128);

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.LastChangedTimeStamp)
                .HasColumnName("LastChangedTimeStamp");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.LastChangedByUserID)
                .HasColumnName("LastChangedByUserID");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.NumberAccesses)
                .HasColumnName("NumberAccesses");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.StreamableFlag)
                .HasColumnName("StreamableFlag");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.StreamingDataRate)
                .HasColumnName("StreamingDataRate");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<FileAttributes>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<FileMetaData>()
                .ToTable("files_metadata", "FILESERVICES")
                .HasKey(e => e.Id);

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.FileID)
                .IsRequired()
                .HasColumnName("FileID");

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.FileName)
                .IsRequired()
                .HasColumnName("FileName")
                .HasMaxLength(128);

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.Key)
                .HasColumnName("meta_key")
                .HasMaxLength(56);

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.Value)
                .HasColumnName("meta_value")
                .HasMaxLength(256);

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<FileMetaData>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<DimEula>()
                .ToTable("dim_eula", "KEYS")
                .HasKey(e => e.Id);

            modelBuilder.Entity<DimEula>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<DimEula>()
                .Property(e => e.PolicyType)
                .HasColumnName("policy_type");

            modelBuilder.Entity<DimEula>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<DimEula>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<DimEula>()
                .Property(e => e.EulaBody)
                .IsRequired()
                .HasColumnName("eula_body");

            modelBuilder.Entity<DimEula>()
                .Property(e => e.EulaTitle)
                .IsRequired()
                .HasColumnName("eula_title")
                .HasMaxLength(50);

            modelBuilder.Entity<DimEula>()
                .Property(e => e.FromDt)
                .HasColumnName("from_dt");

            modelBuilder.Entity<DimEula>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<DimEula>()
                .Property(e => e.ToDt)
                .HasColumnName("to_dt");

            modelBuilder.Entity<DimStats>()
                .HasKey(e => e.StatId)
                .ToTable("dim_stats", "KEYS");

            modelBuilder.Entity<DimStats>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<DimStats>()
                .Property(e => e.DefaultValue)
                .HasColumnName("default_value");

            modelBuilder.Entity<DimStats>()
                .Property(e => e.StatName)
                .IsRequired()
                .HasColumnName("stat_name")
                .HasMaxLength(100);

            modelBuilder.Entity<DimCustomStats>()
                .HasKey(e => e.StatId)
                .ToTable("dim_custom_stats", "KEYS");

            modelBuilder.Entity<DimCustomStats>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<DimCustomStats>()
                .Property(e => e.DefaultValue)
                .HasColumnName("default_value");

            modelBuilder.Entity<DimCustomStats>()
                .Property(e => e.StatName)
                .IsRequired()
                .HasColumnName("stat_name")
                .HasMaxLength(100);

            modelBuilder.Entity<DimCustomStats>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<DimClanStats>()
                .HasKey(e => e.StatId)
                .ToTable("dim_clan_stats", "KEYS");

            modelBuilder.Entity<DimClanStats>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<DimClanStats>()
                .Property(e => e.DefaultValue)
                .HasColumnName("default_value");

            modelBuilder.Entity<DimClanStats>()
                .Property(e => e.StatName)
                .IsRequired()
                .HasColumnName("stat_name")
                .HasMaxLength(100);

            modelBuilder.Entity<DimClanCustomStats>()
                .HasKey(e => e.StatId)
                .ToTable("dim_clan_custom_stats", "KEYS");

            modelBuilder.Entity<DimClanCustomStats>()
                .Property(e => e.StatId)
                .HasColumnName("stat_id");

            modelBuilder.Entity<DimClanCustomStats>()
                .Property(e => e.DefaultValue)
                .HasColumnName("default_value");

            modelBuilder.Entity<DimClanCustomStats>()
                .Property(e => e.StatName)
                .IsRequired()
                .HasColumnName("stat_name")
                .HasMaxLength(100);

            modelBuilder.Entity<DimClanCustomStats>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<Game>()
                .ToTable("game", "WORLD")
                .HasKey(e => e.Id);

            modelBuilder.Entity<Game>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Game>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<Game>()
                .Property(e => e.GameCreateDt)
                .HasColumnName("game_create_dt");

            modelBuilder.Entity<Game>()
                .Property(e => e.GameHostType)
                .IsRequired()
                .HasColumnName("game_host_type")
                .HasMaxLength(32);

            modelBuilder.Entity<Game>()
                .Property(e => e.GameId)
                .HasColumnName("game_id");

            modelBuilder.Entity<Game>()
                .Property(e => e.GameLevel)
                .HasColumnName("game_level");

            modelBuilder.Entity<Game>()
                .Property(e => e.GameName)
                .IsRequired()
                .HasColumnName("game_name")
                .HasMaxLength(64);

            modelBuilder.Entity<Game>()
                .Property(e => e.GameStartDt)
                .HasColumnName("game_start_dt");

            modelBuilder.Entity<Game>()
                .Property(e => e.GameStats)
                .IsRequired()
                .HasColumnName("game_stats")
                .HasMaxLength(256)
                .IsFixedLength();

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField1)
                .HasColumnName("generic_field_1");

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField2)
                .HasColumnName("generic_field_2");

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField3)
                .HasColumnName("generic_field_3");

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField4)
                .HasColumnName("generic_field_4");

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField5)
                .HasColumnName("generic_field_5");

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField6)
                .HasColumnName("generic_field_6");

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField7)
                .HasColumnName("generic_field_7");

            modelBuilder.Entity<Game>()
                .Property(e => e.GenericField8)
                .HasColumnName("generic_field_8");

            modelBuilder.Entity<Game>()
                .Property(e => e.MaxPlayers)
                .HasColumnName("max_players");

            modelBuilder.Entity<Game>()
                .Property(e => e.Metadata)
                .HasColumnName("metadata");

            modelBuilder.Entity<Game>()
                .Property(e => e.MinPlayers)
                .HasColumnName("min_players");

            modelBuilder.Entity<Game>()
                .Property(e => e.PlayerCount)
                .HasColumnName("player_count");

            modelBuilder.Entity<Game>()
                .Property(e => e.PlayerListCurrent)
                .HasColumnName("player_list_current")
                .HasMaxLength(250);

            modelBuilder.Entity<Game>()
                .Property(e => e.PlayerListStart)
                .HasColumnName("player_list_start")
                .HasMaxLength(250);

            modelBuilder.Entity<Game>()
                .Property(e => e.PlayerSkillLevel)
                .HasColumnName("player_skill_level");

            modelBuilder.Entity<Game>()
                .Property(e => e.RuleSet)
                .HasColumnName("rule_set");

            modelBuilder.Entity<Game>()
                .Property(e => e.WorldStatus)
                .IsRequired()
                .HasColumnName("world_status")
                .HasMaxLength(32);

            modelBuilder.Entity<Game>()
                .Property(e => e.DatabaseUser)
                .HasColumnName("database_user")
                .HasMaxLength(32);

            modelBuilder.Entity<GameHistory>()
                .ToTable("game_history", "WORLD")
                .HasKey(e => e.Id);

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameCreateDt)
                .HasColumnName("game_create_dt");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameEndDt)
                .HasColumnName("game_end_dt");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameHostType)
                .IsRequired()
                .HasColumnName("game_host_type")
                .HasMaxLength(32);

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameId)
                .HasColumnName("game_id");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameLevel)
                .HasColumnName("game_level");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameName)
                .IsRequired()
                .HasColumnName("game_name")
                .HasMaxLength(64);

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameStartDt)
                .HasColumnName("game_start_dt");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GameStats)
                .IsRequired()
                .HasColumnName("game_stats")
                .HasMaxLength(256)
                .IsFixedLength();

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField1)
                .HasColumnName("generic_field_1");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField2)
                .HasColumnName("generic_field_2");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField3)
                .HasColumnName("generic_field_3");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField4)
                .HasColumnName("generic_field_4");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField5)
                .HasColumnName("generic_field_5");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField6)
                .HasColumnName("generic_field_6");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField7)
                .HasColumnName("generic_field_7");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.GenericField8)
                .HasColumnName("generic_field_8");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.MaxPlayers)
                .HasColumnName("max_players");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.Metadata)
                .HasColumnName("metadata");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.MinPlayers)
                .HasColumnName("min_players");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.PlayerCount)
                .HasColumnName("player_count");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.PlayerListCurrent)
                .HasColumnName("player_list_current")
                .HasMaxLength(250);

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.PlayerListStart)
                .HasColumnName("player_list_start")
                .HasMaxLength(250);

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.PlayerSkillLevel)
                .HasColumnName("player_skill_level");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.RuleSet)
                .HasColumnName("rule_set");

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.WorldStatus)
                .IsRequired()
                .HasColumnName("world_status")
                .HasMaxLength(32);

            modelBuilder.Entity<GameHistory>()
                .Property(e => e.DatabaseUser)
                .HasColumnName("database_user")
                .HasMaxLength(32);

            modelBuilder.Entity<Locations>()
                .HasKey(e => new { e.Id, e.AppId })
                .ToTable("locations", "WORLD");

            modelBuilder.Entity<Locations>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Locations>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<Locations>()
                .Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(50);

            modelBuilder.Entity<Channels>()
                .HasKey(e => new { e.Id, e.AppId })
                .ToTable("channels", "WORLD");

            modelBuilder.Entity<Channels>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Channels>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<Channels>()
                .Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(50);

            modelBuilder.Entity<Channels>()
                .Property(e => e.MaxPlayers)
                .HasColumnName("max_players");

            modelBuilder.Entity<Channels>()
                .Property(e => e.GenericField1)
                .HasColumnName("generic_field_1");

            modelBuilder.Entity<Channels>()
                .Property(e => e.GenericField2)
                .HasColumnName("generic_field_2");

            modelBuilder.Entity<Channels>()
                .Property(e => e.GenericField3)
                .HasColumnName("generic_field_3");

            modelBuilder.Entity<Channels>()
                .Property(e => e.GenericField4)
                .HasColumnName("generic_field_4");

            modelBuilder.Entity<Channels>()
                .Property(e => e.GenericFieldFilter)
                .HasColumnName("generic_field_filter");

            modelBuilder.Entity<NpId>()
                .HasKey(e => e.AppId)
                .ToTable("account_npids", "ACCOUNTS");

            modelBuilder.Entity<NpId>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<NpId>()
                .Property(e => e.data)
                .IsRequired()
                .HasColumnName("data")
                .HasMaxLength(16);

            modelBuilder.Entity<NpId>()
                .Property(e => e.term)
                .HasColumnName("term");

            modelBuilder.Entity<NpId>()
                .Property(e => e.dummy)
                .HasColumnName("dummy")
                .HasMaxLength(3);

            modelBuilder.Entity<NpId>()
                .Property(e => e.opt)
                .HasColumnName("opt")
                .HasMaxLength(8);

            modelBuilder.Entity<NpId>()
                .Property(e => e.reserved)
                .HasColumnName("reserved")
                .HasMaxLength(8);

            modelBuilder.Entity<NpId>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<NpId>()
                .Property(e => e.ModifiedDt)
                .HasColumnName("modified_dt");

            modelBuilder.Entity<Roles>()
                .HasKey(e => e.RoleId)
                .ToTable("roles", "KEYS");

            modelBuilder.Entity<Roles>()
                .Property(e => e.RoleId)
                .HasColumnName("role_id");

            modelBuilder.Entity<Roles>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<Roles>()
                .Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            modelBuilder.Entity<Roles>()
                .Property(e => e.RoleName)
                .IsRequired()
                .HasColumnName("role_name")
                .HasMaxLength(50);

            modelBuilder.Entity<ServerFlags>()
                .ToTable("server_flags", "KEYS");

            modelBuilder.Entity<ServerFlags>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ServerFlags>()
                .Property(e => e.FromDt)
                .HasColumnName("from_dt");

            modelBuilder.Entity<ServerFlags>()
                .Property(e => e.ServerFlag)
                .IsRequired()
                .HasColumnName("server_flag")
                .HasMaxLength(50);

            modelBuilder.Entity<ServerFlags>()
                .Property(e => e.ToDt)
                .HasColumnName("to_dt");

            modelBuilder.Entity<ServerFlags>()
                .Property(e => e.Value)
                .IsRequired()
                .HasColumnName("value")
                .HasMaxLength(100);

            modelBuilder.Entity<ServerLog>()
                .ToTable("server_log", "LOGS");

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.LogDt)
                .HasColumnName("log_dt");

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.LogMsg)
                .IsRequired()
                .HasColumnName("log_msg");

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.LogStacktrace)
                .HasColumnName("log_stacktrace");

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.LogTitle)
                .IsRequired()
                .HasColumnName("log_title")
                .HasMaxLength(200);

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.MethodName)
                .HasColumnName("method_name")
                .HasMaxLength(50);

            modelBuilder.Entity<ServerLog>()
                .Property(e => e.Payload)
                .HasColumnName("payload");

            modelBuilder.Entity<ServerSetting>()
                .HasKey(e => new { e.AppId, e.Name })
                .ToTable("server_settings", "KEYS");

            modelBuilder.Entity<ServerSetting>()
                .Property(e => e.AppId)
                .HasColumnName("app_id");

            modelBuilder.Entity<ServerSetting>()
                .Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(250);

            modelBuilder.Entity<ServerSetting>()
                .Property(e => e.Value)
                .HasColumnName("value");

            modelBuilder.Entity<UserRole>()
                .ToTable("user_role", "ACCOUNTS");

            modelBuilder.Entity<UserRole>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<UserRole>()
                .Property(e => e.AccountId)
                .HasColumnName("account_id");

            modelBuilder.Entity<UserRole>()
                .Property(e => e.CreateDt)
                .HasColumnName("create_dt");

            modelBuilder.Entity<UserRole>()
                .Property(e => e.FromDt)
                .HasColumnName("from_dt");

            modelBuilder.Entity<UserRole>()
                .Property(e => e.RoleId)
                .HasColumnName("role_id");

            modelBuilder.Entity<UserRole>()
                .Property(e => e.ToDt)
                .HasColumnName("to_dt");

            Database.SetInitializer(new SqliteCreateDatabaseIfNotExists<MiddlewareSQLiteContext>(modelBuilder));
        }
    }
}
