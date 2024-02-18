using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    public class NetAddress : IStreamSerializer
    {
        public static readonly NetAddress Empty = new NetAddress() { AddressType = NetAddressType.NetAddressNone };

        public NetAddressType AddressType;
        public string? Address;
        public uint BinaryAddress;
        public int Port;

        public byte IPBinaryBitOne;
        public byte IPBinaryBitTwo;
        public byte IPBinaryBitThree;
        public byte IPBinaryBitFour;
        public ushort BinaryPort;

        public void Deserialize(BinaryReader reader)
        {
            AddressType = reader.Read<NetAddressType>();

            if(AddressType == NetAddressType.NetAddressTypeBinaryExternal 
                || AddressType == NetAddressType.NetAddressTypeBinaryInternal)
            {
                BinaryAddress = reader.ReadUInt32();
                Port = reader.ReadInt32();
            }

            if(AddressType == NetAddressType.NetAddressTypeBinaryExternalVport
                || AddressType == NetAddressType.NetAddressTypeBinaryInternalVport)
            {
                IPBinaryBitOne = reader.ReadByte();
                IPBinaryBitTwo = reader.ReadByte();
                IPBinaryBitThree = reader.ReadByte();
                IPBinaryBitFour = reader.ReadByte();
                reader.ReadBytes(12);
                BinaryPort = reader.ReadUInt16();
                reader.ReadBytes(2);
            }

            if(AddressType == NetAddressType.NetAddressTypeInternal
                || AddressType == NetAddressType.NetAddressTypeExternal
                || AddressType == NetAddressType.NetAddressTypeNATService
                || AddressType == NetAddressType.NetAddressTypeSignalAddress
                || AddressType == NetAddressType.NetAddressNone)
            {
                Address = reader.ReadString(Constants.NET_MAX_NETADDRESS_LENGTH);
                Port = reader.ReadInt32();
            }
            
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(AddressType);
            if (AddressType == NetAddressType.NetAddressTypeBinaryExternal
                || AddressType == NetAddressType.NetAddressTypeBinaryInternal)  
            {
                writer.Write(BinaryAddress);
                writer.Write(Port);
            }

            if(AddressType == NetAddressType.NetAddressTypeBinaryExternalVport
                || AddressType == NetAddressType.NetAddressTypeBinaryInternalVport)
            {
                writer.Write(IPBinaryBitOne);
                writer.Write(IPBinaryBitTwo);
                writer.Write(IPBinaryBitThree);
                writer.Write(IPBinaryBitFour);

                writer.Write(new byte[12]);
                writer.Write(BinaryPort);
                writer.Write(new byte[2]);
            }

            if(AddressType == NetAddressType.NetAddressTypeInternal 
                || AddressType == NetAddressType.NetAddressTypeExternal 
                || AddressType == NetAddressType.NetAddressTypeNATService 
                || AddressType == NetAddressType.NetAddressTypeSignalAddress
                || AddressType == NetAddressType.NetAddressNone)
            {
                if (Address != null)
                    writer.Write(Address, Constants.NET_MAX_NETADDRESS_LENGTH);
                else
                    writer.Write("127.0.0.1", Constants.NET_MAX_NETADDRESS_LENGTH);
                writer.Write(Port);
            }
        }

        public override string ToString()
        {
            if (AddressType == NetAddressType.NetAddressTypeBinaryExternal
                || AddressType == NetAddressType.NetAddressTypeBinaryInternal) {
                return base.ToString() + " " +
                $"AddressType: {AddressType} " +
                $"BinaryAddress: {BinaryAddress} " +
                $"Port: {Port}";
            } else if (AddressType == NetAddressType.NetAddressTypeBinaryExternalVport
                || AddressType == NetAddressType.NetAddressTypeBinaryInternalVport) {
                return base.ToString() + " " +
                $"AddressType: {AddressType} " +
                $"(Binary) IP : {IPBinaryBitOne}.{IPBinaryBitTwo}.{IPBinaryBitThree}.{IPBinaryBitFour} " +
                $"(Vport/Port) Port: {BinaryPort}"; 
            } else {
                return base.ToString() + " " +
                $"AddressType: {AddressType} " +
                $"Address: {string.Join(" ", Address)} " +
                $"Port: {Port}";
            }

        }
    }
}