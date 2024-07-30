using BitConverterExtension;
using System;
using System.Text;

namespace EdNetService
{
    public class EdStore
    {
        private byte[] _data;
        private int _position;
        private int _bufferSize;

        public EdStore()
        {
            _data = null;
            _position = 0;
            _bufferSize = 0;
        }

        public EdStore(byte[] initialData, int actualSize)
        {
            LoadData(initialData, actualSize);
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public int CurrentSize
        {
            get { return _position; }
        }

        public int MaxSize
        {
            get { return _bufferSize; }
        }

        public int FreeSize
        {
            get { return _bufferSize - _position; }
        }

        public void LoadData(byte[] data, int realSize)
        {
            _data = (data == null) ? new byte[realSize] : data;
            _position = 0;
            _bufferSize = realSize;
        }

        public void MoveTo(int offset)
        {
            if (_data != null && offset >= 0 && offset < _data.Length)
                _position = offset;
        }

        public void MoveToStart()
        {
            MoveTo(0);
        }

        public void MoveToEnd()
        {
            MoveTo(_bufferSize);
        }

        public bool IsReadable(int length)
        {
            return _position + length <= _data.Length;
        }

        public ushort ExtractStart()
        {
            MoveToStart();
            return ExtractUInt16();
        }

        public byte ExtractUInt8()
        {
            byte result = 0;

            if (IsReadable(1))
                result = _data[_position++];

            return result;
        }

        public short ExtractInt16()
        {
            short result = 0;

            if (IsReadable(2))
            {
                result = EndianBitConverter.Big.ToInt16(_data, _position);
                _position += 2;
            }

            return result;
        }

        public ushort ExtractUInt16()
        {
            ushort result = 0;

            if (IsReadable(2))
            {
                result = EndianBitConverter.Big.ToUInt16(_data, _position);
                _position += 2;
            }

            return result;
        }

        public int ExtractInt32()
        {
            int result = 0;

            if (IsReadable(4))
            {
                result = EndianBitConverter.Big.ToInt32(_data, _position);
                _position += 4;
            }

            return result;
        }

        public uint ExtractUInt32()
        {
            uint result = 0U;

            if (IsReadable(4))
            {
                result = EndianBitConverter.Big.ToUInt32(_data, _position);
                _position += 4;
            }

            return result;
        }

        public float ExtractFloat32()
        {
            float result = 0f;

            if (IsReadable(4))
            {
                result = EndianBitConverter.Big.ToSingle(_data, _position);
                _position += 4;
            }

            return result;
        }

        public long ExtractInt64()
        {
            long result = 0L;

            if (IsReadable(8))
            {
                result = EndianBitConverter.Big.ToInt64(_data, _position);
                _position += 8;
            }

            return result;
        }

        public ulong ExtractUInt64()
        {
            ulong result = 0UL;

            if (IsReadable(8))
            {
                result = EndianBitConverter.Big.ToUInt64(_data, _position);
                _position += 8;
            }

            return result;
        }

        public double ExtractDouble64()
        {
            double result = 0.0;

            if (IsReadable(8))
            {
                result = EndianBitConverter.Big.ToDouble(_data, _position);
                _position += 8;
            }

            return result;
        }

        public string ExtractString()
        {
            ushort length = ExtractUInt16();
            string result = null;

            if (length != 0 && IsReadable((int)length))
            {
                result = Encoding.ASCII.GetString(_data, _position, (int)length);
                _position += (int)length;
            }

            return result;
        }

        public byte[] ExtractByteArray()
        {
            ushort length = ExtractUInt16();
            byte[] result = null;

            if (length != 0 && IsReadable((int)length))
            {
                result = new byte[(int)length];
                Buffer.BlockCopy(_data, _position, result, 0, (int)length);
                _position += (int)length;
            }

            return result;
        }

        public byte[] ExtractRawBytes(ushort length)
        {
            byte[] result = null;

            if (IsReadable((int)length))
            {
                result = new byte[(int)length];
                Buffer.BlockCopy(_data, _position, result, 0, (int)length);
                _position += (int)length;
            }

            return result;
        }

        public EdStore ExtractDataStore()
        {
            byte[] data = ExtractByteArray();
            return (data == null) ? null : new EdStore(data, data.Length);
        }

        public bool InsertStart(ushort crc)
        {
            MoveToStart();
            InsertUInt16(crc);
            return true;
        }

        public bool InsertEnd()
        {
            return true;
        }

        public bool InsertUInt8(byte value)
        {
            _data[_position++] = value;
            return true;
        }

        public bool InsertInt16(short value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 2;
            return true;
        }

        public bool InsertUInt16(ushort value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 2;
            return true;
        }

        public bool InsertInt32(int value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 4;
            return true;
        }

        public bool InsertUInt32(uint value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 4;
            return true;
        }

        public bool InsertFloat32(float value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 4;
            return true;
        }

        public bool InsertInt64(long value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 8;
            return true;
        }

        public bool InsertUInt64(ulong value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 8;
            return true;
        }

        public bool InsertDouble64(double value)
        {
            EndianBitConverter.Big.GetBytes(value).CopyTo(_data, _position);
            _position += 8;
            return true;
        }

        public bool InsertString(string value)
        {
            if (value == null || value.Length == 0)
                InsertUInt16(0);
            else
            {
                InsertUInt16((ushort)value.Length);
                Encoding.ASCII.GetBytes(value).CopyTo(_data, _position);
                _position += value.Length;
            }
            return true;
        }

        public bool InsertByteArray(byte[] data)
        {
            return InsertByteArray(data, (ushort)(data?.Length ?? 0));
        }

        public bool InsertByteArray(byte[] data, ushort length)
        {
            if (data == null || length == 0)
                InsertUInt16(0);
            else
            {
                InsertUInt16(length);
                Buffer.BlockCopy(data, 0, _data, _position, (int)length);
                _position += (int)length;
            }
            return true;
        }

        public bool InsertRawBytes(byte[] data, ushort length)
        {
            if (data != null && length > 0)
            {
                Buffer.BlockCopy(data, 0, _data, _position, (int)length);
                _position += (int)length;
            }
            return true;
        }

        public bool InsertDataStore(EdStore store)
        {
            return InsertByteArray(store.Data, (ushort)store.CurrentSize);
        }
    }
}