using System.Collections;

namespace MultiServer.CryptoSporidium.ChannelID
{
  public class SIDKeyGenerator
  {
    private static SIDKeyGenerator m_Instance;
    private short m_SIDMin = 1;
    private short m_SIDMax = 9000;
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

    public short MaxSID
    {
      get => m_SIDMax;
      set => m_SIDMax = value;
    }

    public SceneKey Generate(ushort SceneID, bool newerhome)
    {
      byte[] bytes1 = null;
      if (newerhome)
         bytes1 = new SceneKey(new Guid(new byte[] { 0xB9, 0x20, 0x86, 0xBC, 0x3E, 0x8B, 0x4A, 0xDF, 0xA3, 0x01, 0x4D, 0xEE, 0x2F, 0xA3, 0xAB, 0x69 })).GetBytes();
      else
         bytes1 = new SceneKey(new Guid("44E790BB-D88D-4d4f-9145-098931F62F7B")).GetBytes();
      byte[] bytes2 = BitConverter.GetBytes(SceneID);
      byte[] bytes3 = SceneKey.New().GetBytes();
      int index1 = (int) bytes3[0] & 15;
      int index2 = 0;
      int index3 = 0;
      if (newerhome)
      {
         index2 = m_NewerScatterTable[index1, 0];
         index3 = m_NewerScatterTable[index1, 1];
      }
      else
      {
         index2 = m_ScatterTable[index1, 0];
         index3 = m_ScatterTable[index1, 1];
      }
      bytes3[index2] = bytes2[0];
      bytes3[index3] = bytes2[1];
      byte[] numArray = new byte[16];
      new BitArray(bytes3).Xor(new BitArray(bytes1)).CopyTo(numArray, 0);
      byte num = CRC.Create(numArray, 0, 15);
      numArray[15] = num;
      return new SceneKey(numArray);
    }

    public ushort ExtractSceneID(SceneKey Key, bool newerhome)
    {
      byte[] bytes1 = Key.GetBytes();
      byte[] bytes2 = null;
      if (newerhome)
         bytes2 = new SceneKey(new Guid(new byte[] { 0xB9, 0x20, 0x86, 0xBC, 0x3E, 0x8B, 0x4A, 0xDF, 0xA3, 0x01, 0x4D, 0xEE, 0x2F, 0xA3, 0xAB, 0x69 })).GetBytes();
      else
         bytes2 = new SceneKey(new Guid("44E790BB-D88D-4d4f-9145-098931F62F7B")).GetBytes();
      byte[] numArray = new byte[16];
      new BitArray(bytes1).Xor(new BitArray(bytes2)).CopyTo(numArray, 0);
      int index1 = (int) numArray[0] & 15;
      int index2 = 0;
      int index3 = 0;
      if (newerhome)
      {
         index2 = m_NewerScatterTable[index1, 0];
         index3 = m_NewerScatterTable[index1, 1];
      }
      else
      {
         index2 = m_ScatterTable[index1, 0];
         index3 = m_ScatterTable[index1, 1];
      }
      ushort sceneID = (ushort) ((uint) numArray[index2] | (uint) numArray[index3] << 8);
      if ((int) sceneID < (int) m_SIDMin || (int) sceneID > (int) m_SIDMax)
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
  }
}
