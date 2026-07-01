using System.Text.Json;

namespace Altered.Tests
{
    public static class TestHelpers
    {
        public static T? GetValue<T>(this object? obj)
        {
            return obj switch
            {
                null => default,
                JsonElement element => element.Deserialize<T>(),
                T typed => typed,
                _ => (T)Convert.ChangeType(obj, typeof(T))
            };
        }
    }
}
