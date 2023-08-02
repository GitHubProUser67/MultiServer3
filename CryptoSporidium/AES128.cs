namespace PSMultiServer.CryptoSporidium
{
    public class AES128
    {
        public const int BLOCK_SIZE = 128 / 8;
        public const int KEY_SIZE = 128 / 8;
        public const int ROUND_NO = 10;

        // forward sbox
        private readonly byte[] sBox = new byte[256] {
        //0     1    2      3     4    5     6     7      8    9     A      B    C     D     E     F
        0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76, //0
        0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0, //1
        0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15, //2
        0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75, //3
        0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84, //4
        0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf, //5
        0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8, //6
        0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2, //7
        0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73, //8
        0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb, //9
        0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79, //A
        0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08, //B
        0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a, //C
        0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e, //D
        0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf, //E
        0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16 }; //F


        private readonly byte[] inverseSBox = new byte[256] {
        //0     1    2      3     4    5     6     7      8    9     A      B    C     D     E     F
        0x52, 0x09, 0x6A, 0xD5, 0x30, 0x36, 0xA5, 0x38, 0xBF, 0x40, 0xA3, 0x9E, 0x81, 0xF3, 0xD7, 0xFB, //0
        0x7C, 0xE3, 0x39, 0x82, 0x9B, 0x2F, 0xFF, 0x87, 0x34, 0x8E, 0x43, 0x44, 0xC4, 0xDE, 0xE9, 0xCB, //1
        0x54, 0x7B, 0x94, 0x32, 0xA6, 0xC2, 0x23, 0x3D, 0xEE, 0x4C, 0x95, 0x0B, 0x42, 0xFA, 0xC3, 0x4E, //2
        0x08, 0x2E, 0xA1, 0x66, 0x28, 0xD9, 0x24, 0xB2, 0x76, 0x5B, 0xA2, 0x49, 0x6D, 0x8B, 0xD1, 0x25, //3
        0x72, 0xF8, 0xF6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xD4, 0xA4, 0x5C, 0xCC, 0x5D, 0x65, 0xB6, 0x92, //4
        0x6C, 0x70, 0x48, 0x50, 0xFD, 0xED, 0xB9, 0xDA, 0x5E, 0x15, 0x46, 0x57, 0xA7, 0x8D, 0x9D, 0x84, //5
        0x90, 0xD8, 0xAB, 0x00, 0x8C, 0xBC, 0xD3, 0x0A, 0xF7, 0xE4, 0x58, 0x05, 0xB8, 0xB3, 0x45, 0x06, //6
        0xD0, 0x2C, 0x1E, 0x8F, 0xCA, 0x3F, 0x0F, 0x02, 0xC1, 0xAF, 0xBD, 0x03, 0x01, 0x13, 0x8A, 0x6B, //7
        0x3A, 0x91, 0x11, 0x41, 0x4F, 0x67, 0xDC, 0xEA, 0x97, 0xF2, 0xCF, 0xCE, 0xF0, 0xB4, 0xE6, 0x73, //8
        0x96, 0xAC, 0x74, 0x22, 0xE7, 0xAD, 0x35, 0x85, 0xE2, 0xF9, 0x37, 0xE8, 0x1C, 0x75, 0xDF, 0x6E, //9
        0x47, 0xF1, 0x1A, 0x71, 0x1D, 0x29, 0xC5, 0x89, 0x6F, 0xB7, 0x62, 0x0E, 0xAA, 0x18, 0xBE, 0x1B, //A
        0xFC, 0x56, 0x3E, 0x4B, 0xC6, 0xD2, 0x79, 0x20, 0x9A, 0xDB, 0xC0, 0xFE, 0x78, 0xCD, 0x5A, 0xF4, //B
        0x1F, 0xDD, 0xA8, 0x33, 0x88, 0x07, 0xC7, 0x31, 0xB1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xEC, 0x5F, //C
        0x60, 0x51, 0x7F, 0xA9, 0x19, 0xB5, 0x4A, 0x0D, 0x2D, 0xE5, 0x7A, 0x9F, 0x93, 0xC9, 0x9C, 0xEF, //D
        0xA0, 0xE0, 0x3B, 0x4D, 0xAE, 0x2A, 0xF5, 0xB0, 0xC8, 0xEB, 0xBB, 0x3C, 0x83, 0x53, 0x99, 0x61, //E
        0x17, 0x2B, 0x04, 0x7E, 0xBA, 0x77, 0xD6, 0x26, 0xE1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0C, 0x7D }; //F

        // round coefficients
        private readonly byte[] roundCoefficient = new byte[ROUND_NO] {
        0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36 }; //Round coefficient is 10, the number of key is 11

        // keys
        private byte[] key = new byte[KEY_SIZE] { 0x09, 0x15, 0x00, 0x97, 0x00, 0x12, 0x10, 0x13, 0x03, 0x02, 0x19, 0x02, 0x05, 0x00, 0x29, 0x80 };
        private byte[][] subkeys = new byte[ROUND_NO + 1][]; //each subkey is 16-bytes length
        public byte[] Key
        {
            get { return key; }
            set
            { //When key is set, subkeys are automatically generated
                if (value == null || value.Length != KEY_SIZE)
                    return;
                key = value;
                Array.Copy(key, subkeys[0], KEY_SIZE);
                for (int i = 0; i < ROUND_NO; ++i)
                {
                    Array.Copy(subkeys[i], subkeys[i + 1], KEY_SIZE);
                    keyTransform(subkeys[i + 1], i);
                }
            }
        }

        public byte[][] Subkeys { get { return subkeys; } }

        public AES128(byte[] key = null)
        {
            for (int i = 0; i < ROUND_NO + 1; ++i)
                subkeys[i] = new byte[KEY_SIZE]; //All the necessary array size for this subkey is made at the beginning
            Key = key != null && key.Length == KEY_SIZE ? key : this.key; //key / default key
        }

        private void keywordTransform(ref byte[] keyword, int roundno)
        { //round no starts from 0, ends at 9
            if (roundno < 0 || roundno > ROUND_NO - 1)
                return;
            byte buf = keyword[0];
            keyword[0] = (byte)(sBox[keyword[1]] ^ roundCoefficient[roundno]);
            keyword[1] = sBox[keyword[2]];
            keyword[2] = sBox[keyword[3]];
            keyword[3] = sBox[buf];
        }

        private void keyTransform(byte[] key, int roundno)
        {
            byte[] keyword = new byte[4];
            Array.Copy(key, 12, keyword, 0, 4);
            keywordTransform(ref keyword, roundno);
            for (int k = 0; k < 4; ++k)
                key[k] ^= keyword[k];
            for (int i = 0; i < KEY_SIZE - 4; ++i)
                key[i + 4] ^= key[i];
        }

        public const byte BYTE_POLY_REDUCTION = 0x1b;
        public const ushort POLY_REDUCTION = 0x11b;
        private void shiftRows(ref byte[] text)
        {
            byte buf;
            //shift row 2 (1 -> 13, 5 -> 1, 9 -> 5, 13 -> 9)
            buf = text[1];
            text[1] = text[5];
            text[5] = text[9];
            text[9] = text[13];
            text[13] = buf;

            //shift row 3 (2 -> 10, 6 -> 14, 10 -> 2, 14 -> 6)
            buf = text[2];
            text[2] = text[10];
            text[10] = buf;
            buf = text[6];
            text[6] = text[14];
            text[14] = buf;

            //shift row 4 (3 -> 7, 7 -> 11, 11 -> 15, 15 -> 3)
            buf = text[15];
            text[15] = text[11];
            text[11] = text[7];
            text[7] = text[3];
            text[3] = buf;
        }

        private void inverseShiftRows(ref byte[] text)
        {
            byte buf;
            //inverse shift row 2 (1 -> 5, 5 -> 9, 9 -> 13, 13 -> 1)
            buf = text[1];
            text[1] = text[13];
            text[13] = text[9];
            text[9] = text[5];
            text[5] = buf;

            //inverse shift row 3 (2 -> 10, 6 -> 14, 10 -> 2, 14 -> 6)
            buf = text[2];
            text[2] = text[10];
            text[10] = buf;
            buf = text[6];
            text[6] = text[14];
            text[14] = buf;

            //inverse shift row 4 (3 -> 15, 7 -> 3, 11 -> 7, 15 -> 11)
            buf = text[15];
            text[15] = text[3];
            text[3] = text[7];
            text[7] = text[11];
            text[11] = buf;
        }

        private byte galoisMult2(byte val, byte polyRed = BYTE_POLY_REDUCTION)
        { //used for GaloisMult with 2
            return val >= 128 ? (byte)(val << 1 ^ polyRed) : (byte)(val << 1);
        }

        private byte[] quickXORTable = new byte[8] { 0x00, 0x1b, 0x36, 0x2d, 0x6c, 0x77, 0x5a, 0x41 };
        private byte galoisDefaultMult(byte val, byte mult)
        {
            int buf = val << 3;
            if (mult != 0x0E)
                buf ^= val;
            if (mult > 0x0C)
                buf ^= val << 2;
            if ((mult & 0x02) > 0)
                buf ^= val << 1;
            byte xorval = quickXORTable[buf >> 8];
            return xorval == 0 ? (byte)buf : (byte)(buf ^ xorval);
        }

        private void mixColumn(ref byte[] text)
        {
            byte[] temp = new byte[4];
            int p;
            for (int i = 0; i < 4; ++i)
            {
                p = i * 4;
                temp[0] = (byte)(galoisMult2(text[p]) ^ (galoisMult2(text[p + 1]) ^ text[p + 1]) ^ text[p + 2] ^ text[p + 3]);
                temp[1] = (byte)(galoisMult2(text[p + 1]) ^ (galoisMult2(text[p + 2]) ^ text[p + 2]) ^ text[p + 3] ^ text[p]);
                temp[2] = (byte)(galoisMult2(text[p + 2]) ^ (galoisMult2(text[p + 3]) ^ text[p + 3]) ^ text[p] ^ text[p + 1]);
                temp[3] = (byte)(galoisMult2(text[p + 3]) ^ (galoisMult2(text[p]) ^ text[p]) ^ text[p + 1] ^ text[p + 2]);
                Array.Copy(temp, 0, text, p, 4);
            }
        }

        private byte[] inverseMixColumnMatrixElementTable = new byte[] { 0x0B, 0x0D, 0x09, 0x0E, 0x0B, 0x0D, 0x09 };
        private void inverseMixColumn(ref byte[] text)
        {
            byte[] temp = new byte[4];
            int p, p2;
            for (int i = 0; i < 4; ++i)
            {
                p = i * 4;
                for (int j = 0; j < 4; ++j)
                {
                    p2 = 3 - j;
                    temp[j] = (byte)(galoisDefaultMult(text[p], inverseMixColumnMatrixElementTable[p2]) ^ galoisDefaultMult(text[p + 1], inverseMixColumnMatrixElementTable[p2 + 1])
                      ^ galoisDefaultMult(text[p + 2], inverseMixColumnMatrixElementTable[p2 + 2]) ^ galoisDefaultMult(text[p + 3], inverseMixColumnMatrixElementTable[p2 + 3]));
                }
                Array.Copy(temp, 0, text, p, 4);
            }
        }

        private void encryptBlock(byte[] plaintext, ref byte[] ciphertext)
        { //since this is private function, no "input protection" is needed.
            plaintext.CopyTo(ciphertext, 0);
            for (int j = 0; j < BLOCK_SIZE; ++j)
                ciphertext[j] ^= Subkeys[0][j];
            for (int r = 0; r < ROUND_NO; ++r)
            {
                for (int i = 0; i < BLOCK_SIZE; ++i)
                    ciphertext[i] = sBox[ciphertext[i]];
                shiftRows(ref ciphertext);
                if (r < ROUND_NO - 1)
                    mixColumn(ref ciphertext);
                for (int j = 0; j < BLOCK_SIZE; ++j)
                    ciphertext[j] ^= Subkeys[r + 1][j];
            }
        }

        private void decryptBlock(byte[] ciphertext, ref byte[] plaintext)
        { //since this is private function, no "input protection" is needed.
            ciphertext.CopyTo(plaintext, 0);
            for (int r = 0; r < ROUND_NO; ++r)
            {
                for (int j = 0; j < BLOCK_SIZE; ++j)
                    plaintext[j] ^= Subkeys[ROUND_NO - r][j];
                if (r > 0)
                    inverseMixColumn(ref plaintext);
                inverseShiftRows(ref plaintext);
                for (int i = 0; i < BLOCK_SIZE; ++i)
                    plaintext[i] = inverseSBox[plaintext[i]];
            }
            for (int j = 0; j < BLOCK_SIZE; ++j)
                plaintext[j] ^= Subkeys[0][j];
        }

        public bool Encrypt(byte[] plaintext, ref byte[] ciphertext)
        {
            try
            {
                if (plaintext == null || ciphertext == null || ciphertext.Length < plaintext.Length) //invalid input(s)
                    return false;
                int extrabytes = plaintext.Length % BLOCK_SIZE;
                int pblock = plaintext.Length / BLOCK_SIZE + (extrabytes > 0 ? 1 : 0);
                int cblock = ciphertext.Length / BLOCK_SIZE;
                if (cblock < pblock) //invalid size
                    return false;
                pblock = plaintext.Length / BLOCK_SIZE;
                byte[] text = new byte[BLOCK_SIZE];
                int p;
                for (int k = 0; k < pblock; ++k)
                { //Encrypt all possible blocks
                    p = k * BLOCK_SIZE;
                    Array.Copy(plaintext, p, text, 0, BLOCK_SIZE);
                    encryptBlock(text, ref text);
                    Array.Copy(text, 0, ciphertext, p, BLOCK_SIZE);
                }
                if (extrabytes > 0)
                { //encrypt the left over
                    p = pblock * BLOCK_SIZE;
                    Array.Copy(plaintext, p, text, 0, extrabytes);
                    for (int i = extrabytes; i < BLOCK_SIZE; ++i) //TODO not sure if there is any faster way in C#
                        text[i] = 0;
                    encryptBlock(text, ref text);
                    Array.Copy(text, 0, ciphertext, p, BLOCK_SIZE);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRYPTOSPORIDIUM : has throw an exception in AES128 Encrypt - {ex}");

                return false;
            }
        }

        public bool Decrypt(byte[] ciphertext, ref byte[] plaintext)
        {
            try
            {
                //can only recover up to valid multiplication of 16, extra bytes are not decrypted
                if (plaintext == null || ciphertext == null) //invalid input(s)
                    return false;
                int cblock = ciphertext.Length / BLOCK_SIZE;
                int pblock = plaintext.Length / BLOCK_SIZE;
                if (pblock < cblock)
                    return false; //invalid size
                byte[] text = new byte[BLOCK_SIZE];
                int p;
                for (int k = 0; k < cblock; ++k)
                {
                    p = k * BLOCK_SIZE;
                    Array.Copy(ciphertext, p, text, 0, BLOCK_SIZE);
                    decryptBlock(text, ref text);
                    Array.Copy(text, 0, plaintext, p, BLOCK_SIZE);
                } //extra bytes are not taken cared of...
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRYPTOSPORIDIUM : has throw an exception in AES128 Decrypt - {ex}");

                return false;
            }
        }
    }
}
