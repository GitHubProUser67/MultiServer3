namespace PSMultiServer.Addons.Horizon.Server.Common.Stream
{
    public interface IStreamSerializer
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);

    }
}
