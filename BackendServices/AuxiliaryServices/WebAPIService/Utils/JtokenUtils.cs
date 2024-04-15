using Newtonsoft.Json.Linq;

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
        public static object? GetValueFromJToken(JToken jToken, string propertyName)
        {
            try
            {
                JToken? valueToken = jToken[propertyName];

                if (valueToken != null)
                {
                    if (valueToken.Type == JTokenType.Object || valueToken.Type == JTokenType.Array)
                        return valueToken.ToObject<object>();
                    else if (valueToken.Type == JTokenType.Integer)
                        return valueToken.ToObject<int>();
                    else if (valueToken.Type == JTokenType.String)
                        return valueToken.ToObject<string>();
                    else if (valueToken.Type == JTokenType.Boolean)
                        return valueToken.ToObject<bool>();
                    else if (valueToken.Type == JTokenType.Float)
                        return valueToken.ToObject<float>();
                }
            }
            catch
            {
                // Not Important.
            }

            return null;
        }
    }
}
