using BackendProject;
using System.Text;

namespace SRVEmu.Arcadia.EA;

public readonly struct Packet
{
    public string TXN => DataDict["TXN"] as string ?? string.Empty;

    public string this[string key]
    {
        get => DataDict.GetValueOrDefault(key) as string ?? string.Empty;
        set => DataDict[key] = value;
    }

    public Packet(byte[] packet)
    {
        Type = Encoding.ASCII.GetString(packet, 0, 4);

        byte[][] firstSplit = MiscUtils.SplitAt(packet, 12);
        Checksum = firstSplit[0][4..];

        var bigEndianChecksum = (BitConverter.IsLittleEndian ? Checksum.Reverse().ToArray() : Checksum).AsSpan();
        Length = BitConverter.ToUInt32(bigEndianChecksum[..4]);
        uint idAndTransmissionType = BitConverter.ToUInt32(bigEndianChecksum[4..]);
        TransmissionType = idAndTransmissionType & 0xff000000;
        Id = idAndTransmissionType & 0x00ffffff;

        Data = firstSplit[1];
        DataDict = PacketUtils.ParseFeslPacketToDict(Data);
    }

    public Packet(string type, uint transmissionType, uint packetId, Dictionary<string, object>? dataDict = null)
    {
        Type = type.Trim();
        TransmissionType = transmissionType;
        Id = packetId;
        // TODO Packet length needs to be set here
        Length = 0;
        Data = null;
        Checksum = null;
        DataDict = dataDict ?? new Dictionary<string, object>();
    }

    public Packet Clone()
    {
        return new Packet(Type, TransmissionType, Id, DataDict);
    }

    public async Task<byte[]> Serialize()
    {
        string data = PacketUtils.DataDictToPacketString(DataDict).ToString();
        byte[] header = PacketUtils.BuildPacketHeader(Type, TransmissionType, Id, data);

        byte[] dataBytes = Encoding.ASCII.GetBytes(data);

        using MemoryStream response = new(header.Length + dataBytes.Length);

        await response.WriteAsync(header);
        await response.WriteAsync(dataBytes);
        await response.FlushAsync();

        return response.ToArray();
    }

    public string Type { get; }
    public uint Id { get; }
    public uint TransmissionType { get;  }
    public uint Length { get; }
    public Dictionary<string, object> DataDict { get; }
    public byte[]? Data { get; }
    public byte[]? Checksum { get; }
}