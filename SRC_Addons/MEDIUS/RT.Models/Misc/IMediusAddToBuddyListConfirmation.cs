using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models.Misc
{
    public interface IMediusAddToBuddyListConfirmationRequest
    {
        MediusBuddyAddType AddType { get; }
        int TargetAccountID { get; }
    }

    public interface IMediusAddToBuddyListConfirmationResponse
    {
        MediusBuddyAddType AddType { get; }
        int OriginatorAccountID { get; }
    }
}