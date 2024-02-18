using System.Collections;

namespace BackendProject.HomeTools.ChannelID
{
    public class SIDKeyGenerator
    {
        private static SIDKeyGenerator? m_Instance;
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
            int index1 = bytes3[0] & 15;
            bytes3[m_ScatterTable[index1, 0]] = bytes2[0];
            bytes3[m_ScatterTable[index1, 1]] = bytes2[1];
            byte[] numArray = new byte[16];
            new BitArray(bytes3).Xor(new BitArray(bytes1)).CopyTo(numArray, 0);
            numArray[15] = CRC.Create(numArray, 0, 15);
            return new SceneKey(numArray);
        }

        public SceneKey GenerateNewerType(ushort SceneID)
        {
            byte[] bytes1 = new SceneKey(new byte[] { 0xB9, 0x20, 0x86, 0xBC, 0x3E, 0x8B, 0x4A, 0xDF, 0xA3, 0x01, 0x4D, 0xEE, 0x2F, 0xA3, 0xAB, 0x69 }).GetBytes();
            byte[] bytes2 = BitConverter.GetBytes(SceneID);
            byte[] bytes3 = SceneKey.New().GetBytes();
            int index1 = bytes3[0] & 15;
            bytes3[m_NewerScatterTable[index1, 0]] = bytes2[0];
            bytes3[m_NewerScatterTable[index1, 1]] = bytes2[1];
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
            int index1 = numArray[0] & 15;
            ushort sceneID = (ushort)(numArray[m_ScatterTable[index1, 0]] | (uint)numArray[m_ScatterTable[index1, 1]] << 8);
            if (sceneID < m_SIDMin || sceneID > m_SIDMax)
                throw new InvalidSceneIDException(Key, sceneID);
            return sceneID;
        }

        public ushort ExtractSceneIDNewerType(SceneKey Key)
        {
            byte[] bytes1 = Key.GetBytes();
            byte[] bytes2 = new SceneKey(new byte[] { 0xB9, 0x20, 0x86, 0xBC, 0x3E, 0x8B, 0x4A, 0xDF, 0xA3, 0x01, 0x4D, 0xEE, 0x2F, 0xA3, 0xAB, 0x69 }).GetBytes();
            byte[] numArray = new byte[16];
            new BitArray(bytes1).Xor(new BitArray(bytes2)).CopyTo(numArray, 0);
            int index1 = numArray[0] & 15;
            ushort sceneID = (ushort)(numArray[m_NewerScatterTable[index1, 0]] | (uint)numArray[m_NewerScatterTable[index1, 1]] << 8);
            if (sceneID < m_SIDMin || sceneID > m_SIDMax)
                throw new InvalidSceneIDException(Key, sceneID);
            return sceneID;
        }

        public void Verify(SceneKey Key)
        {
            byte[] data = !(Key.ToString() == "00000000-0000-0000-0000-000000000000") ? Key.GetBytes() : throw new InvalidSceneIDKeyException(Key);
            if (CRC.Create(data, 0, 15) != data[15])
                throw new InvalidSceneIDKeyException(Key);
        }

        public void VerifyNewerKey(SceneKey Key)
        {
            byte[] data = !(Key.ToString() == "00000000-0000-0000-0000-000000000000") ? Key.GetBytes() : throw new InvalidSceneIDKeyException(Key);
            if (CRC16.CalcCcittCRC16(data, 14) != (ushort)(data[14] << 8 | data[15]))
                throw new InvalidSceneIDKeyException(Key);
        }
    }
}
