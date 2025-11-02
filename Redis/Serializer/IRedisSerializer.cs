using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.Serializer
{
    public interface IRedisSerializer
    {

        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] data);
        string Name { get; }
    }
}
