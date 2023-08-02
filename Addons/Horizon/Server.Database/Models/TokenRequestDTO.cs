using PSMultiServer.Addons.Horizon.RT.Common;

namespace PSMultiServer.Addons.Horizon.Server.Database.Models
{
    public partial class TokenRequestDTO
    {
        public MediusTokenActionType TokenAction { get; set; }
        public MediusTokenCategoryType TokenCategory { get; set; }
        public uint EntityID { get; set; }
        public int SubmitterAccountID { get; set; }
        public string TokenToReplace { get; set; }
        public string Token { get; set; }
    }
}
