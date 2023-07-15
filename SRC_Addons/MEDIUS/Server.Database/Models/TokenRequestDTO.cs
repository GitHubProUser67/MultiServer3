using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;

namespace PSMultiServer.SRC_Addons.MEDIUS.Server.Database.Models
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
