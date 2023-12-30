using System.Reflection;
using System.Text;

namespace SRVEmu.Messages
{
    public abstract class AbstractMessage
    {
        public abstract string _Name { get; }
        public string PlaintextData = string.Empty;

        public void Read(string input)
        {
            input = input.TrimEnd('\0');
            Type? type = GetType();

            bool hasN = input.IndexOf('\n') > 0;
            string[] pairs;
            if (hasN) pairs = input.Split('\n');
            else pairs = input.Split((char)9);

            foreach (string pair in pairs)
            {
                if (pair.Length == 0) continue;
                string[] eqSplit = pair.Split('=');
                if (eqSplit.Length < 2) continue;
                string? value = eqSplit[1];
                if (eqSplit.Length > 1)
                {
                    if (string.IsNullOrEmpty(value)) continue;
                    else if (value[0] == '\"' && value[^1] == '\"')
                        value = value[1..^1];
                }
                PropertyInfo? prop = type.GetProperty(eqSplit[0]);
                if (prop == null || (prop.PropertyType != typeof(string)) || prop.Name[0] == '_')
                {
                    if (!eqSplit[0].Contains("TIME"))
                        CustomLogger.LoggerAccessor.LogError("Unexpected Proprerty: " + pair + " (for " + type.ToString() + ")");
                }
                else
                    prop.SetValue(this, value);
            }
        }

        public string Write()
        {
            Type? type = GetType();
            PropertyInfo[] props = type.GetProperties();
            StringBuilder keyValue = new();
            foreach (PropertyInfo? prop in props)
            {
                if ((prop.PropertyType != typeof(string) && prop.PropertyType != typeof(string[])) || prop.Name[0] == '_') continue;
                if (prop.PropertyType == typeof(string[]))
                {
                    string[]? values = (string[]?)prop.GetValue(this);
                    if (values == null) continue;
                    for (int i = 0; i<values.Length; i++)
                    {
                        if (values[i] != null)
                            keyValue.Append(EncodeKV(prop.Name, values[i]));
                    }
                }
                else
                {
                    string? value = (string?)prop.GetValue(this);
                    if (value == null)
                        continue;
                    keyValue.Append(EncodeKV(prop.Name, value));
                }
            }
            if (keyValue.Length == 0) keyValue.Append('\n');

            return keyValue.ToString();
        }

        private string EncodeKV(string key, string value)
        {
            // Custom Rules:

            switch (key)
            {
                case "LUID":
                    key = "_LUID";
                    break;
                default:
                    break;
            }

            return key + "=" + value + '\n';
        }

        public virtual void Process(AbstractDirtySockServer context, DirtySockClient client)
        {

        }

        public byte[] GetData()
        {
            var plaintext = _Name.Length == 8;
            var body = (plaintext ? PlaintextData : Write()) + "\0";
            var size = body.Length + 12;

            MemoryStream mem = new();
            BinaryWriter io = new(mem);
            io.Write(Encoding.ASCII.GetBytes(_Name));
            if (!plaintext)
                io.Write(0); //4 byte pad
            io.Write(new byte[] { (byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size });
            io.Write(Encoding.ASCII.GetBytes(body));

            byte[] bytes = mem.ToArray();
            io.Dispose();
            mem.Dispose();
            return bytes;
        }
    }
}
