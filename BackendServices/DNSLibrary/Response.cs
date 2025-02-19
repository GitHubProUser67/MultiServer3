﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DNS.Protocol.ResourceRecords;
using DNS.Protocol.Utils;

namespace DNS.Protocol {
    public class Response : IResponse {
        private Header header;
        private IList<Question> questions;
        private IList<IResourceRecord> answers;
        private IList<IResourceRecord> authority;
        private IList<IResourceRecord> additional;

        public static byte[] MakeType0DnsResponsePacket(byte[] Req, List<IPAddress> Ips, int maxResponseSize = 512, int timeToLive = 180)
        {
            const byte dnsHeaderSize = 12;

            if (Req.Length < dnsHeaderSize || Ips == null)
                return null;

            bool TruncateFlag = false;

            List<byte> ans = new List<byte>();

            // https://web.archive.org/web/20150326065952/http://www.ccs.neu.edu/home/amislove/teaching/cs4700/fall09/handouts/project1-primer.pdf
            // Header
            ans.AddRange(new byte[2] { Req[0], Req[1] }); // ID

            if (Ips.Count == 0)
            {
                ans.AddRange(new byte[2] { 0x81, 0x83 });
                Ips.Add(IPAddress.None); // NXDOMAIN
            }
            else
                ans.AddRange(new byte[2] { 0x81, 0x80 }); // OPCODE & RCODE etc...

            ans.AddRange(new byte[2] { Req[4], Req[5] }); // QDCOUNT (copy from request)

            ans.AddRange(BitConverter.GetBytes(!BitConverter.IsLittleEndian ? EndianTools.EndianUtils.ReverseUshort((ushort)IPAddress.HostToNetworkOrder((short)Ips.Count)) : (ushort)IPAddress.HostToNetworkOrder((short)Ips.Count))); // ANCOUNT (number of answers)

            ans.AddRange(new byte[4]); // NSCOUNT & ARCOUNT (not used)

            for (int i = 12; i < Req.Length; i++)
                ans.Add(Req[i]);

            int responseSize = ans.Count;

            foreach (IPAddress ip in Ips)
            {
                byte[] addrBytes = ip.GetAddressBytes();

                ushort addrSizeOf = (ushort)addrBytes.Length;
                int payloadSize = dnsHeaderSize + addrSizeOf;

                if (payloadSize + responseSize > maxResponseSize)
                {
                    if (maxResponseSize == 512)
                        TruncateFlag = true;
                    break;
                }

                ans.AddRange(new byte[2] { 0xC0, 0x0C }); // Pointer to domain name in query

                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    ans.AddRange(new byte[4] { 0x00, 0x1C, 0x00, 0x01 }); // Type AAAA (IPv6), Class IN
                else
                    ans.AddRange(new byte[4] { 0x00, 0x01, 0x00, 0x01 }); // Type A (IPv4), Class IN

                ans.AddRange(BitConverter.GetBytes(BitConverter.IsLittleEndian ? EndianTools.EndianUtils.ReverseInt(timeToLive) : timeToLive)); // TTL
                ans.AddRange(BitConverter.GetBytes(BitConverter.IsLittleEndian ? EndianTools.EndianUtils.ReverseUshort(addrSizeOf) : addrSizeOf)); // Data length (4 bytes for IPv4, 16 bytes for IPv6)
                ans.AddRange(addrBytes);

                responseSize += payloadSize;
            }

            if (TruncateFlag)
            {
                // Set the truncated (TC) flag in the header (6th bit of second byte)
                ans[3] = (byte)(ans[3] | 0x02);
            }

            byte[] response = ans.ToArray();

            if (responseSize < maxResponseSize)
                Array.Resize(ref response, response.Length + (maxResponseSize - responseSize));

            return response;
        }

        public static Response FromRequest(IRequest request) {
            Response response = new Response();

            response.Id = request.Id;

            foreach (Question question in request.Questions) {
                response.Questions.Add(question);
            }

            return response;
        }

        public static Response FromArray(byte[] message) {
            Header header = Header.FromArray(message);
            int offset = header.Size;

            if (!header.Response) {
                throw new ArgumentException("Invalid response message");
            }

            if (header.Truncated) {
                return new Response(header,
                    Question.GetAllFromArray(message, offset, header.QuestionCount),
                    new List<IResourceRecord>(),
                    new List<IResourceRecord>(),
                    new List<IResourceRecord>());
            }

            return new Response(header,
                Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AnswerRecordCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AuthorityRecordCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out offset));
        }

        public Response(Header header, IList<Question> questions, IList<IResourceRecord> answers,
                IList<IResourceRecord> authority, IList<IResourceRecord> additional) {
            this.header = header;
            this.questions = questions;
            this.answers = answers;
            this.authority = authority;
            this.additional = additional;
        }

        public Response() {
            this.header = new Header();
            this.questions = new List<Question>();
            this.answers = new List<IResourceRecord>();
            this.authority = new List<IResourceRecord>();
            this.additional = new List<IResourceRecord>();

            this.header.Response = true;
        }

        public Response(IResponse response) {
            this.header = new Header();
            this.questions = new List<Question>(response.Questions);
            this.answers = new List<IResourceRecord>(response.AnswerRecords);
            this.authority = new List<IResourceRecord>(response.AuthorityRecords);
            this.additional = new List<IResourceRecord>(response.AdditionalRecords);

            this.header.Response = true;

            Id = response.Id;
            RecursionAvailable = response.RecursionAvailable;
            AuthorativeServer = response.AuthorativeServer;
            OperationCode = response.OperationCode;
            ResponseCode = response.ResponseCode;
        }

        public IList<Question> Questions {
            get { return questions; }
        }

        public IList<IResourceRecord> AnswerRecords {
            get { return answers; }
        }

        public IList<IResourceRecord> AuthorityRecords {
            get { return authority; }
        }

        public IList<IResourceRecord> AdditionalRecords {
            get { return additional; }
        }

        public int Id {
            get { return header.Id; }
            set { header.Id = value; }
        }

        public bool RecursionAvailable {
            get { return header.RecursionAvailable; }
            set { header.RecursionAvailable = value; }
        }

        public bool AuthenticData {
            get { return header.AuthenticData; }
            set { header.AuthenticData = value; }
        }

        public bool CheckingDisabled {
            get { return header.CheckingDisabled; }
            set { header.CheckingDisabled = value; }
        }

        public bool AuthorativeServer {
            get { return header.AuthorativeServer; }
            set { header.AuthorativeServer = value; }
        }

        public bool Truncated {
            get { return header.Truncated; }
            set { header.Truncated = value; }
        }

        public OperationCode OperationCode {
            get { return header.OperationCode; }
            set { header.OperationCode = value; }
        }

        public ResponseCode ResponseCode {
            get { return header.ResponseCode; }
            set { header.ResponseCode = value; }
        }

        public int Size {
            get {
                return header.Size +
                    questions.Sum(q => q.Size) +
                    answers.Sum(a => a.Size) +
                    authority.Sum(a => a.Size) +
                    additional.Sum(a => a.Size);
            }
        }

        public byte[] ToArray() {
            UpdateHeader();
            ByteStream result = new ByteStream(Size);

            result
                .Append(header.ToArray())
                .Append(questions.Select(q => q.ToArray()))
                .Append(answers.Select(a => a.ToArray()))
                .Append(authority.Select(a => a.ToArray()))
                .Append(additional.Select(a => a.ToArray()));

            return result.ToArray();
        }

        public override string ToString() {
            UpdateHeader();

            return ObjectStringifier.New(this)
                .Add(nameof(Header), header)
                .Add(nameof(Questions), nameof(AnswerRecords), nameof(AuthorityRecords), nameof(AdditionalRecords))
                .ToString();
        }

        private void UpdateHeader() {
            header.QuestionCount = questions.Count;
            header.AnswerRecordCount = answers.Count;
            header.AuthorityRecordCount = authority.Count;
            header.AdditionalRecordCount = additional.Count;
        }
    }
}
