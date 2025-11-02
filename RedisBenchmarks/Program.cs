
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MessagePack;
using Redis.Data;
using Redis.RedisService;
using Redis.Serializer;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;
[MemoryDiagnoser]
[RankColumn]
public class RedisBenchmarks
{
    private IConnectionMultiplexer _redis;
    private List<ComplexEntity> _testData;
    private const int EntityCount = 1000;

    [GlobalSetup]
    public void Setup()
    {
        _redis = ConnectionMultiplexer.Connect("localhost:6379");
        _testData = GenerateTestData(EntityCount);

        // Warm up Redis
        var db = _redis.GetDatabase();
        db.StringSet("warmup", "test");
        db.StringGet("warmup");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _redis?.Dispose();
    }

    // JSON Benchmarks
    [Benchmark]
    public async Task Json_Individual()
    {
        var service = new RedisService(_redis, new JsonRedisSerializer());
        await service.SaveIndividualAsync(_testData);
    }

    [Benchmark]
    public async Task Json_BatchWithAwait()
    {
        var service = new RedisService(_redis, new JsonRedisSerializer());
        await service.SaveBatchWithAwaitAsync(_testData);
    }

    [Benchmark]
    public void Json_FireAndForget()
    {
        var service = new RedisService(_redis, new JsonRedisSerializer());
        service.SaveBatchFireAndForget(_testData);
    }

    [Benchmark]
    public async Task Json_ChunkedBatch()
    {
        var service = new RedisService(_redis, new JsonRedisSerializer());
        await service.SaveChunkedBatchAsync(_testData, 500);
    }

    // MessagePack Benchmarks
    [Benchmark]
    public async Task MessagePack_Individual()
    {
        var service = new RedisService(_redis, new MessagePackRedisSerializer());
        await service.SaveIndividualAsync(_testData);
    }

    [Benchmark]
    public async Task MessagePack_BatchWithAwait()
    {
        var service = new RedisService(_redis, new MessagePackRedisSerializer());
        await service.SaveBatchWithAwaitAsync(_testData);
    }

    [Benchmark]
    public void MessagePack_FireAndForget()
    {
        var service = new RedisService(_redis, new MessagePackRedisSerializer());
        service.SaveBatchFireAndForget(_testData);
    }

    [Benchmark]
    public async Task MessagePack_ChunkedBatch()
    {
        var service = new RedisService(_redis, new MessagePackRedisSerializer());
        await service.SaveChunkedBatchAsync(_testData, 500);
    }

    // Protobuf Benchmarks
    [Benchmark]
    public async Task Protobuf_Individual()
    {
        var service = new RedisService(_redis, new ProtobufRedisSerializer());
        await service.SaveIndividualAsync(_testData);
    }

    [Benchmark]
    public async Task Protobuf_BatchWithAwait()
    {
        var service = new RedisService(_redis, new ProtobufRedisSerializer());
        await service.SaveBatchWithAwaitAsync(_testData);
    }

    [Benchmark]
    public void Protobuf_FireAndForget()
    {
        var service = new RedisService(_redis, new ProtobufRedisSerializer());
        service.SaveBatchFireAndForget(_testData);
    }

    [Benchmark]
    public async Task Protobuf_ChunkedBatch()
    {
        var service = new RedisService(_redis, new ProtobufRedisSerializer());
        await service.SaveChunkedBatchAsync(_testData, 500);
    }

    private List<ComplexEntity> GenerateTestData(int count)
    {
        var entities = new List<ComplexEntity>();
        var random = new Random(42); // Fixed seed for consistency

        for (int i = 0; i < count; i++)
        {
            entities.Add(new ComplexEntity
            {
                Id = i,
                Name = $"Entity_{i}",
                Email = $"entity{i}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365)),
                Data = new NestedData
                {
                    Field1 = $"Field1_Value_{i}",
                    Field2 = $"Field2_Value_{i}",
                    Field3 = random.Next(1000),
                    Field4 = random.NextDouble() * 1000,
                    Numbers = Enumerable.Range(0, 10).Select(_ => random.Next(100)).ToList()
                },
                Tags = new List<string> { "tag1", "tag2", "tag3", $"tag_{i}" },
                Metadata = new Dictionary<string, int>
                    {
                        { "views", random.Next(10000) },
                        { "likes", random.Next(1000) },
                        { "shares", random.Next(100) }
                    }
            });
        }

        return entities;
    }
}

// ==================== MANUAL COMPARISON ====================

public class ManualBenchmark
{
    public static async Task RunComparison()
    {
        Console.WriteLine("=== Redis Serialization & Batching Comparison ===\n");

        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        var testData = GenerateTestData(1000);

        var serializers = new List<IRedisSerializer>
            {
                new JsonRedisSerializer(),
                new MessagePackRedisSerializer(),
                new ProtobufRedisSerializer()
            };

        Console.WriteLine("Test Configuration:");
        Console.WriteLine($"- Entity Count: {testData.Count}");
        Console.WriteLine($"- Entity Structure: ComplexEntity with nested data, lists, dictionaries");
        Console.WriteLine();

        // Size comparison
        Console.WriteLine("=== Serialization Size Comparison ===");
        var sampleEntity = testData[0];
        foreach (var serializer in serializers)
        {
            var data = serializer.Serialize(sampleEntity);
            Console.WriteLine($"{serializer.Name,-20}: {data.Length,6} bytes");
        }
        Console.WriteLine();

        // Speed comparison
        foreach (var serializer in serializers)
        {
            Console.WriteLine($"=== {serializer.Name} ===");
            var service = new RedisService(redis, serializer);

            // Individual
            var sw = Stopwatch.StartNew();
            await service.SaveIndividualAsync(testData.Take(100).ToList());
            sw.Stop();
            Console.WriteLine($"Individual (100):        {sw.ElapsedMilliseconds,6} ms");

            // Batch with await
            sw.Restart();
            await service.SaveBatchWithAwaitAsync(testData);
            sw.Stop();
            Console.WriteLine($"Batch + Await (1000):    {sw.ElapsedMilliseconds,6} ms");

            // Fire and forget
            sw.Restart();
            service.SaveBatchFireAndForget(testData);
            sw.Stop();
            Console.WriteLine($"Fire & Forget (1000):    {sw.ElapsedMilliseconds,6} ms");

            // Chunked
            sw.Restart();
            await service.SaveChunkedBatchAsync(testData, 500);
            sw.Stop();
            Console.WriteLine($"Chunked Batch (1000):    {sw.ElapsedMilliseconds,6} ms");

            Console.WriteLine();
        }

        redis.Dispose();
    }

    private static List<ComplexEntity> GenerateTestData(int count)
    {
        var entities = new List<ComplexEntity>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            entities.Add(new ComplexEntity
            {
                Id = i,
                Name = $"Entity_{i}",
                Email = $"entity{i}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365)),
                Data = new NestedData
                {
                    Field1 = $"Field1_Value_{i}",
                    Field2 = $"Field2_Value_{i}",
                    Field3 = random.Next(1000),
                    Field4 = random.NextDouble() * 1000,
                    Numbers = Enumerable.Range(0, 10).Select(_ => random.Next(100)).ToList()
                },
                Tags = new List<string> { "tag1", "tag2", "tag3", $"tag_{i}" },
                Metadata = new Dictionary<string, int>
                    {
                        { "views", random.Next(10000) },
                        { "likes", random.Next(1000) },
                        { "shares", random.Next(100) }
                    }
            });
        }

        return entities;
    }
}

// ==================== PROGRAM ====================

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Choose benchmark mode:");
        Console.WriteLine("1. Quick manual comparison (fast)");
        Console.WriteLine("2. Full BenchmarkDotNet analysis (slow but detailed)");
        Console.Write("\nChoice (1 or 2): ");

        var choice = Console.ReadLine();

        if (choice == "1")
        {
            await ManualBenchmark.RunComparison();
        }
        else if (choice == "2")
        {
            Console.WriteLine("\nRunning BenchmarkDotNet (this will take several minutes)...\n");
            BenchmarkRunner.Run<RedisBenchmarks>();
        }
        else
        {
            Console.WriteLine("Invalid choice. Running manual comparison by default.\n");
            await ManualBenchmark.RunComparison();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}