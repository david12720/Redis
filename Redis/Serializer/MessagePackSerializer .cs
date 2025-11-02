using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using ProtoBuf;

namespace Redis.Serializer
{
    public class MessagePackRedisSerializer : IRedisSerializer
    {
        public string Name => "MessagePack";

        public byte[] Serialize<T>(T obj)
        {
            return global::MessagePack.MessagePackSerializer.Serialize(obj);
        }

        public T Deserialize<T>(byte[] data)
        {
            return global::MessagePack.MessagePackSerializer.Deserialize<T>(data);
        }
    }
}
