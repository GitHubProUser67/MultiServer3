namespace PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream
{
    public interface IStreamSerializer
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);

    }
}
