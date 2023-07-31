namespace PSMultiServer.Addons.Medius.Server.Common.Stream
{
    public interface IStreamSerializer
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);

    }
}
