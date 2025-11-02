using Redis.Data;
using Redis.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using ProtoBuf;
using StackExchange.Redis;

namespace Redis.RedisService
{
    public class RedisService
    {
        private readonly IDatabase _db;
        private readonly IRedisSerializer _serializer;

        public RedisService(StackExchange.Redis.IConnectionMultiplexer redis, IRedisSerializer serializer)
        {
            _db = redis.GetDatabase();
            _serializer = serializer;
        }

        // Method 1: Individual writes (no batching)
        public async Task SaveIndividualAsync(List<ComplexEntity> entities)
        {
            foreach (var entity in entities)
            {
                var key = $"entity:{entity.Id}";
                var value = _serializer.Serialize(entity);
                await _db.StringSetAsync(key, value);
            }
        }

        // Method 2: Batch with await
        public async Task SaveBatchWithAwaitAsync(List<ComplexEntity> entities)
        {
            var batch = _db.CreateBatch();
            var tasks = new List<Task>();

            foreach (var entity in entities)
            {
                var key = $"entity:{entity.Id}";
                var value = _serializer.Serialize(entity);
                tasks.Add(batch.StringSetAsync(key, value));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        // Method 3: Batch with fire-and-forget
        public void SaveBatchFireAndForget(List<ComplexEntity> entities)
        {
            var batch = _db.CreateBatch();

            foreach (var entity in entities)
            {
                var key = $"entity:{entity.Id}";
                var value = _serializer.Serialize(entity);
                batch.StringSetAsync(key, value, flags: CommandFlags.FireAndForget);
            }

            batch.Execute();
        }

        // Method 4: Batched batches (chunked)
        public async Task SaveChunkedBatchAsync(List<ComplexEntity> entities, int chunkSize = 500)
        {
            for (int i = 0; i < entities.Count; i += chunkSize)
            {
                var chunk = entities.Skip(i).Take(chunkSize).ToList();
                var batch = _db.CreateBatch();
                var tasks = new List<Task>();

                foreach (var entity in chunk)
                {
                    var key = $"entity:{entity.Id}";
                    var value = _serializer.Serialize(entity);
                    tasks.Add(batch.StringSetAsync(key, value));
                }

                batch.Execute();
                await Task.WhenAll(tasks);
            }
        }

        // Read method for completeness
        public async Task<ComplexEntity> GetAsync(int id)
        {
            var key = $"entity:{id}";
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return null;

            return _serializer.Deserialize<ComplexEntity>(value);
        }
    }
}
