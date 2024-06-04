using HashLib; //The only reason why HashLib is used is because rpcn uses sha224 to sign their tickets, which is not natively supported in .NET
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace XI5
{
    //https://www.psdevwiki.com/ps3/X-I-5-Ticket
    //https://github.com/RipleyTom/rpcn/blob/master/src/server/client/ticket.rs
    public class XI5Ticket
    {
        const uint XI5_VER_2_0 = 553648128;
        const uint XI5_VER_2_1 = 553713664;
        const uint XI5_VER_3_0 = 822083584;
        const uint XI5_VER_4_0 = 1090519040;

        private static IHash _sha224hasher;
        private static ECDsaSigner ECDsaRPCN;

        static XI5Ticket()
        {
            _sha224hasher = HashFactory.Crypto.CreateSHA224();
            PemReader pr = new(new StringReader("-----BEGIN PUBLIC KEY-----\r\nME4wEAYHKoZIzj0CAQYFK4EEACADOgAEsHvA8K3bl2V+nziQOejSucl9wqMdMELn\r\n0Eebk9gcQrCr32xCGRox4x+TNC+PAzvVKcLFf9taCn0=\r\n-----END PUBLIC KEY-----"));
            ECDsaRPCN = new ECDsaSigner();
            ECDsaRPCN.Init(false, (ECPublicKeyParameters)pr.ReadObject());
        }

        public string? TicketVersion { get; private set; }
        public string? Serial { get; private set; }
        public uint IssuerId { get; private set; }
        public DateTime? Issued { get; private set; }
        public DateTime? Expires { get; private set; }
        public ulong UserId { get; private set; }
        public string? OnlineId { get; private set; }
        public string? Region { get; private set; }
        public string? Domain { get; private set; }
        public string? ServiceId { get; private set; }
        public uint Status { get; private set; }
        public string? IssuerName { get; private set; }

        private byte[]? _fullBodyData;
        private byte[]? _signature;

        public XI5Ticket(byte[] data)
        {
            MemoryStream ms = new(data);

            uint version = ms.ReadUInt();

            if (version != XI5_VER_2_0 && version != XI5_VER_2_1 && version != XI5_VER_3_0 && version != XI5_VER_4_0)
                throw new NotSupportedException("Invalid ticket version: "+version); //invalid version

            TicketVersion = version == XI5_VER_2_0 ? "XI5_VER_2_0" : version == XI5_VER_2_1 ? "XI5_VER_2_1" : version == XI5_VER_3_0 ? "XI5_VER_3_0" : version == XI5_VER_4_0 ? "XI5_VER_4_0" : "UNKNOWN";
            if (version != XI5_VER_2_1)
            {
                Directory.CreateDirectory("bad_xi5");
                File.WriteAllBytes("bad_xi5/" + DateTime.Now.Ticks + ".bin", data);
                throw new NotImplementedException("Reading " + TicketVersion + " ticket is not yet implemented.");
            }

            uint size = ms.ReadUInt();
            if (size != ms.Length - 8) //invalid data
                throw new ArgumentException($"Specified ticket size: {size} | Actual ticket size : {ms.Length - 8}");

            ParseTicketV2_1(ms);
        }
        
        [MemberNotNull(nameof(Serial), nameof(OnlineId), nameof(Region), nameof(Domain), nameof(ServiceId), nameof(IssuerName), nameof(_signature), nameof(_fullBodyData))]
        private void ParseTicketV2_1(MemoryStream ms)
        {
            _fullBodyData = ReadFullBody(ms);
            byte[] footer = ReadFooter(ms);

            MemoryStream bodyStream = new(_fullBodyData);
            bodyStream.Seek(4, SeekOrigin.Begin); //skip existing dt and size
            
            Serial = ReadBinaryAsString(bodyStream);
            IssuerId = ReadUInt(bodyStream);
            Issued = ReadTime(bodyStream);
            Expires = ReadTime(bodyStream);
            UserId = ReadULong(bodyStream);
            OnlineId = ReadString(bodyStream);
            Region = ReadBinaryAsString(bodyStream);
            Domain = ReadString(bodyStream);
            ServiceId = ReadBinaryAsString(bodyStream);
            Status = ReadUInt(bodyStream);

            MemoryStream footerStream = new(footer);
            IssuerName = ReadBinaryAsString(footerStream);
            _signature = ReadBinary(footerStream);

            //TODO: check out this later
            //optionally there is also cookie (Binary Data type)
            //and 2 empty entries (Empty data type)
        }

        public bool SignedByOfficialRPCN { 
            get
            {
                if (IssuerId != 0x33333333) //they are using IssuerId = 0x33333333 as a constant
                    return false;

                Asn1InputStream decoder = new(_signature);
                if (decoder.ReadObject() is not DerSequence seq)
                    return false;

                HashResult hres = _sha224hasher.ComputeBytes(_fullBodyData);
                return ECDsaRPCN.VerifySignature(hres.GetBytes(), ((DerInteger)seq[0]).Value, ((DerInteger)seq[1]).Value);
            }
        }

        private static byte[] ReadFullField(Stream stream, Datatype expected)
        {
            Datatype dt = (Datatype)stream.ReadUShort();
            if (dt != expected)
                throw new InvalidDataException($"Expected datatype: {expected} | Actual datatype: {dt}");

            ushort size = stream.ReadUShort();
            byte[] data = new byte[size+4]; //with datatype and size included
            stream.Seek(-4, SeekOrigin.Current);
            if (!stream.ReadAll(data, 0, data.Length))
                throw new EndOfStreamException($"Failed to read {size} bytes from stream");
            return data;
        }

        private static byte[] ReadField(Stream stream, Datatype expected)
        {
            Datatype dt = (Datatype)stream.ReadUShort();
            if (dt != expected)
                throw new InvalidDataException($"Expected datatype: {expected} | Actual datatype: {dt}");

            ushort size = stream.ReadUShort();
            byte[] data = new byte[size];
            if (!stream.ReadAll(data, 0, size))
                throw new EndOfStreamException($"Failed to read {size} bytes from stream");
            return data;
        }

        private static byte[] ReadBody(Stream stream) => ReadField(stream, Datatype.Body);
        private static byte[] ReadFullBody(Stream stream) => ReadFullField(stream, Datatype.Body);
        private static byte[] ReadFooter(Stream stream) => ReadField(stream, Datatype.Footer);
        private static byte[] ReadFullFooter(Stream stream) => ReadFullField(stream, Datatype.Footer);
        private static byte[] ReadBinary(Stream stream) => ReadField(stream, Datatype.Binary);
        private static byte[] ReadFullBinary(Stream stream) => ReadFullField(stream, Datatype.Binary);
		
        private static string ReadBinaryAsString(Stream stream)
        {
            byte[] data = ReadBinary(stream);
            int inx = Array.FindIndex(data, 0, (x) => x == 0);//search for 0
            if (inx >= 0)
                return Encoding.UTF8.GetString(data, 0, inx);
            return Encoding.UTF8.GetString(data);
        }
		
        private static uint ReadUInt(Stream stream)
        {
            byte[] data = ReadField(stream, Datatype.UInt);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }
		
        private static DateTime ReadTime(Stream stream)
        {
            byte[] data = ReadField(stream, Datatype.Time);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return DateTimeOffset.FromUnixTimeMilliseconds((long)BitConverter.ToUInt64(data, 0)).UtcDateTime;
        }

        private static ulong ReadULong(Stream stream)
        {
            byte[] data = ReadField(stream, Datatype.ULong);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        private static string ReadString(Stream stream)
        {
            byte[] data = ReadField(stream, Datatype.String);
            int inx = Array.FindIndex(data, 0, (x) => x == 0);//search for 0
            if (inx >= 0)
                return Encoding.UTF8.GetString(data, 0, inx);
            return Encoding.UTF8.GetString(data);
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine("{");
            builder.AppendLine("    TicketVersion = " + TicketVersion);
            builder.AppendLine("    Serial = " + Serial);
            builder.AppendLine("    IssuerId = " + IssuerId);
            builder.AppendLine("    Issued = " + Issued);
            builder.AppendLine("    Expires = " + Expires);
            builder.AppendLine("    UserId = " + UserId);
            builder.AppendLine("    OnlineId = " + OnlineId);
            builder.AppendLine("    Region = " + Region);
            builder.AppendLine("    Domain = " + Domain);
            builder.AppendLine("    ServiceId = " + ServiceId);
            builder.AppendLine("    Status = " + Status);
            builder.AppendLine("    IssuerName = " + IssuerName);
            builder.AppendLine("}");
            return builder.ToString();
        }

        enum Datatype : ushort
        {
            Empty = 0,
            UInt = 1,
            ULong = 2,
            String = 4,
            Time = 7,
            Binary = 8,
            Body = 0x3000,
            Footer = 0x3002
        }
    }
}