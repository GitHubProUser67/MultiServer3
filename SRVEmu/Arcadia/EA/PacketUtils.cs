using System.Text;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;

namespace SRVEmu.Arcadia.EA;

public static class PacketUtils
{
    private static readonly SecureRandom Random = new();

    public static string GenerateSalt()
    {
        return Random.NextInt64(100000000, 999999999).ToString();
    }

    public static byte[] BuildPacketHeader(string type, uint transmissionType, uint packetId, string data)
    {
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);

        uint maskedTransmissionType = (transmissionType << 24) & 0xFF000000; // ensure it only occupies the high-order byte
        uint maskedPacketId = packetId & 0x00FFFFFF; // make sure it fits in the lower 3 bytes
        byte[] transmissionTypePacketIdBytes = UintToBytes(maskedTransmissionType | maskedPacketId);

        byte[] packetLengthBytes = CalculatePacketLength(data);

        return typeBytes.Concat(transmissionTypePacketIdBytes).Concat(packetLengthBytes).ToArray();
    }

    private static byte[] CalculatePacketLength(string packetData)
    {
        byte[] dataBytes = Encoding.ASCII.GetBytes(packetData);

        int length = dataBytes.Length + 12;
        byte[] bytes = BitConverter.GetBytes(length);

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

    public static async Task<(int, byte[])> ReadApplicationDataAsync(TlsServerProtocol network)
    {
        byte[] readBuffer = new byte[8096];
        try
        {
            int read = await Task.Run(() => network.ReadApplicationData(readBuffer, 0, readBuffer.Length));
            return (read, readBuffer);
        }
        catch
        {
            throw new Exception("[Arcadia:PacketUtils] - Connection has been closed");
        }
    }

    public static Dictionary<string, object> ParseFeslPacketToDict(byte[] data)
    {
        string dataString = Encoding.ASCII.GetString(data);

        var dataSplit = dataString.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x.Replace("\0", string.Empty))).ToArray();
        dataSplit = dataSplit.Select(x => x.Replace("\0", string.Empty)).ToArray();

        var dataDict = new Dictionary<string, object>();
        for (var i = 0; i < dataSplit.Length; i++)
        {
            var entrySplit = dataSplit[i].Split('=', StringSplitOptions.TrimEntries);

            string parameter = entrySplit[0];
            string value = entrySplit.Length > 1 ? entrySplit[1].Replace(@"\", string.Empty) : string.Empty;

            dataDict.TryAdd(parameter, value);
        }

        return dataDict;
    }

    public static StringBuilder DataDictToPacketString(Dictionary<string, object> packetData)
    {
        var dataBuilder = new StringBuilder();

        for (var i = 0; i < packetData.Count; i++)
        {
            var line = packetData.ElementAt(i);
            string parameter = line.Key;
            var value = line.Value;

            dataBuilder.Append(parameter).Append('=');

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
}