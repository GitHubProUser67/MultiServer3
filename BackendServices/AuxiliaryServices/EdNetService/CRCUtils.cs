namespace EdNetService
{
    public class CRCUtils
    {
        private static bool IsCRCTableInitiated = false;

        private static readonly uint[] CRCTable = new uint[256];

        private static readonly uint[] CRCTable_New = new uint[]
        {
            0U,
            498536548U,
            997073096U,
            651767980U,
            1994146192U,
            1802195444U,
            1303535960U,
            1342533948U,
            3988292384U,
            4027552580U,
            3604390888U,
            3412177804U,
            2607071920U,
            2262029012U,
            2685067896U,
            3183342108U
        };

        public static string GetCRCFromStringHexadecimal(string s, bool v2)
        {
            return "0x" + GetCRCFromString(s, v2).ToString("X").PadLeft(4, '0');
        }

        public static ushort GetCRCFromString(string s, bool v2)
        {
            return GetCRCFromBuffer(s.ToCharArray(), v2);
        }

        public static ushort GetCRCFromBuffer(char[] b, bool v2)
        {
            uint CRCValue = 0;

            if (b.Length >= 2 && b[0] == '"' && b[^1] == '"')
            {
                char[] result = new char[b.Length - 2];
                Array.Copy(b, 1, result, 0, result.Length);
                CRCValue = GetCRCFromBuffer32(result, v2);
            }
            else
                CRCValue = GetCRCFromBuffer32(b, v2);

            return (ushort)(((CRCValue ^ uint.MaxValue) & 65535U) ^ (((CRCValue ^ uint.MaxValue) & 4294901760U) >> 16));
        }

        private static uint GetCRCFromBuffer32(char[] b, bool v2)
        {
            uint CRCValue = uint.MaxValue;

            if (!v2 && !IsCRCTableInitiated)
                InitializeCRCTable();

            foreach (byte byteValue in b.Select(v => (byte)v))
            {
                if (v2)
                {
                    CRCValue = (CRCValue >> 4 ^ CRCTable_New[(int)((UIntPtr)((CRCValue ^ (uint)byteValue) & 15U))]);
                    CRCValue = (CRCValue >> 4 ^ CRCTable_New[(int)((UIntPtr)((CRCValue ^ (uint)(byte)(byteValue >> 4)) & 15U))]);
                }
                else
                    CRCValue = (CRCValue >> 8) ^ CRCTable[(CRCValue ^ (byte)byteValue) & 0xFF];
            }

            return ~CRCValue;
        }

        private static void InitializeCRCTable()
        {
            uint uVar2 = 0;

            do
            {
                int iVar1 = 8;
                uint uVar3 = uVar2;

                do
                {
                    if ((uVar3 & 1) == 0)
                        uVar3 >>= 1;
                    else
                        uVar3 = uVar3 >> 1 ^ 0xEDB88320;

                    iVar1--;

                } while (iVar1 > 0);

                CRCTable[uVar2] = uVar3;

                uVar2++;

            } while (uVar2 < 256);

            IsCRCTableInitiated = true;
        }
    }
}
