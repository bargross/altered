using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
