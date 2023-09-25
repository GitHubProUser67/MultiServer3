using System.Collections;

namespace MultiServer.CryptoSporidium.ChannelID
{
  public class SIDKeyGenerator
  {
        private static SIDKeyGenerator m_Instance;
        private short m_SIDMin = 0;
        private int m_SIDMax = 65535;
        private int[,] m_ScatterTable = new int[16, 2]
        {
          {
            2,
            1
          },
          {
            2,
            9
          },
          {
            1,
            4
          },
          {
            11,
            3
          },
          {
            14,
            12
          },
          {
            10,
            5
          },
          {
            9,
            1
          },
          {
            3,
            7
          },
          {
            2,
            10
          },
          {
            1,
            4
          },
          {
            5,
            9
          },
          {
            5,
            13
          },
          {
            7,
            2
          },
          {
            6,
            7
          },
          {
            14,
            8
          },
          {
            1,
            8
          }
        };

        private int[,] m_NewerScatterTable = new int[16, 2]
        {
          {
            3,
            12
          },
          {
            8,
            6
          },
          {
            2,
            8
          },
          {
            4,
            5
          },
          {
            5,
            1
          },
          {
            4,
            10
          },
          {
            1,
            3
          },
          {
            11,
            5
          },
          {
            3,
            4
          },
          {
            5,
            6
          },
          {
            13,
            10
          },
          {
            7,
            5
          },
          {
            2,
            9
          },
          {
            3,
            9
          },
          {
            10,
            8
          },
          {
            4,
            10
          }
        };

        public static SIDKeyGenerator Instance
        {
          get
          {
            if (m_Instance == null)
              m_Instance = new();
            return m_Instance;
          }
        }

        public short MinSID
        {
          get => m_SIDMin;
          set => m_SIDMin = value;
        }

        public int MaxSID
        {
            get => m_SIDMax;
            set => m_SIDMax = value;
        }

        public SceneKey Generate(ushort SceneID)
        {
          byte[] bytes1 = new SceneKey(new Guid("44E790BB-D88D-4d4f-9145-098931F62F7B")).GetBytes();
          byte[] bytes2 = BitConverter.GetBytes(SceneID);
          byte[] bytes3 = SceneKey.New().GetBytes();
          int index1 = (int) bytes3[0] & 15;
          int index2 = m_ScatterTable[index1, 0];
          int index3 = m_ScatterTable[index1, 1];
          bytes3[index2] = bytes2[0];
          bytes3[index3] = bytes2[1];
          byte[] numArray = new byte[16];
          new BitArray(bytes3).Xor(new BitArray(bytes1)).CopyTo(numArray, 0);
          byte num = CRC.Create(numArray, 0, 15);
          numArray[15] = num;
          return new SceneKey(numArray);
        }

        public SceneKey GenerateNewerType(ushort SceneID)
        {
            byte[] bytes1 = new SceneKey(new byte[] { 0xB9, 0x20, 0x86, 0xBC, 0x3E, 0x8B, 0x4A, 0xDF, 0xA3, 0x01, 0x4D, 0xEE, 0x2F, 0xA3, 0xAB, 0x69 }).GetBytes();
            byte[] bytes2 = BitConverter.GetBytes(SceneID);
            byte[] bytes3 = SceneKey.New().GetBytes();
            int index1 = (int)bytes3[0] & 15;
            int index2 = m_NewerScatterTable[index1, 0];
            int index3 = m_NewerScatterTable[index1, 1];
            bytes3[index2] = bytes2[0];
            bytes3[index3] = bytes2[1];
            byte[] numArray = new byte[16];
            new BitArray(bytes3).Xor(new BitArray(bytes1)).CopyTo(numArray, 0);
            uint num = CRC16.CalcCcittCRC16(numArray, 14);
            numArray[14] = (byte)(num >> 8); // Get the higher 8 bits
            numArray[15] = (byte)num;        // Get the lower 8 bits
            return new SceneKey(numArray);
        }

        public ushort ExtractSceneID(SceneKey Key)
        {
          byte[] bytes1 = Key.GetBytes();
          byte[] bytes2 = new SceneKey(new Guid("44E790BB-D88D-4d4f-9145-098931F62F7B")).GetBytes();
          byte[] numArray = new byte[16];
          new BitArray(bytes1).Xor(new BitArray(bytes2)).CopyTo(numArray, 0);
          int index1 = (int) numArray[0] & 15;
          int index2 = m_ScatterTable[index1, 0];
          int index3 = m_ScatterTable[index1, 1];
          ushort sceneID = (ushort) ((uint) numArray[index2] | (uint) numArray[index3] << 8);
          if ((int) sceneID < (int) m_SIDMin || (int)sceneID > m_SIDMax)
            throw new InvalidSceneIDException(Key, sceneID);
          return sceneID;
        }

        public ushort ExtractSceneIDNewerType(SceneKey Key)
        {
            byte[] bytes1 = Key.GetBytes();
            byte[] bytes2 = new SceneKey(new byte[] { 0xB9, 0x20, 0x86, 0xBC, 0x3E, 0x8B, 0x4A, 0xDF, 0xA3, 0x01, 0x4D, 0xEE, 0x2F, 0xA3, 0xAB, 0x69 }).GetBytes();
            byte[] numArray = new byte[16];
            new BitArray(bytes1).Xor(new BitArray(bytes2)).CopyTo(numArray, 0);
            int index1 = (int)numArray[0] & 15;
            int index2 = m_NewerScatterTable[index1, 0];
            int index3 = m_NewerScatterTable[index1, 1];
            ushort sceneID = (ushort)((uint)numArray[index2] | (uint)numArray[index3] << 8);
            if ((int)sceneID < (int)m_SIDMin || (int)sceneID > m_SIDMax)
                throw new InvalidSceneIDException(Key, sceneID);
            return sceneID;
        }

        public void Verify(SceneKey Key)
        {
          byte[] data = !(Key.ToString() == "00000000-0000-0000-0000-000000000000") ? Key.GetBytes() : throw new InvalidSceneIDKeyException(Key);
          byte num = data[15];
          if ((int) CRC.Create(data, 0, 15) != (int) num)
            throw new InvalidSceneIDKeyException(Key);
        }

        public void VerifyNewerKey(SceneKey Key)
        {
            byte[] data = !(Key.ToString() == "00000000-0000-0000-0000-000000000000") ? Key.GetBytes() : throw new InvalidSceneIDKeyException(Key);
            ushort crc16Value = (ushort)((data[14] << 8) | data[15]);
            if (CRC16.CalcCcittCRC16(data, 14) != crc16Value)
                throw new InvalidSceneIDKeyException(Key);
        }

        /*public byte[] ConvertGuidStringToBinary(string guidString) // Thanks to Home eboot reversing
        {
            string sanitizedInput = guidString.Replace("-", ""); // Remove hyphens

            // Check if the string length is even (each byte is represented by two hex characters)
            if (sanitizedInput.Length % 2 != 0)
            {
                ServerConfiguration.LogError("Invalid input length.");
                return null;
            }

            byte[] byteArray = Enumerable.Range(0, sanitizedInput.Length / 2)
                .Select(x => Convert.ToByte(sanitizedInput.Substring(x * 2, 2), 16))
                .ToArray();

            return byteArray;
        }*/
    }
}
