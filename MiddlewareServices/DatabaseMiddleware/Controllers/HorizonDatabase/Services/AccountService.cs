using Horizon.LIBRARY.Database.Entities;
using Horizon.LIBRARY.Database.Models;

namespace DatabaseMiddleware.Controllers.HorizonDatabase.Services
{
    public class AccountService
    {
        public AccountDTO toAccountDTO(Account account)
        {
            return new AccountDTO()
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                Friends = account.AccountFriend.Select(f => toAccountRelationDTO(f.AccountId, f.Account.AccountName)).ToArray(),
                Ignored = account.AccountIgnored.Select(i => toAccountRelationDTO(i.AccountId, i.Account.AccountName)).ToArray(),
                AccountWideStats = account.AccountStat.OrderBy(a => a.StatId).Select(a => a.StatValue).ToArray(),
                MediusStats = account.MediusStats,
                MachineId = account.MachineId,
                AppId = account.AppId,
            };
        }

        public AccountRelationDTO toAccountRelationDTO(int AccountId, string AccountName)
        {
            return new AccountRelationDTO()
            {
                AccountId = AccountId,
                AccountName = AccountName,
            };
        }
    }
}
