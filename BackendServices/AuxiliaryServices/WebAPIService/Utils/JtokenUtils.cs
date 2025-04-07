using Newtonsoft.Json.Linq;
using System;

namespace WebAPIService.Utils
{
    public static class JtokenUtils
    {
        /// <summary>
        /// Parse a newtonsoft json JToken.
        /// <para>Parcours un Jtoken issue de la librairie newtonsoft json.</para>
        /// </summary>
        /// <param name="jToken">A newtonsoft json jtoken.</param>
        /// <param name="propertyName">the property to parse in the jtoken.</param>
        /// <returns>A complex object.</returns>
        public static object GetValueFromJToken(JToken jToken, string propertyName)
        {
            try
            {
                JToken valueToken = jToken[propertyName];

                if (valueToken != null)
                {
                    switch (valueToken.Type)
                    {
                        case JTokenType.Object:
                        case JTokenType.Array:
                            return valueToken.ToObject<object>();
                        case JTokenType.Integer:
                            return valueToken.ToObject<int>();
                        case JTokenType.String:
                            return valueToken.ToObject<string>();
                        case JTokenType.Boolean:
                            return valueToken.ToObject<bool>();
                        case JTokenType.Float:
                            return valueToken.ToObject<float>();
                        case JTokenType.Null:
                            return null;
                        case JTokenType.Date:
                            return valueToken.ToObject<DateTime>();
                        case JTokenType.Bytes:
                            return valueToken.ToObject<byte[]>();
                        case JTokenType.Guid:
                            return valueToken.ToObject<Guid>();
                        case JTokenType.Uri:
                            return valueToken.ToObject<Uri>();
                        case JTokenType.TimeSpan:
                            return valueToken.ToObject<TimeSpan>();
                        default:
                            {
                                CustomLogger.LoggerAccessor.LogWarn($"[JtokenUtils] - GetValueFromJToken - Unsupported JTokenType: {valueToken.Type}");
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[JtokenUtils] - GetValueFromJToken - thrown an exception: {ex}");
            }

            return null;
        }
    }
}
