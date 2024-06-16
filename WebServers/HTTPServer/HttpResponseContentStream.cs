using System;
using System.IO;
using System.Text;

namespace HTTPServer
{
	/// <summary>
	/// From WebOne.
	/// A wrapper around a <see cref="Stream"/> that can write HTTP response bodies in according to RFC 9112 §7.
	/// </summary>
	public class HttpResponseContentStream : Stream
	{
		//RTFM: https://datatracker.ietf.org/doc/html/rfc9112#section-7

		private readonly Stream inner;
		private readonly bool UseChunkedTransfer;

		/// <summary>
		/// Initialize this HttpResponseContentStream instance.
		/// </summary>
		/// <param name="inner">The stream used to communicate with client.</param>
		/// <param name="chunked">Use HTTP Chunked Transfer</param>
		public HttpResponseContentStream(Stream inner, bool UseChunkedTransfer)
		{
            this.inner = inner;
            this.UseChunkedTransfer = UseChunkedTransfer;
        }

        public override void Flush()
		{ inner.Flush(); }

		public override long Seek(long offset, SeekOrigin origin)
		{ throw new NotImplementedException(); }

		public override void SetLength(long value)
		{ throw new NotImplementedException(); }

		public override int Read(byte[] buffer, int offset, int count)
		{ throw new NotImplementedException(); }

		/// <summary>
		/// Writes a sequence of bytes to the client.
		/// </summary>
		/// <param name="buffer">Array of bytes containing the data payload.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (UseChunkedTransfer)
			{
				byte[] ChunckHeader = Encoding.ASCII.GetBytes((count - offset).ToString("X") + "\r\n");
                byte[] ChunckFooter = Encoding.ASCII.GetBytes("\r\n");

                // Send chunk
                inner.Write(ChunckHeader, 0, ChunckHeader.Length);
				inner.Write(buffer, offset, count);
				inner.Write(ChunckFooter, 0, ChunckFooter.Length);
			}
			else
                // Just write the body
                inner.Write(buffer, offset, count);
        }

		/// <summary>
		/// If the data transfer channel between server and client is based on encoded transfer, send mark of content end,
		/// required to properly finish the transfer session.
		/// </summary>
		/// <param name="trailer">Trailing header (if any)</param>
		public void WriteTerminator(string trailer = "")
		{
			if (UseChunkedTransfer)
			{
				byte[] TrailerHeader = Encoding.ASCII.GetBytes("0\r\n");
                byte[] TrailerFooter = Encoding.ASCII.GetBytes(trailer + "\r\n");

                // Write terminating chunk if need
                try
                {
					inner.Write(TrailerHeader, 0, TrailerHeader.Length);
					inner.Write(TrailerFooter, 0, TrailerFooter.Length);
				}
				catch { /* Sometimes an connection lost may occur here. It's not a reason to worry. */ };
			}
		}

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;

		public override long Length
		{
			get { throw new NotImplementedException(); }
		}

		public override long Position { get; set; }
	}
}