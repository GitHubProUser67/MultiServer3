namespace ComponentAce.Compression.Libs.zlib;

public class ZStreamException : IOException
{
	public ZStreamException()
	{
	}

	public ZStreamException(string s)
		: base(s)
	{
	}
}