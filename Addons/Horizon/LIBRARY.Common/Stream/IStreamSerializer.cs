namespace MultiServer.Addons.Horizon.LIBRARY.Common.Stream
{
    public interface IStreamSerializer
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}
