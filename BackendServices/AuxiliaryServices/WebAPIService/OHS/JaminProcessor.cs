using System;
using CustomLogger;
using Newtonsoft.Json.Linq;
using NLua;
using System.Text;
using System.Linq;

namespace WebAPIService.OHS
{
    public class JaminProcessor
    {
        public static bool VerifyHash(string str, string referencehash)
        {
            if (EncryptDecrypt.Hash32Str(str).Equals(referencehash, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public static (string, string?) JaminDeFormatWithWriteKey(string? dataforohs, bool hashed, int game, bool unescape = true)
        {
            try
            {
                string writekey = string.Empty;

                // Execute the Lua script and get the result
                object[]? returnValues = null;

                if (!string.IsNullOrEmpty(dataforohs))
                {
                    if (unescape)
                        dataforohs = EncryptDecrypt.UnEscape(dataforohs);

                    if (game != 0 && !string.IsNullOrEmpty(dataforohs))
                        dataforohs = EncryptDecrypt.Decrypt(dataforohs, game);

                    if (!string.IsNullOrEmpty(dataforohs))
                    {
#if DEBUG
                        LoggerAccessor.LogInfo($"[OHS] - JaminDeFormatWithWriteKey Assembled Data : {dataforohs}");
#endif
                        if (hashed)
                        {
                            string InData = dataforohs[8..]; // We remove the hash.
                            if (VerifyHash(InData, dataforohs[..8]))
                            {
                                writekey = InData[..8];
                                InData = InData[8..]; // We remove the writekey.
                                returnValues = ExecuteLuaScript(LUAJaminCode.LUAJaminDecryptor.Replace("PUT_FORMATEDJAMINVALUE_HERE", ToLiteral(InData)));
                            }
                        }
                        else
                        {
                            writekey = dataforohs[..8];
                            dataforohs = dataforohs[8..]; // We remove the writekey.
                            returnValues = ExecuteLuaScript(LUAJaminCode.LUAJaminDecryptor.Replace("PUT_FORMATEDJAMINVALUE_HERE", ToLiteral(dataforohs)));
                        }

                        if (!string.IsNullOrEmpty(returnValues?[0].ToString()))
                        {
                            string? endvalue = returnValues[0].ToString();
#if DEBUG
                            LoggerAccessor.LogInfo($"[OHS] - JaminDeFormatWithWriteKey De-Assembled Data : {endvalue}");
#endif
                            return (writekey, endvalue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"[OHS] - JaminDeFormatWithWriteKey function JaminDeFormat failed - {ex}");
            }

            return ("11111111", null);
        }

        public static string? JaminDeFormat(string? dataforohs, bool hashed, int game, bool unescape = true)
        {
            try
            {
                // Execute the Lua script and get the result
                object[]? returnValues = null;

                if (!string.IsNullOrEmpty(dataforohs))
                {
                    if (unescape)
                        dataforohs = EncryptDecrypt.UnEscape(dataforohs);

                    if (game != 0 && !string.IsNullOrEmpty(dataforohs))
                        dataforohs = EncryptDecrypt.Decrypt(dataforohs, game);

                    if (!string.IsNullOrEmpty(dataforohs))
                    {
#if DEBUG
                        LoggerAccessor.LogInfo($"[OHS] - JaminDeFormat Assembled Data : {dataforohs}");
#endif
                        if (hashed)
                        {
                            string InData = dataforohs[8..]; // We remove the hash.
                            if (VerifyHash(InData, dataforohs[..8]))
                                returnValues = ExecuteLuaScript(LUAJaminCode.LUAJaminDecryptor.Replace("PUT_FORMATEDJAMINVALUE_HERE", ToLiteral(InData)));
                        }
                        else
                            returnValues = ExecuteLuaScript(LUAJaminCode.LUAJaminDecryptor.Replace("PUT_FORMATEDJAMINVALUE_HERE", ToLiteral(dataforohs)));

                        if (!string.IsNullOrEmpty(returnValues?[0].ToString()))
                        {
                            string? endvalue = returnValues[0].ToString();
#if DEBUG
                            LoggerAccessor.LogInfo($"[OHS] - JaminDeFormat De-Assembled Data : {endvalue}");
#endif
                            return endvalue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"[OHS] - JaminDeFormat function JaminDeFormat failed - {ex}");
            }

            return null;
        }

        public static string? JaminFormat(string dataforohs, int game)
        {
            try
            {
#if DEBUG
                LoggerAccessor.LogInfo($"[OHS] - JaminFormat Input Data: {dataforohs}");
#endif
                // Execute the Lua script and get the result
                object[] returnValues = ExecuteLuaScript(LUAJaminCode.LUAJaminEncryptor.Replace("PUT_TABLEINPUT_HERE", dataforohs));

                string? LuaReturn = returnValues[0].ToString();

                if (!string.IsNullOrEmpty(LuaReturn))
                {
#if DEBUG
                    LoggerAccessor.LogInfo($"[OHS] - JaminFormat Assembled Data : {LuaReturn}");
#endif
                    if (game != 0)
                        return EncryptDecrypt.Encrypt(LuaReturn, new Random().Next(1, 95 * 95), game);
                    else
                        return LuaReturn;
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"[OHS] - JaminFormat function JaminFormat failed - {ex}");
            }

            return null;
        }

        // Function to convert a JToken to a Lua table-like string
        public static string ConvertJTokenToLuaTable(JToken token, bool nested, string? propertyName = null)
        {
            int arrayIndex = 1;

            if (nested)
            {
                if (token.Type == JTokenType.Object)
                {
                    StringBuilder resultBuilder = new StringBuilder("{ ");

                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        if (property.Value.Type == JTokenType.String)
                            // Handle string type
                            resultBuilder.Append($"[\"{property.Name}\"] = \"{property.Value}\", ");
                        else if (property.Value.Type == JTokenType.Integer)
                            // Handle integer type
                            resultBuilder.Append($"[\"{property.Name}\"] = {property.Value}, ");
                        else if (property.Value.Type == JTokenType.Array)
                        {
                            // Handle array type
                            resultBuilder.Append($"[\"{property.Name}\"] = {{ ");
                            foreach (JToken arrayItem in property.Value)
                            {
                                resultBuilder.Append($"{ConvertJTokenToLuaTable(arrayItem, true)}");
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            // Handle nested object type
                            resultBuilder.Append($"[\"{property.Name}\"] = {ConvertJTokenToLuaTable(property.Value, true, property.Name)}, ");
                        else
                            resultBuilder.Append($"[\"{property.Name}\"] = {ConvertJTokenToLuaTable(property.Value, true)}, ");
                    }

                    if (resultBuilder.Length > 2)
                        resultBuilder.Length -= 2;

                    resultBuilder.Append(" }");

                    return resultBuilder.ToString();
                }
                else if (token.Type == JTokenType.String)
                    return $"\"{token.Value<string>()}\"";
                else
                    return token.ToString(); // For other value types, use their raw string representation
            }
            else
            {
                if (token.Type == JTokenType.Object)
                {
                    StringBuilder resultBuilder = new StringBuilder();

                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        if (property.Value.Type == JTokenType.String)
                            // Handle string type
                            resultBuilder.Append($"\"{property.Value}\", ");
                        else if (property.Value.Type == JTokenType.Integer)
                            // Handle integer type
                            resultBuilder.Append($"{property.Value}, ");
                        else if (property.Value.Type == JTokenType.Array)
                        {
                            // Handle array type
                            resultBuilder.Append($"{{ ");
                            foreach (JToken arrayItem in property.Value)
                            {
                                resultBuilder.Append($"{ConvertJTokenToLuaTable(arrayItem, true)}");
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            // Handle nested object type
                            resultBuilder.Append($"{ConvertJTokenToLuaTable(property.Value, true, property.Name)}, ");
                        else
                            resultBuilder.Append($"{ConvertJTokenToLuaTable(property.Value, true)}, ");
                    }

                    if (resultBuilder.Length > 2)
                        resultBuilder.Length -= 2;

                    return resultBuilder.ToString();
                }
                else if (token.Type == JTokenType.String)
                    return $"\"{token.Value<string>()}\"";
                else
                    return token.ToString(); // For other value types, use their raw string representation
            }
        }

        public static string ConvertJObjectStringToLuaTable(JObject jsonObj)
        {
            string luaTableString = "{";
            foreach (JProperty? property in jsonObj.Properties())
            {
                luaTableString += $"[\"{property.Name}\"] = {JsonValueToLuaValue(property.Value)}, ";
            }
            // Remove the trailing comma and space
            if (jsonObj.Properties().Any())
                luaTableString = luaTableString.Remove(luaTableString.Length - 2);
            luaTableString += "}";
            return luaTableString;
        }

        public static string JsonValueToLuaValue(JToken token)
        {
            return token.Type switch
            {
                JTokenType.String => $"\"{token}\"",
                JTokenType.Object => ConvertJObjectStringToLuaTable(JObject.Parse(token.ToString())),
                JTokenType.Array => ConvertJTokenToLuaTable(token, false),
                _ => token.ToString().ToLower(),
            };
        }

        public static object[] ExecuteLuaScript(string luaScript)
        {
            object[]? returnValues = null;

            using (Lua lua = new Lua())
            {
                try
                {
                    // Execute the Lua script
                    returnValues = lua.DoString(luaScript);

                    // If the script returns no values, return an empty object array
                    if (returnValues == null || returnValues.Length == 0)
                        returnValues = Array.Empty<object>();
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during script execution
                    LoggerAccessor.LogError("[ExecuteLuaScript] - Error executing Lua script: " + ex);
                    returnValues = Array.Empty<object>();
                }

                lua.Close();
            }

            return returnValues;
        }

        private static string ToLiteral(string input)
        {
            StringBuilder literal = new StringBuilder(input.Length + 2);
            literal.Append('"');
            foreach (char c in input)
            {
                switch (c)
                {
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                            literal.Append(c);
                            // As UTF16 escaped character
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            literal.Append('"');
            return literal.ToString();
        }
    }
}
