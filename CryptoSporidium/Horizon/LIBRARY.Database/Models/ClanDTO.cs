namespace CryptoSporidium.Horizon.LIBRARY.Database.Models
{
    public class ClanDTO
    {

        public int ClanId { get; set; }
        public string? ClanName { get; set; }
        public AccountDTO? ClanLeaderAccount { get; set; }
        public List<AccountDTO>? ClanMemberAccounts { get; set; }
        public List<ClanInvitationDTO>? ClanMemberInvitations { get; set; }
        public List<ClanMessageDTO>? ClanMessages { get; set; }
        public List<ClanTeamChallengeDTO>? ClanTeamChallenges { get; set; }
        public int AppId { get; set; }
        public bool IsDisbanded { get; set; }
        public string? ClanMediusStats { get; set; }

        /// <summary>
        /// Collection of ladder stats.
        /// </summary>
        public int[]? ClanStats { get; set; }

        /// <summary>
        /// Collection of ladder wide stats.
        /// </summary>
        public int[]? ClanWideStats { get; set; }

        /// <summary>
        /// Collection of ladder wide stats.
        /// </summary>
        public int[]? ClanCustomWideStats { get; set; }
    }

    public class CreateClanDTO
    {
        public string? ClanName { get; set; }
        public int AccountId { get; set; }
        public int AppId { get; set; }
    }

    public class ClanInvitationDTO
    {
        public int InvitationId { get; set; }
        public int ClanId { get; set; }
        public string? ClanName { get; set; }
        public int TargetAccountId { get; set; }
        public string? TargetAccountName { get; set; }
        public string? Message { get; set; }
        public int AppId { get; set; }
        public int ResponseStatus { get; set; }
        public int ResponseTime { get; set; }
        public string? ResponseMessage { get; set; }
    }

    public class AccountClanInvitationDTO
    {
        public int LeaderAccountId { get; set; }
        public string? LeaderAccountName { get; set; }
        public ClanInvitationDTO? Invitation { get; set; }
    }

    public class ClanInvitationResponseDTO
    {
        public int AccountId { get; set; }
        public int InvitationId { get; set; }
        public int Response { get; set; }
        public int ResponseTime { get; set; }
        public string? ResponseMessage { get; set; }
    }

    public class ClanMessageDTO
    {
        public int Id { get; set; }
        public string? Message { get; set; }
    }

    public class ClanTransferLeadershipDTO
    {
        public int ClanId { get; set; }
        public int AccountId { get; set; }
        public int NewLeaderAccountId { get; set; }
    }

    public class ClanTeamChallengeDTO
    {
        public int ChallengerClanID { get; set; }
        public int AgainstClanID { get; set; }
        public int Status { get; set; }
        public int ResponseTime { get; set; }
        public string? ChallengeMsg { get; set; }
        public string? ResponseMessage { get; set; }
        public int ClanChallengeId { get; set; }
    }
}