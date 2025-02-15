// Copyright (c) 2006, ComponentAce
// http://www.componentace.com
// All rights reserved.

// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
// Neither the name of ComponentAce nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

/*
Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright 
notice, this list of conditions and the following disclaimer in 
the documentation and/or other materials provided with the distribution.

3. The names of the authors may not be used to endorse or promote products
derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
/*
* This program is based on zlib-1.1.3, so all credit should go authors
* Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
* and contributors of zlib.
*/
using System.IO;

namespace ComponentAce.Compression.Libs.zlib
{
    public class ZInputStream : BinaryReader
    {
        protected ZStream z = new ZStream();

        protected int bufsize = 512;

        protected int flush;

        protected byte[] buf;

        protected byte[] buf1 = new byte[1];

        protected bool compress;

        internal Stream in_Renamed;

        internal bool nomoreinput;

        public virtual int FlushMode
        {
            get
            {
                return flush;
            }
            set
            {
                flush = value;
            }
        }

        public virtual long TotalIn => z.total_in;

        public virtual long TotalOut => z.total_out;

        internal void InitBlock()
        {
            flush = 0;
            buf = new byte[bufsize];
        }

        public ZInputStream(Stream in_Renamed)
            : base(in_Renamed)
        {
            InitBlock();
            this.in_Renamed = in_Renamed;
            z.inflateInit();
            compress = false;
            z.next_in = buf;
            z.next_in_index = 0;
            z.avail_in = 0;
        }

        public ZInputStream(Stream in_Renamed, int level)
            : base(in_Renamed)
        {
            InitBlock();
            this.in_Renamed = in_Renamed;
            z.deflateInit(level);
            compress = true;
            z.next_in = buf;
            z.next_in_index = 0;
            z.avail_in = 0;
        }

        public override int Read()
        {
            if (read(buf1, 0, 1) == -1)
                return -1;
            return buf1[0] & 0xFF;
        }

        public int read(byte[] b, int off, int len)
        {
            if (len == 0)
                return 0;
            z.next_out = b;
            z.next_out_index = off;
            z.avail_out = len;
            int num;
            do
            {
                if (z.avail_in == 0 && !nomoreinput && buf != null)
                {
                    z.next_in_index = 0;
                    z.avail_in = SupportClass.ReadInput(in_Renamed, buf, 0, bufsize);
                    if (z.avail_in == -1)
                    {
                        z.avail_in = 0;
                        nomoreinput = true;
                    }
                }
                num = ((!compress) ? z.inflate(flush) : z.deflate(flush));
                if (nomoreinput && num == -5)
                    return -1;
                if (num != 0 && num != 1)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                if (nomoreinput && z.avail_out == len)
                    return -1;
            }
            while (z.avail_out == len && num == 0);
            return len - z.avail_out;
        }

        public long skip(long n)
        {
            int num = 512;
            if (n < num)
                num = (int)n;
            byte[] array = new byte[num];
            return SupportClass.ReadInput(BaseStream, array, 0, array.Length);
        }

        public override void Close()
        {
            in_Renamed.Close();
        }
    }
}