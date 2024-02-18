namespace QuazalServer.QNetZ.DDL
{
	public interface IAnyData
	{
		void Read(Stream s);
		void Write(Stream s);
	}

	public class qBuffer : IAnyData
	{
		public qBuffer()
		{
			data = Array.Empty<byte>();
		}

		public byte[] data;
		public void Read(Stream s)
		{
			ushort thisSize = Helper.ReadU16(s);
			data = new byte[thisSize];
			s.Read(data, 0, thisSize);
		}

		public void Write(Stream s)
		{
			Helper.WriteU16(s, (ushort)data.Length);
			s.Write(data, 0, data.Length);
		}
	}

	public class AnyData<T> : IAnyData where T: class
	{
		public AnyData()
		{
			className = typeof(T).Name; // that's for writing
		}

		public AnyData(T _data) : this()
		{
			data = _data;
		}

		public string className;
		public T? data;

		public void Read(Stream s)
		{
			className = Helper.ReadString(s);
			uint thisSize = Helper.ReadU32(s);

			// not this data - skip
			if (className != typeof(T).Name)
			{
				s.Seek(thisSize, SeekOrigin.Current);
				return;
			}

			thisSize = Helper.ReadU32(s);
			long curPos = s.Position;
			data = DDLSerializer.ReadObject<T>(s);

			if ((s.Position - curPos) != thisSize)
			{
				CustomLogger.LoggerAccessor.LogError($"AnyData<{typeof(T).Name}> reading error - data size mismatch");
				return;
			}
		}

		public void Write(Stream s)
		{
			Helper.WriteString(s, className);

            MemoryStream m = new();
			DDLSerializer.WriteObject(data, m);

			uint size = (uint)m.Position;

			// write size into memory buffer and data
			Helper.WriteU32(s, size + sizeof(int));
			Helper.WriteU32(s, size);
			s.Write(m.GetBuffer(), 0, (int)size);
		}
	}
}
