﻿using CustomLogger;
using Newtonsoft.Json.Linq;
using NLua;
using System;
using System.Linq;
using System.Text;

namespace WebAPIService.OHS
{
    public class LuaUtils
    {
        // Function to convert a JToken to a Lua table-like string
        public static string ConvertJTokenToLuaTable(JToken token, bool nested)
        {
            int arrayIndex = 1;

            if (nested)
            {
                if (token.Type == JTokenType.Object)
                {
                    StringBuilder resultBuilder = new StringBuilder("{ ");

                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        if (property.Value.Type == JTokenType.Array)
                        {
                            resultBuilder.Append($"[\"{property.Name}\"] = {{ ");
                            foreach (JToken arrayItem in property.Value)
                            {
                                resultBuilder.Append(ConvertJTokenToLuaTable(arrayItem, true));
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            resultBuilder.Append($"[\"{property.Name}\"] = {ConvertJTokenToLuaTable(property.Value, true)}, ");
                        else if (property.Value.Type == JTokenType.String)
                            resultBuilder.Append($"[\"{property.Name}\"] = \"{property.Value}\", ");
                        else
                            resultBuilder.Append($"[\"{property.Name}\"] = {property.Value}, ");
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
                    StringBuilder resultBuilder = new StringBuilder("{ ");

                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        if (property.Value.Type == JTokenType.Array)
                        {
                            resultBuilder.Append("{ ");
                            foreach (JToken arrayItem in property.Value)
                            {
                                resultBuilder.Append(ConvertJTokenToLuaTable(arrayItem, true));
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            resultBuilder.Append($"{ConvertJTokenToLuaTable(property.Value, true)}, ");
                        else if (property.Value.Type == JTokenType.String)
                            resultBuilder.Append($"\"{property.Value}\", ");
                        else
                            resultBuilder.Append($"{property.Value}, ");
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

        public static string JsonValueToLuaValue(JToken token)
        {
            return token.Type switch
            {
                JTokenType.Object => ConvertJObjectStringToLuaTable(JObject.Parse(token.ToString())),
                JTokenType.Array => ConvertJTokenToLuaTable(token, false),
                JTokenType.String => $"\"{token}\"",
                _ => token.ToString().ToLower(),
            };
        }

        public static string ToLiteral(string input)
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

        private static string ConvertJObjectStringToLuaTable(JObject jsonObj)
        {
            StringBuilder luaTableStringBuilder = new StringBuilder();
            luaTableStringBuilder.Append("{ ");

            foreach (JProperty? property in jsonObj.Properties())
            {
                luaTableStringBuilder.AppendFormat("[\"{0}\"] = {1}, ", property.Name, JsonValueToLuaValue(property.Value));
            }

            // Remove the trailing comma and space if the string builder has more than the initial length
            if (luaTableStringBuilder.Length > 2)
                luaTableStringBuilder.Length -= 2;

            luaTableStringBuilder.Append(" }");
            return luaTableStringBuilder.ToString();
        }
    }
}