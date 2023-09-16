using System.Text.RegularExpressions;

namespace MultiServer.CryptoSporidium.BAR
{
    public class FileTypeAnalyser
    {
        public static FileTypeAnalyser Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new FileTypeAnalyser();
                return m_instance;
            }
        }

        private FileTypeAnalyser()
        {
            m_FileTypeExtensions = new Dictionary<HomeFileType, string>();
            RegisterFileTypes();
        }

        public string GetRegisteredExtension(HomeFileType type)
        {
            return m_FileTypeExtensions[type];
        }

        private void RegisterFileTypes()
        {
            m_FileTypeExtensions[HomeFileType.Collision] = ".hkx";
            m_FileTypeExtensions[HomeFileType.Model] = ".mdl";
            m_FileTypeExtensions[HomeFileType.Animation] = ".ani";
            m_FileTypeExtensions[HomeFileType.Skin] = ".skn";
            m_FileTypeExtensions[HomeFileType.Texture] = ".dds";
            m_FileTypeExtensions[HomeFileType.Xml] = ".xml";
            m_FileTypeExtensions[HomeFileType.LUASource] = ".lua";
            m_FileTypeExtensions[HomeFileType.Scene] = ".scene";
            m_FileTypeExtensions[HomeFileType.MP3] = ".mp3";
            m_FileTypeExtensions[HomeFileType.LightProbe] = ".probe";
            m_FileTypeExtensions[HomeFileType.Unknown] = string.Empty;
        }

        public HomeFileType Analyse(Stream inStream)
        {
            HomeFileType result = HomeFileType.Unknown;
            try
            {
                TextReader textReader = new StreamReader(inStream);
                string text = textReader.ReadLine();
                if (text == null)
                {
                    result = HomeFileType.Unknown;
                    return result;
                }
                if (text.StartsWith("DDS"))
                {
                    result = HomeFileType.Texture;
                    return result;
                }
                if (text.StartsWith("HM"))
                {
                    result = HomeFileType.Model;
                    return result;
                }
                if (text.StartsWith("AC11"))
                {
                    result = HomeFileType.Animation;
                    return result;
                }
                if (text.StartsWith("SK08"))
                {
                    result = HomeFileType.Skin;
                    return result;
                }
                if (text.StartsWith("WW"))
                {
                    result = HomeFileType.Collision;
                    return result;
                }
                if (text.StartsWith("PR"))
                {
                    result = HomeFileType.LightProbe;
                    return result;
                }
                if (text.Contains("<?xml"))
                {
                    result = HomeFileType.Xml;
                    string text2 = textReader.ReadLine();
                    if (text2 != null && text2.StartsWith("<gap:game"))
                    {
                        result = HomeFileType.Scene;
                        return result;
                    }
                }
                if (text.StartsWith("<gap:game"))
                {
                    result = HomeFileType.Scene;
                    return result;
                }
                if (text.StartsWith("<") && text.EndsWith(">"))
                {
                    result = HomeFileType.Xml;
                    return result;
                }
                if (text.StartsWith("<!--"))
                {
                    result = HomeFileType.Xml;
                    return result;
                }
                if (text.StartsWith("ID3"))
                {
                    result = HomeFileType.MP3;
                    return result;
                }
                string text3 = textReader.ReadToEnd();
                if (Regex.IsMatch(text3, "function[\\s\\w\\d]+()"))
                    result = HomeFileType.LUASource;
                else if (text3.Contains("LoadLibrary"))
                    result = HomeFileType.LUASource;
                else if (Regex.IsMatch(text3, ";{}"))
                    result = HomeFileType.LUASource;
                textReader.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        private static FileTypeAnalyser m_instance;

        private Dictionary<HomeFileType, string> m_FileTypeExtensions;
    }
}
