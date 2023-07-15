namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    public interface IMediusRequest
    {
        MessageId MessageID { get; set; }
    }

    public interface IMediusResponse
    {
        MessageId MessageID { get; set; }

        bool IsSuccess { get; }
    }
}