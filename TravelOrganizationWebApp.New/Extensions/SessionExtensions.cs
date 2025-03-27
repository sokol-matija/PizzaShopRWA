using System.Text.Json;

namespace Microsoft.AspNetCore.Http
{
    public static class SessionExtensions
    {
        // No need to redefine these, they already exist in ISession
        // Commented out to avoid name conflicts and recursion
        /*
        public static void SetString(this ISession session, string key, string value)
        {
            session.SetString(key, value);
        }

        public static void SetInt32(this ISession session, string key, int value)
        {
            session.SetInt32(key, value);
        }
        */

        public static void SetObject<T>(this ISession session, string key, T value)
        {
            if (value == null)
            {
                session.Remove(key);
                return;
            }

            string json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            string? json = session.GetString(key);
            
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }
    }
} 