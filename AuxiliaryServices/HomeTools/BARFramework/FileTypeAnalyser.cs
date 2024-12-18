using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HomeTools.BARFramework
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
            m_FileTypeExtensions[HomeFileType.Effect] = ".effect";
            m_FileTypeExtensions[HomeFileType.Skin] = ".skn";
            m_FileTypeExtensions[HomeFileType.Texture] = ".dds";
            m_FileTypeExtensions[HomeFileType.PNG] = ".png";
            m_FileTypeExtensions[HomeFileType.Xml] = ".xml";
            m_FileTypeExtensions[HomeFileType.LUASource] = ".lua";
            m_FileTypeExtensions[HomeFileType.LUACompiled] = ".luac";
            m_FileTypeExtensions[HomeFileType.Scene] = ".scene";
            m_FileTypeExtensions[HomeFileType.MP3] = ".mp3";
            m_FileTypeExtensions[HomeFileType.MP4] = ".mp4";
            m_FileTypeExtensions[HomeFileType.Bank] = ".bnk";
            m_FileTypeExtensions[HomeFileType.Font] = ".ttf";
            m_FileTypeExtensions[HomeFileType.LightProbe] = ".probe";
            m_FileTypeExtensions[HomeFileType.Unknown] = string.Empty;
        }

        public static HomeFileType GetFileType(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".hkx": return HomeFileType.Collision;
                case ".mdl": return HomeFileType.Model;
                case ".ani": return HomeFileType.Animation;
                case ".effect": return HomeFileType.Effect;
                case ".skn": return HomeFileType.Skin;
                case ".dds": return HomeFileType.Texture;
                case ".png": return HomeFileType.PNG;
                case ".xml": return HomeFileType.Xml;
                case ".lua": return HomeFileType.LUASource;
                case ".luac": return HomeFileType.LUACompiled;
                case ".scene": return HomeFileType.Scene;
                case ".mp3": return HomeFileType.MP3;
                case ".mp4": return HomeFileType.MP4;
                case ".bnk": return HomeFileType.Bank;
                case ".ttf": return HomeFileType.Font;
                case ".probe": return HomeFileType.LightProbe;
                case ".bar": return HomeFileType.BarArchive;
                case ".sharc": return HomeFileType.BarArchive;
                default: return HomeFileType.Unknown;
            }
        }

        public HomeFileType Analyse(Stream inStream)
        {
            HomeFileType result = HomeFileType.Unknown;
            try
            {
                using (TextReader textReader = new StreamReader(inStream))
                {
                    string text = textReader.ReadToEnd();

                    if (string.IsNullOrEmpty(text))
                        result = HomeFileType.Unknown;
                    else if (text.StartsWith("DDS |"))
                        result = HomeFileType.Texture;
                    else if (text.StartsWith("LuaQ"))
                        result = HomeFileType.LUACompiled;
                    else if (text.StartsWith("HM") || text.StartsWith("MR04"))
                        result = HomeFileType.Model;
                    else if (text.StartsWith("‰PNG") || text.Contains("Photoshop ICC profile") || text.Contains("IHDR"))
                        result = HomeFileType.PNG;
                    else if (text.StartsWith("AC11"))
                        result = HomeFileType.Animation;
                    else if (text.StartsWith("SK08"))
                        result = HomeFileType.Skin;
                    else if (text.StartsWith("WW") || text.Contains("Havok-5.0.0-r1"))
                        result = HomeFileType.Collision;
                    else if (text.StartsWith("PR"))
                        result = HomeFileType.LightProbe;
                    else if (text.Contains("CHNK"))
                        result = HomeFileType.Effect;
                    else if (text.Contains("<gap:game"))
                        result = HomeFileType.Scene;
                    else if (text.StartsWith("<") && text.EndsWith(">"))
                        result = HomeFileType.Xml;
                    else if (text.StartsWith("<!--"))
                        result = HomeFileType.Xml;
                    else if (text.Contains("<?xml"))
                        result = HomeFileType.Xml;
                    else if (text.Contains("klBS"))
                        result = HomeFileType.Bank;
                    else if (text.StartsWith("ID3") || text.Contains("LAME3.") || text.Contains("SfMarkers"))
                        result = HomeFileType.MP3;
                    else if (text.Contains("ftypmp42"))
                        result = HomeFileType.MP4;
                    else if (Regex.IsMatch(text, "function[\\s\\w\\d]+()"))
                        result = HomeFileType.LUASource;
                    else if (text.Contains("LoadLibrary"))
                        result = HomeFileType.LUASource;
                    else if (Regex.IsMatch(text, ";{}"))
                        result = HomeFileType.LUASource;
                    else if (text.Contains("DSIG"))
                        result = HomeFileType.Font;

                    textReader.Close();
                }
            }
            catch
            {
                result = HomeFileType.Unknown;
            }
            return result;
        }

        private static FileTypeAnalyser m_instance;

        private Dictionary<HomeFileType, string> m_FileTypeExtensions;
    }
}