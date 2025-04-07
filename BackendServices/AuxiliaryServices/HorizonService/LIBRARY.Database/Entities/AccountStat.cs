using System;

namespace Horizon.LIBRARY.Database.Entities
{
    public partial class AccountStat
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int StatId { get; set; }
        public int StatValue { get; set; }
        public DateTime? ModifiedDt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual DimStats? Stat { get; set; }
    }
}
