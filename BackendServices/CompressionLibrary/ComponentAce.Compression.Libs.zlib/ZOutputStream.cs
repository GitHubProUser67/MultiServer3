using System.IO;
using System;

namespace ComponentAce.Compression.Libs.zlib
{
    public class ZOutputStream : Stream
    {
        protected internal ZStream z = new ZStream();

        protected internal int bufsize = 4096;

        protected internal int flush_Renamed_Field;

        protected internal byte[] buf;

        protected internal byte[] buf1 = new byte[1];

        protected internal bool compress;

        protected bool closed = false;

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

        public virtual long TotalIn => z != null ? z.total_in : 0L;

        public virtual long TotalOut => z != null ? z.total_out : 0L;

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

        private void ImplDisposing(bool disposeOutput)
        {
            if (!closed)
            {
                try
                {
                    try
                    {
                        Finish();
                    }
                    catch (IOException)
                    {
                        // Ignore
                    }
                }
                finally
                {
                    closed = true;
                    End();
                    if (disposeOutput)
                        out_Renamed.Dispose();
                    out_Renamed = null;
                }
            }
        }

        protected void Detach(bool disposing)
        {
            if (disposing)
                ImplDisposing(disposeOutput: false);
            base.Dispose(disposing);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ImplDisposing(disposeOutput: true);
            base.Dispose(disposing);
        }

        public ZOutputStream(Stream out_Renamed, bool NoHeader)
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            if (NoHeader)
                z.inflateInit(-15);
            else
                z.inflateInit();
            compress = false;
        }

        public ZOutputStream(Stream out_Renamed, int level, bool NoHeader)
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            if (NoHeader)
                z.deflateInit(level, -15);
            else
                z.deflateInit(level);
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
                return;
            byte[] array = new byte[b1.Length];
            Array.Copy(b1, 0, array, 0, b1.Length);
            if (z != null)
            {
                z.next_in = array;
                z.next_in_index = off;
                z.avail_in = len;
                int num;
                do
                {
                    z.next_out = buf;
                    z.next_out_index = 0;
                    z.avail_out = bufsize;
                    num = (!compress) ? z.inflate(flush_Renamed_Field) : z.deflate(flush_Renamed_Field);
                    if (num != 0 && num != 1)
                        throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                    if (buf != null)
                        out_Renamed?.Write(buf, 0, bufsize - z.avail_out);
                }
                while ((z.avail_in > 0 || z.avail_out == 0) && num == 0);
            }
        }

        public virtual void Finish()
        {
            int num;
            if (z != null)
            {
                do
                {
                    z.next_out = buf;
                    z.next_out_index = 0;
                    z.avail_out = bufsize;
                    num = (!compress) ? z.inflate(4) : z.deflate(4);
                    if (num != 1 && num != 0)
                        throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                    if (bufsize - z.avail_out > 0 && buf != null)
                        out_Renamed?.Write(buf, 0, bufsize - z.avail_out);
                }
                while ((z.avail_in > 0 || z.avail_out == 0) && num == 0);
            }

            Flush();
        }

        public virtual void End()
        {
            if (z != null)
            {
                if (compress)
                    z.deflateEnd();
                else
                    z.inflateEnd();
                z.free();
            }
            z = null;
        }

        public override void Close()
        {
            if (!closed)
            {
                try
                {
                    try
                    {
                        Finish();
                    }
                    catch (IOException)
                    {
                        // Ignore
                    }
                }
                finally
                {
                    closed = true;
                    End();
                    out_Renamed.Close();
                    out_Renamed = null;
                }
            }
        }

        public override void Flush()
        {
            out_Renamed?.Flush();
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

    public class ZOutputStreamLeaveOpen
        : ZOutputStream
    {
        public ZOutputStreamLeaveOpen(Stream output)
            : base(output, false)
        {
        }

        public ZOutputStreamLeaveOpen(Stream output, bool NoHeader)
            : base(output, NoHeader)
        {
        }

        public ZOutputStreamLeaveOpen(Stream output, int level, bool NoHeader)
            : base(output, level, NoHeader)
        {
        }

        public override void Close()
        {
            Detach(true);
        }

        protected override void Dispose(bool disposing)
        {
            Detach(disposing);
        }
    }
}