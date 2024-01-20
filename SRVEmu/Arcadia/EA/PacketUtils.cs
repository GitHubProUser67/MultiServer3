using System.Text;
using Org.BouncyCastle.Security;

namespace SRVEmu.Arcadia.EA;

public static class PacketUtils
{
    private static readonly SecureRandom Random = new();

    public static Dictionary<string, object> ParseFeslPacketToDict(byte[] data)
    {
        string[] dataSplit = Encoding.ASCII.GetString(data).Split('\n').Where(x => !string.IsNullOrWhiteSpace(x.Replace("\0", string.Empty))).ToArray();
        dataSplit = dataSplit.Select(x => x.Replace("\0", string.Empty)).ToArray();

        Dictionary<string, object> dataDict = new();
        for (int i = 0; i < dataSplit.Length; i++)
        {
            string[] entrySplit = dataSplit[i].Split('=', StringSplitOptions.TrimEntries);
            dataDict.TryAdd(entrySplit[0], entrySplit.Length > 1 ? entrySplit[1].Replace(@"\", string.Empty) : string.Empty);
        }

        return dataDict;
    }

    public static StringBuilder DataDictToPacketString(Dictionary<string, object> packetData)
    {
        StringBuilder dataBuilder = new();

        for (int i = 0; i < packetData.Count; i++)
        {
            var line = packetData.ElementAt(i);
            object value = line.Value;

            dataBuilder.Append(line.Key).Append('=');

            if (value.ToString()?.Contains(' ') == true)
                dataBuilder.Append('"').Append(value).Append('"');
            else
                dataBuilder.Append(value);

            dataBuilder.Append('\n');
        }

        if (dataBuilder.Length > 0)
            dataBuilder.Remove(dataBuilder.Length - 1, 1);

        dataBuilder.Append('\0');
        return dataBuilder;
    }

    public static string GenerateSalt()
    {
        return Random.NextInt64(100000000, 999999999).ToString();
    }

    public static byte[] BuildPacketHeader(string type, uint transmissionType, uint packetId, string data)
    {
        return Encoding.ASCII.GetBytes(type).Concat(UintToBytes((transmissionType << 24) & 0xFF000000 
            | packetId & 0x00FFFFFF)).Concat(CalculatePacketLength(data)).ToArray();
    }

    private static byte[] CalculatePacketLength(string packetData)
    {
        byte[] bytes = BitConverter.GetBytes(Encoding.ASCII.GetBytes(packetData).Length + 12);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return bytes;
    }

    private static byte[] UintToBytes(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return bytes;
    }
}