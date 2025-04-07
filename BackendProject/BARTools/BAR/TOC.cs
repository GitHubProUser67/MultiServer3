using System.Collections;

namespace BackendProject.BARTools.BAR
{
    public class TOC
    {
        public TOC(BARArchive parentArchive)
        {
            m_entries = new SortedDictionary<int, TOCEntry>();
            m_rawUnsortedEntries = new Dictionary<int, TOCEntry>();
            m_compressedSize = 0U;
            m_parent = parentArchive;
        }

        public TOCEntry[] RawUnsortedEntries
        {
            get
            {
                TOCEntry[] array = new TOCEntry[m_rawUnsortedEntries.Values.Count];
                m_rawUnsortedEntries.Values.CopyTo(array, 0);
                return array;
            }
        }

        public void Add(TOCEntry newEntry)
        {
            if (!m_entries.ContainsKey((int)newEntry.FileName))
            {
                m_entries.Add((int)newEntry.FileName, newEntry);
                m_rawUnsortedEntries.Add((int)newEntry.FileName, newEntry);
            }
        }

        public void Remove(HashedFileName FileName)
        {
            if (m_entries.ContainsKey((int)FileName))
            {
                m_entries.Remove((int)FileName);
                m_rawUnsortedEntries.Remove((int)FileName);
            }
        }

        public void Clear()
        {
            m_entries.Clear();
            m_rawUnsortedEntries.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            foreach (TOCEntry entry in m_entries.Values)
            {
                yield return entry;
            }
            yield break;
        }

        public TOCEntry? this[int index]
        {
            get
            {
                return GetEntry(index);
            }
        }

        public TOCEntry? this[HashedFileName filename]
        {
            get
            {
                TOCEntry? result = null;
                if (m_entries.ContainsKey((int)filename))
                    result = m_entries[(int)filename];
                return result;
            }
        }

        public TOCEntry? this[string path]
        {
            get
            {
                AfsHash afsHash = new(path);
                HashedFileName filename = new(afsHash.Value);
                return this[filename];
            }
        }

        private TOCEntry? GetEntry(int index)
        {
            if (index >= m_entries.Count)
                return null;
            TOCEntry[] array = new TOCEntry[m_entries.Count];
            m_entries.Values.CopyTo(array, 0);
            return array[index];
        }

        public TOCEntry? GetLastEntry()
        {
            int num = -1;
            TOCEntry? result = null;
            foreach (TOCEntry tocentry in m_entries.Values)
            {
                if (tocentry.DataOffset > (uint)num)
                {
                    num = (int)tocentry.DataOffset;
                    result = tocentry;
                }
            }
            return result;
        }

        public uint Count
        {
            get
            {
                return (uint)m_entries.Count;
            }
        }

        public void RecalculateDataSectionOffsets()
        {
            uint num = 0U;
            foreach (TOCEntry tocentry in m_entries.Values)
            {
                if (num == 0U)
                    tocentry.DataOffset = 0U;
                else
                    tocentry.DataOffset = num;
                num += tocentry.AlignedSize;
            }
        }

        public TOCEntry[] SortByDataSectionOffset()
        {
            TOCEntry[] array = new TOCEntry[m_entries.Count];
            m_entries.Values.CopyTo(array, 0);
            for (int i = 1; i < array.Length; i++)
            {
                TOCEntry tocentry = array[i];
                int num = i - 1;
                while (num >= 0 && array[num].DataOffset > tocentry.DataOffset)
                {
                    array[num + 1] = array[num];
                    num--;
                }
                array[num + 1] = tocentry;
            }
            return array;
        }

        public byte[] GetBytesVersion1()
        {
            MemoryStream memoryStream = new();
            BinaryWriter binaryWriter = new(memoryStream);
            foreach (TOCEntry tocentry in m_entries.Values)
            {
                binaryWriter.Write((int)tocentry.FileName);
                uint num = tocentry.DataOffset;
                num |= (uint)tocentry.Compression;
                binaryWriter.Write(num);
                binaryWriter.Write(tocentry.Size);
                binaryWriter.Write(tocentry.CompressedSize);
            }
            binaryWriter.Close();
            return memoryStream.ToArray();
        }

        public byte[] GetBytesVersion2(string key, byte[] IV, EndianType endian)
        {
            MemoryStream memoryStream = new();
            BinaryWriter binaryWriter = new(memoryStream);
            foreach (TOCEntry tocentry in m_entries.Values.Reverse()) // IMPORTANT, HOME EBOOT WANT THE TOC BACKWARD (spent a considerable amount of reverse...)
            {
                if (endian == EndianType.BigEndian)
                {
                    binaryWriter.Write(Utils.EndianSwap((int)tocentry.FileName));
                    uint num = tocentry.DataOffset;
                    num |= (uint)tocentry.Compression;
                    binaryWriter.Write(Utils.EndianSwap(num));
                    binaryWriter.Write(Utils.EndianSwap(tocentry.Size));
                    binaryWriter.Write(Utils.EndianSwap(tocentry.CompressedSize));
                }
                else
                {
                    binaryWriter.Write((int)tocentry.FileName);
                    uint num = tocentry.DataOffset;
                    num |= (uint)tocentry.Compression;
                    binaryWriter.Write(num);
                    binaryWriter.Write(tocentry.Size);
                    binaryWriter.Write(tocentry.CompressedSize);
                }
                
                binaryWriter.Write(tocentry.IV);
            }
            binaryWriter.Close();
            return new UnBAR.AESCTR256EncryptDecrypt().InitiateCTRBuffer(memoryStream.ToArray(), Convert.FromBase64String(key), IV);
        }

        public uint Version1Size
        {
            get
            {
                return (uint)(m_entries.Values.Count * 16L);
            }
        }

        public uint Version2Size
        {
            get
            {
                return (uint)(m_entries.Values.Count * 24L);
            }
        }

        public uint CompressedSize
        {
            get
            {
                return m_compressedSize;
            }
            set
            {
                m_compressedSize = value;
            }
        }

        internal void ResortOffsets()
        {
            TOCEntry[] collection = SortByDataSectionOffset();
            LinkedList<TOCEntry> linkedList = new(collection);
            LinkedListNode<TOCEntry>? linkedListNode = linkedList.First;
            if (linkedListNode != null)
            {
                LinkedListNode<TOCEntry>? next = linkedListNode.Next;
                if (linkedListNode.Value.DataOffset > 0U)
                    linkedListNode.Value.DataOffset = 0U;
                while (next != null)
                {
                    uint dataOffset = linkedListNode.Value.DataOffset;
                    uint num = dataOffset + linkedListNode.Value.AlignedSize;
                    if (num != next.Value.DataOffset)
                        next.Value.DataOffset = num;
                    linkedListNode = next;
                    next = linkedListNode.Next;
                }
            }
        }

        private SortedDictionary<int, TOCEntry> m_entries;

        private Dictionary<int, TOCEntry> m_rawUnsortedEntries;

        private uint m_compressedSize;

        private BARArchive m_parent;
    }
}