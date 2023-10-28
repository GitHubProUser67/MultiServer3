namespace ComponentAce.Compression.Libs.zlib;

public class ZOutputStream : Stream
{
	protected internal ZStream z = new ZStream();

	protected internal int bufsize = 4096;

	protected internal int flush_Renamed_Field;

	protected internal byte[] buf;

	protected internal byte[] buf1 = new byte[1];

	protected internal bool compress;

	private Stream out_Renamed;

	public virtual int FlushMode
	{
		get
		{
			return flush_Renamed_Field;
		}
		set
		{
			flush_Renamed_Field = value;
		}
	}

	public virtual long TotalIn => z.total_in;

	public virtual long TotalOut => z.total_out;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => 0L;

	public override long Position
	{
		get
		{
			return 0L;
		}
		set
		{
		}
	}

	private void InitBlock()
	{
		flush_Renamed_Field = 0;
		buf = new byte[bufsize];
	}

	public ZOutputStream(Stream out_Renamed, bool NoHeader)
	{
		InitBlock();
		this.out_Renamed = out_Renamed;
		if (NoHeader)
		{
			z.inflateInit(-15);
		}
		else
		{
			z.inflateInit();
		}
		compress = false;
	}

	public ZOutputStream(Stream out_Renamed, int level, bool NoHeader)
	{
		InitBlock();
		this.out_Renamed = out_Renamed;
		if (NoHeader)
		{
			z.deflateInit(level, -15);
		}
		else
		{
			z.deflateInit(level);
		}
		compress = true;
	}

	public void WriteByte(int b)
	{
		buf1[0] = (byte)b;
		Write(buf1, 0, 1);
	}

	public override void WriteByte(byte b)
	{
		WriteByte(b);
	}

	public override void Write(byte[] b1, int off, int len)
	{
		if (len == 0)
		{
			return;
		}
		byte[] array = new byte[b1.Length];
		Array.Copy(b1, 0, array, 0, b1.Length);
		z.next_in = array;
		z.next_in_index = off;
		z.avail_in = len;
		int num;
		do
		{
			z.next_out = buf;
			z.next_out_index = 0;
			z.avail_out = bufsize;
			num = ((!compress) ? z.inflate(flush_Renamed_Field) : z.deflate(flush_Renamed_Field));
			if (num != 0 && num != 1)
			{
				throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
			}
			out_Renamed.Write(buf, 0, bufsize - z.avail_out);
		}
		while ((z.avail_in > 0 || z.avail_out == 0) && num == 0);
	}

	public virtual void finish()
	{
		int num;
		do
		{
			z.next_out = buf;
			z.next_out_index = 0;
			z.avail_out = bufsize;
			num = ((!compress) ? z.inflate(4) : z.deflate(4));
			if (num != 1 && num != 0)
			{
				throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
			}
			if (bufsize - z.avail_out > 0)
			{
				out_Renamed.Write(buf, 0, bufsize - z.avail_out);
			}
		}
		while ((z.avail_in > 0 || z.avail_out == 0) && num == 0);
		try
		{
			Flush();
		}
		catch
		{
		}
	}

	public virtual void end()
	{
		if (compress)
		{
			z.deflateEnd();
		}
		else
		{
			z.inflateEnd();
		}
		z.free();
		z = null;
	}

	public override void Close()
	{
		try
		{
			finish();
		}
		catch
		{
		}
		finally
		{
			end();
			out_Renamed.Close();
			out_Renamed = null;
		}
	}

	public override void Flush()
	{
		out_Renamed.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return 0;
	}

	public override void SetLength(long value)
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return 0L;
	}
}