using System.IO;
using System;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Text;
using NLua;
using System.Linq;

namespace WebAPIService.LuaUtils
{
    public class LuaProcessor
    {
        // Function to convert a JToken to a Lua table-like string
        public static string ConvertToLuaTable(JToken token, bool nested, string? propertyName = null)
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
                                resultBuilder.Append($"{ConvertToLuaTable(arrayItem, true)}");
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            // Handle nested object type
                            resultBuilder.Append($"[\"{property.Name}\"] = {ConvertToLuaTable(property.Value, true, property.Name)}, ");
                        else
                            resultBuilder.Append($"[\"{property.Name}\"] = {ConvertToLuaTable(property.Value, true)}, ");
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
                                resultBuilder.Append($"{ConvertToLuaTable(arrayItem, true)}");
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            // Handle nested object type
                            resultBuilder.Append($"{ConvertToLuaTable(property.Value, true, property.Name)}, ");
                        else
                            resultBuilder.Append($"{ConvertToLuaTable(property.Value, true)}, ");
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

        // Function to execute a lua script contained within a string.
        public static object[] ExecuteLuaScriptString(string luaScript)
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
                    LoggerAccessor.LogError("[ExecuteLuaScriptString] - Error executing Lua script: " + ex);
                    returnValues = Array.Empty<object>();
                }

                lua.Close();
            }

            return returnValues;
        }

        // Function to execute a lua script stroed in a given file.
        public static object[] ExecuteLuaScriptFile(string luaPath)
        {
            if (!File.Exists(luaPath))
                return Array.Empty<object>();

            object[]? returnValues = null;

            using (Lua lua = new Lua())
            {
                try
                {
                    // Execute the Lua script
                    returnValues = lua.DoString(File.ReadAllText(luaPath));

                    // If the script returns no values, return an empty object array
                    if (returnValues == null || returnValues.Length == 0)
                        returnValues = Array.Empty<object>();
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during script execution
                    LoggerAccessor.LogError("[ExecuteLuaScriptFile] - Error executing Lua script: " + ex);
                    returnValues = Array.Empty<object>();
                }

                lua.Close();
            }

            return returnValues;
        }
    }
}
