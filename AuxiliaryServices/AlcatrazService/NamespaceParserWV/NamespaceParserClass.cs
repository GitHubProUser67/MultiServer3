using System;
using System.IO;
using System.Text;

namespace NamespaceParserWV
{
    // https://github.com/zeroKilo/GROBackendWV/blob/master/NamespaceParserWV/Form1.cs
    public class NamespaceParserClass
    {
        private StringBuilder sb;

        public string ScanMemory(byte[] data)
        {
            sb = new StringBuilder();

            using (MemoryStream m = new MemoryStream(data))
            {
                try
                {
                    while (m.Position < data.Length)
                        Parse(m);
                }
                catch
                {
                    Log("Position = 0x" + m.Position.ToString("X8"));
                }
                return sb.ToString();
            }
        }

        public string ScanExecutable(byte[] data)
        {
            sb = new StringBuilder();

            using (MemoryStream m = new MemoryStream(data))
            {
                try
                {
                    while (m.Position < data.Length)
                    {
                        if (ReadU32(m) == 0xCD652312)
                        {
                            m.Seek(17, SeekOrigin.Current);
                            Parse(m);
                            while ((m.Position % 4) != 0)
                                m.ReadByte();
                        }
                    }
                }
                catch
                {
                    Log("Position = 0x" + m.Position.ToString("X8"));
                }
                return sb.ToString();
            }
        }

        private static string ReadString(Stream s)
        {
            string result = string.Empty;
            uint len = ReadU32(s);
            if (len > 1000)
                throw new Exception();
            for (int i = 0; i < len; i++)
                result += (char)s.ReadByte();
            return result;
        }

        private static uint ReadU32(Stream s)
        {
            uint result = 0;
            for (int i = 0; i < 4; i++)
            {
                result <<= 8;
                result |= (byte)s.ReadByte();
            }
            return result;
        }

        private void Log(string s)
        {
            sb.AppendLine(s);
        }

        private static string MakeTabs(int depth)
        {
            string tabs = string.Empty;
            for (int i = 0; i < depth; i++)
                tabs += "\t";
            return tabs;
        }

        private void Parse(Stream m, int depth = 0)
        {
            uint count;
            _ = MakeTabs(depth);
            count = ReadU32(m);
            for (int i = 0; i < count; i++)
            {
                byte type = (byte)m.ReadByte();
                switch (type)
                {
                    case 3:
                        ParseDOClassDeclaration(m, depth);
                        break;
                    case 4:
                        ParseDatasetDeclaration(m, depth);
                        break;
                    case 6:
                        ParseVariable(m, depth);
                        break;
                    case 8:
                        ParseRMC(m, depth);
                        break;
                    case 9:
                        ParseAction(m, depth);
                        break;
                    case 0xA:
                        ParseAdapterDeclaration(m, depth);
                        break;
                    case 0xB:
                        ParsePropertyDeclaration(m, depth);
                        break;
                    case 0xC:
                        ParseProtocolDeclaration(m, depth);
                        break;
                    case 0xD:
                        ParseParameter(m, depth);
                        break;
                    case 0xE:
                        ParseReturnValue(m, depth);
                        break;
                    case 0xF:
                        ParseClassDeclaration(m, depth);
                        break;
                    case 0x10:
                        ParseTemplateDeclaration(m, depth);
                        break;
                    case 0x11:
                        ParseSimpleTypeDeclaration(m, depth);
                        break;
                    case 0x12:
                        ParseTemplateInstance(m, depth);
                        break;
                    case 0x13:
                        ParseDDLUnitDeclaration(m, depth);
                        break;
                    case 0x14:
                        ParseDupSpaceDeclaration(m, depth);
                        break;
                    default:
                        Log("Unknown type found: 0x" + type.ToString("X2"));
                        throw new Exception();
                }
            }
        }

        private void ParseDDLUnitDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[DDL Unit Declaration]");
            ParseDeclaration(m, depth + 1);
            Log(tabs + "\t[" + ReadString(m) + "]");
            Log(tabs + "\t[" + ReadString(m) + "]");
        }

        private void ParseDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Declaration]");
            ParseNamespaceItem(m, depth + 1);
            ParseNamespace(m, depth + 1);
        }

        private void ParseNamespaceItem(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Namespace Item]");
            Log(tabs + "\t[" + ReadString(m) + "]");
            Log(tabs + "\t[" + ReadString(m) + "]");
        }

        private void ParseNamespace(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Namespace]");
            Log(tabs + "\t[" + ReadString(m) + "]");
            Parse(m, depth + 1);
        }

        private void ParseTemplateInstance(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Template Instance]");
            ParseDeclaration(m, depth + 1);
            ParseNamedStringList(m, depth + 1);
        }

        private void ParseNamedStringList(Stream m, int depth = 0)
        {
            uint count;
            string tabs = MakeTabs(depth);
            Log(tabs + "[Named String List]");
            Log(tabs + "\t[" + ReadString(m) + "]");
            count = ReadU32(m);
            Log(tabs + "\t[0x" + count.ToString("X8") + "]");
            for (int i = 0; i < count; i++)
                Log(tabs + "\t\t[" + ReadString(m) + "]");
        }

        private void ParseClassDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Class Declaration]");
            ParseDeclaration(m, depth + 1);
            ParseNamespace(m, depth + 1);
        }

        private void ParsePropertyDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Property Declaration]");
            ParseNamespaceItem(m, depth + 1);
            ParseNamespace(m, depth + 1);
            Log(tabs + "\t[0x" + ReadU32(m).ToString("X8") + "]");
            Log(tabs + "\t[0x" + ReadU32(m).ToString("X8") + "]");
        }

        private void ParseVariable(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Variable]");
            Log(tabs + "\t[" + ReadString(m) + "]");
            Log(tabs + "\t[" + ReadString(m) + "]");
            ParseDeclarationUse(m, depth + 1);
            Log(tabs + "\t[0x" + ReadU32(m).ToString("X8") + "]");

        }

        private void ParseDeclarationUse(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            byte b = (byte)m.ReadByte();
            ENamespaceItem type = (ENamespaceItem)b;
            Log(tabs + $"[{type:G}]");
            if ((ENamespaceItem)b == ENamespaceItem.TemplateInstance)
                ParseTemplateDeclarationUse(m, depth + 1);
            else
                Log(tabs + "\t[" + ReadString(m) + "]");
        }

        private void ParseTemplateDeclarationUse(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[TemplateInstance]");
            Log(tabs + "\t[" + ReadString(m) + "]");
            Log(tabs + "\t[" + ReadString(m) + "]");
            byte count = (byte)m.ReadByte();
            for (int i = 0; i < count; i++)
                ParseDeclarationUse(m, depth + 1);
        }

        private void ParseProtocolDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Protocol Declaration]");
            ParseDeclaration(m, depth + 1);
            Parse(m, depth + 1);
        }

        private void ParseRMC(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[RMC]");
            ParseProtocolDeclaration(m, depth + 1);
            Parse(m, depth + 1);
        }

        private void ParseParameter(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Parameter]");
            ParseVariable(m, depth + 1);
            ParseDeclarationUse(m, depth + 1);
            Log(tabs + "\t[0x" + ReadU32(m).ToString("X8") + "]");
            Log(tabs + "\t[0x" + ((byte)m.ReadByte()).ToString("X8") + "]");
        }

        private void ParseReturnValue(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Return Value]");
            ParseVariable(m, depth + 1);
            ParseDeclarationUse(m, depth + 1);
            Log(tabs + "\t[0x" + ReadU32(m).ToString("X8") + "]");
        }

        private void ParseTemplateDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Template Declaration]");
            ParseDeclaration(m, depth + 1);
            Log(tabs + "\t[0x" + ReadU32(m).ToString("X8") + "]");
        }

        private void ParseSimpleTypeDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Simple Type Declaration]");
            ParseDeclaration(m, depth + 1);
        }

        private void ParseDatasetDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Dataset Declaration]");
            ParseNamespaceItem(m, depth + 1);
            ParseNamespace(m, depth + 1);
            Parse(m, depth + 1);
        }

        private void ParseDOClassDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[DO Class Declaration]");
            ParseNamespaceItem(m, depth + 1);
            ParseNamespace(m, depth + 1);
            Log(tabs + "\t[" + ReadString(m) + "]");
            Log(tabs + "\t[0x" + ReadU32(m).ToString("X8") + "]");
            Parse(m, depth + 1);
        }

        private void ParseAdapterDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Adapter Declaration]");
            ParseDeclaration(m, depth + 1);
        }

        private void ParseDupSpaceDeclaration(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Duplicated Space Declaration]");
            ParseDeclaration(m, depth + 1);
        }

        private void ParseAction(Stream m, int depth = 0)
        {
            string tabs = MakeTabs(depth);
            Log(tabs + "[Action]");
            ParseProtocolDeclaration(m, depth + 1);
            Parse(m, depth + 1);
        }
    }
}
