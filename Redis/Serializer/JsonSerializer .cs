using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.Serializer
{
    public class JsonRedisSerializer : IRedisSerializer
    {
        public string Name => "System.Text.Json";

        public byte[] Serialize<T>(T obj)
        {
            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        public T Deserialize<T>(byte[] data)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(data);
        }
    }
}
