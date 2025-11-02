using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using ProtoBuf;

namespace Redis.Serializer
{
    public class ProtobufRedisSerializer : IRedisSerializer
    {
        public string Name => "Protobuf-net";

        public byte[] Serialize<T>(T obj)
        {
            using var ms = new System.IO.MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, obj);
            return ms.ToArray();
        }

        public T Deserialize<T>(byte[] data)
        {
            using var ms = new System.IO.MemoryStream(data);
            return ProtoBuf.Serializer.Deserialize<T>(ms);
        }
    }
}
