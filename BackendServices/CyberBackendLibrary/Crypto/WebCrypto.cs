using CyberBackendLibrary.HTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using CastleLibrary.Utils.AES;

namespace CyberBackendLibrary.Crypto
{
    public static class WebCrypto
    {
        public static readonly byte[] AuthIV = new byte[] { 0x30, 0x57, 0xB5, 0x1F, 0x32, 0xD4, 0xAD, 0xBF, 0xAA, 0xAA, 0x21, 0x41, 0x6C, 0xDC, 0x5D, 0xF5 };

        private static readonly double[] ChecksumDoubleBox = new double[] {
        2.7887184390393327E-68, -1.8618331480548654E+44, -4.3020517462112105E-88, 2.7690669394447383E-306,
        -4.4732610650720826E+186, -1.4390889935876456E-122, 9.011151663372176E-162, 4.2908917155383753E-218,
        -2.4128387770212816E-68, -2.302277439516697E+17, -1.42652914942737E+76, -5.8837740751567983E+133,
        1.5920040750264217E+296, 1.0396241970410689E+80, -4.5251989463276728E-37, -99908801504.216187,
        -9.6938069800013986E-234, 7154320198.8792391, 3.3350146220305454, 4.5992494633926639E+296,
        0.00015538478019864411, -2.3770471563162649E-231, -3.7776792306925663E-257, -3.2751772927936956E-79,
        2.9753036182759686E+19, -3.4108151155201765E-97, 1.8254489178010996E-150, -1.533309463713607E+223,
        -3.3038829395282477E-163, -3.5867627522725082E-44, -8.0575990854406417E+228, -4.5693368234658098E+218,
        1.5758545622916187E+250, -3.8469203628037247E+75, -7.7468478373878765E+283, 2.9070685939131584E-109,
        6.3703271911431292E-267, 3221.4418522251376, 5.4205212587320372E+146, -8.0205306949294802E+33,
        -9.4975431916177958E-136, 3.667794057662362E-115, 7.7199257831233639E-151, -2.1070369526203118E+101,
        1.6411659940921769E-155, -3.6168969623686786E-163, 2.1579389902107392E-277, 4.874590773217776E-111,
        4.0857360992391282E-165, 1.0606435456003952E+267, 5.7931290514683669E-152, -6.7290098482703866E+52,
        1.2606024827897781E+296, -1.6704953006900546E-176, -3.0078722972120132E+48, 3.1447104851678525E+301,
        1915.2152581197442, 3.3184196363573966E+181, 6.1149917535759973E-276, 5.4064678119532772E-23,
        -1.0984738414178737E-88, 6.1990027968293353E-291, 3.4566479109573149E-196, -5.3509295815048191E-26,
        1.6505428070349743E+277, -9.6811524958504798E+67, 1.1280023390074459E+149, -4.9292339788735822E-68,
        2.2601210796010041E+178, 8.5226517645726473E+190, -1.0017171101609985E-279, -3.65646314408369E-50,
        2.2970458601470527E+32, 1.0565926139826181E+120, -1.2064366402132023E-257, -2.9763571200300399E-212,
        7.4113698535820762E+30, 4.8640419208754734E-102, 10521018956905.494, 2.4526076739600515E-218,
        -1.5830022524963814E-73, 3.4336565057851734E+189, -2.3577655252445132E-156, -9.8917585013646492E-214,
        1.0989790604300809E+275, -1.2587740892853646E+280, 9.4814881264904194E-182, -1.1571531521085438E+237,
        1.229603368447179E+177, -2.604114864379276E+231, -9.2664762752613728E+19, 3.1108827462661377E+207,
        -0.00097578440812214106, -1.6334311869848364E+64, 2.1522092663265792E-148, -9.703594347075139E-116,
        -5.38618978004519E+153, 1.5183273612167631E-68, 1.488965790360276E+231, -1.9635140707561793E+263,
        8.3909068205945064E-24, 9.0358877500255654E+296, -9.1999672792620295E+304, 3.5512093967849135E+264,
        -2.8892650165544142E+105, 4.3108753206738937E-193, -1.8348272430681013E-43, 5.1858841474652699E-213,
        -3.004636725533007E-243, 6.132335794645227E+296, -3.7468512147382775E-154, 7.6218053022237471E-96,
        4.3743536049921071E-103, 5.9816034318820231E-216, 1.460199038405993E-91, 6.5876192666238764E+29,
        -7.3794025156926594E+268, 5.7991955341983553E-13, -5.6634238293438093E+89, -5.8200791917381605E+113,
        -9.382352775848059E+207, 2.861558467972407E-77, 2.4696625114917344E-110, 2.7557374555587394E-267,
        -2.4824860795877484E+248, -2.3404664456505793E+142, 1.1936363470265948E-138, -5.1554094132155965E+218,
        -2.1345621384067773E+95, 3.9095198281989856E-22, -2.5788222256938449E+115, 1.9099286980025499E-239,
        5.5884812735780678E-97, -4.3532845559098668E-234, -1.2510799969917003E-181, 4.5603347604034982E+136,
        4.7061203764035123E-83, 9.1737236235132167E-41, -2.613613939729813E-298, 1.9880402498870573E+83,
        1.7345656478778084E+249, -3.0319077060207514E-105, -6.7629333338910013E+294, -1.6866269280513859E+99,
        -3.3783315435921158E-248, -5.0835175037376279E+51, 2.2823484706956437E+223, -5.2350800373676555E-66,
        2.1938940008469276E-254, 2.6638615147066538E+223, 2.4842709854584848E+182, -1.0299057921221703E-27,
        3.0990818534379688E+185, 2.0262114067427639E-188, -5.4296554749002973E+66, 4.7675539353956493E+169,
        1.3573132727962003E-118, -6.5274465463956772E-14, -4.5392607746338724E-127, 1.6102418832131022E-23,
        -1.1847308830399821E+132, 1.2513568633577283E-16, -1.0228949260107244E+112, -8.9922531416247386E+192,
        7.916548295980459E-222, 1.2312727320104881E+232, -1.6122158694745404E-18, -3.481233193000274E-90,
        1.1167822845877998E-174, 3.0933218387919583E+155, 3.5277642137107372E+44, -2.9137622769215298E+290,
        1.9957749835438084E+236, 6.1872472933038688E-164, 5.2600259335078834E+243, -1.4443859349276303E+148,
        5.1467859055870169E+229, 7.7995527993037581E+130, -1.1114533180621355E-125, 2.6399665987764258E+231,
        4.2784642805717917E+60, 5.4325889050482818E-24, 2.3471782221662184E-139, 1.5023114689184499E+139,
        -7.739248628015553E-261, -1.2718020201744012E+243, -1.9381306484670001E+67, -4.6914660580129212E+70,
        -1.1133219329239814E-265, 3.6846941503987795E+249, -2.0748351868579771E+204, 6.6937077510630353E+112,
        -6.2048307225072857E-139, 3.176804470596129E-165, -2.7918709132760115E-30, -1.0492637299075539E+301,
        -2.6913198235452055E+26, -2.9117223403452271E+233, -4.7027363008739307E-228, 1.8114587021063903E-271,
        -1.3241417053933245E+48, 3.6325124729858898E+91, -6.2457791807094912E-141, -1.4491174268613238E+237,
        9.8874123752594577E+120, -2.0319709324516646E+28, -1.5957803938054728E+152, 3.1874238863547971E+279,
        -1.8250643090422224E-303, 5.9599696895418363E+64, -2.2787607401244759E-36, 1.5008922629428054E+65,
        1.1505681598544734E+148, -1.3637263423245691E+49, -4.8856652999596355E-147, -2.9116196687899166E+205,
        1.8874332547772457E+175, -6.5990215259187729E-33, 2.6264478209319808E+277, 1.5689005450286684E-299,
        -6.1497353187898311E-262, 1.9581923365154108E+242, 3.8357464877695169E+219, -7.0508384785114966E+159,
        6.0207319343426195E+116, 1.5765702742095276E+54, 4.9958914890061474E+286, -3.6781148546295676E-239,
        -6.0586380916259018E-146, 1.0108359360144948E-84, 1.0745744993668423E-293, 9.2956759200531128E+287,
        -4.7638929609005592E+115, 9.5054275587231182E-84, -1.8743692919210275E+266, -2.0903280235511906E-65,
        2.6520969143313191E+44, 2.5068662790255933E-108, 7.4153606567030972E-173, 0.027621597514295559,
        1.4414796379735879E-59, 8.9494241208820869E+230, -1.9129924797775677E+176, -3.8951193066001035E-157,
        1.1346280594792284E-121, -2.316668663199865E-205, -1.5072932644663529E-226, -1.629331170675189E-133,
        1.9851829729633109E-177, 9.7822255568416942E+240, 9.862710004113815E-199, -6.1538515262395028E+109,
        6.7804585434686301E-67, -6.4119939251743156E-51, -5.3875889389093929E+155, 8.3674428579373504E+136
        };

        private static readonly byte[] ChecksumByteBox = new byte[] { 244, 255, 193, 198, 73,
            25, 27, 127, 192, 90, 35, 239, 122, 141, 223, 225, 111, 127, 53, 226, 240, 185,
            90, 26, 57, 224, 100, 115, 35, 196, 114, 197, 142, 108, 119, 62, 60, 55, 16, 167,
            73, 53, 169, 40, 105, 223, 222, 166, 178, 14, 131, 5, 9, 148, 181, 37, 215, 123,
            75, 255, 32, 134, 166, 29, 115, 164, 102, 232, 166, 204, 45, 202, 57, 75, 21, 119,
            255, 197, 117, 203, 227, 3, 51, 167, 232, 34, 125, 150, 10, 90, 112, 252, 33, 76,
            41, 216, 1, 65, 153, 8, 186, 201, 230, 193, 150, 240, 75, 181, 160, 15, 69, 214,
            176, 36, 71, 251, 216, 105, 35, 95, 220, 233, 168, 63, 51, 116, 54, 152, 208,
            228, 207, 248, 167, 183, 184, 16, 102, 84, 156, 99, 82, 204, 43, 74, 72, 247,
            134, 63, 26, 91, 190, 240, 10, 245, 244, 23, 100, 143, 35, 130, 51, 81, 57, 120,
            175, 192, 179, 240, 193, 60, 233, 177, 128, 67, 103, 48, 98, 101, 134, 114, 92,
            114, 155, 187, 63, 120, 7, 246, 12, 9, 6, 131, 57, 217, 8, 49, 253, 166, 38, 0,
            242, 163, 128, 236, 2, 210, 86, 54, 63, 124, 252, 5, 175, 108, 65, 216, 106, 240,
            186, 72, 63, 38, 93, 220, 44, 18, 193, 255, 179, 67, 231, 39, 254, 235, 174, 24,
            112, 136, 67, 56, 237, 179, 166, 68, 167, 147, 197, 51, 58, 248, 240, 215, 57, 
            64, 72, 113 };

        public static string? Encrypt(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[]? EncryptToByteArray(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            if (string.IsNullOrEmpty(result))
                return null;
            else
                return Encoding.UTF8.GetBytes(result);
        }

        public static string? EncryptNoPreserve(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[]? EncryptNoPreserveToByteArray(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            if (string.IsNullOrEmpty(result))
                return null;
            else
                return Encoding.UTF8.GetBytes(result);
        }

        public static string? Decrypt(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static string? Decrypt(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static byte[]? DecryptToByteArray(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static byte[]? DecryptToByteArray(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static string? ProcessSecureCheckum(byte[] inputData, long initialValue)
        {
            byte currentByte = 0;
            int DataIndex = 0;
            double tempDouble = BitConverter.Int64BitsToDouble(!BitConverter.IsLittleEndian ? EndianTools.EndianUtils.EndianSwap(initialValue) : initialValue);
            byte[] CheckSumBytes = new byte[inputData.Length];

            Ionic.Crc.CRC32 crc = new();

            do
            {
                currentByte = (byte)(inputData[DataIndex] ^ BitConverter.DoubleToInt64Bits(!BitConverter.IsLittleEndian ? EndianTools.EndianUtils.EndianSwap(tempDouble) : tempDouble));
                CheckSumBytes[DataIndex] = currentByte;
                tempDouble = ChecksumDoubleBox[currentByte] /
                                    (((uint)ChecksumByteBox[(uint)tempDouble & 0xFF] << 0x10) |
                                    ((uint)ChecksumByteBox[(uint)tempDouble >> 8 & 0xFF] << 8) |
                                    ChecksumByteBox[(uint)tempDouble >> 0x10 & 0xFF] | 0x3F000000);
                DataIndex++;
            } while (DataIndex != inputData.Length);

            crc.SlurpBlock(CheckSumBytes, 0, CheckSumBytes.Length);

            return $"{crc.Crc32Result:X4}";
        }
    }
}
