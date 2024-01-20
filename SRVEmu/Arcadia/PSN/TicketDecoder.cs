using System.Text;

namespace SRVEmu.Arcadia.PSN;

public static class TicketDecoder
{
    public static TicketData[] DecodeFromASCIIString(string asciiString)
    {
        return DecodeFromBuffer(Convert.FromHexString(asciiString[1..])).Where(x => x.Type != TicketDataType.Empty).ToArray();
    }

    private static IEnumerable<TicketData> DecodeFromBuffer(byte[] payload)
    {
        using MemoryStream stream = new(payload);
        using BinaryReader reader = new(stream);

        while(reader.BaseStream.Position < reader.BaseStream.Length)
        {
            TicketData? ticket = ReadTicketData(reader);
            if (ticket is not null) yield return ticket;
        }
    }

    private static TicketData? ReadTicketData(BinaryReader reader)
    {
        ushort id = BitConverter.ToUInt16(BitConverter.GetBytes(reader.ReadUInt16()).Reverse().ToArray(), 0);
        ushort len = BitConverter.ToUInt16(BitConverter.GetBytes(reader.ReadUInt16()).Reverse().ToArray(), 0);
        TicketDataType ticketType = (TicketDataType)(id & 0x0FFF);

        CustomLogger.LoggerAccessor.LogDebug("[Arcadia] - TicketDecoder-ReadTicketData() id:0x{id:X}, len:{len}, type:{type}", id, len, ticketType);

        switch (ticketType)
        {
            case TicketDataType.Empty:
                return new EmptyData() { Id = id, Length = len };

            case TicketDataType.U32:
                byte[] u32Bytes = reader.ReadBytes(4);
                Array.Reverse(u32Bytes);
                return new U32Data { Value = BitConverter.ToUInt32(u32Bytes, 0), Id = id, Length = len };

            case TicketDataType.U64:
                byte[] u64Bytes = reader.ReadBytes(8);
                Array.Reverse(u64Bytes);
                return new U64Data { Value = BitConverter.ToUInt64(u64Bytes, 0), Id = id, Length = len };

            case TicketDataType.Time:
                byte[] timeBytes = reader.ReadBytes(8);
                Array.Reverse(timeBytes);
                return new TimeData { Value = BitConverter.ToUInt64(timeBytes, 0), Id = id, Length = len };

            case TicketDataType.Binary:
                return new BinaryData { Value = reader.ReadBytes(len), Id = id, Length = len };

            case TicketDataType.BString:
                return new BStringData { Value = Encoding.UTF8.GetString(reader.ReadBytes(len)), Id = id, Length = len };

            case TicketDataType.Blob:
                BlobData blobData = new()
                {
                    Tag = reader.ReadByte(),
                    Children = new List<TicketData>(),
                    Id = id,
                    Length = len
                };

                ushort remainingLength = (ushort)(len - 1); // Subtract 1 for the tag

                while (remainingLength > 0)
                {
                    CustomLogger.LoggerAccessor.LogDebug("[Arcadia] - TicketDecoder-ReadTicketData() Calling ReadTicketData() recursively!");
                    TicketData? child = ReadTicketData(reader);
                    CustomLogger.LoggerAccessor.LogDebug("[Arcadia] - TicketDecoder-ReadTicketData() Recursive ReadTicketData() exited!");

                    if (child is not null)
                    {
                        blobData.Children.Add(child);
                        remainingLength -= (ushort)(4 + child.Length); // 4 bytes for id and len
                        CustomLogger.LoggerAccessor.LogWarn("[Arcadia] - TicketDecoder-ReadTicketData() Failed to parse blob contents!");
                    }
                    else
                        return null;
                }

                return blobData;

            default:
                CustomLogger.LoggerAccessor.LogWarn("[Arcadia] - TicketDecoder-ReadTicketData() Unknown or unhandled type! id: 0x{id:X}, type:{type}", id, ticketType);
                reader.BaseStream.Seek(len, SeekOrigin.Current);  // Skip the unknown type
                return null;
        }
    }
}