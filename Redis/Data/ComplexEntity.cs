using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.Data
{
    
        [MessagePackObject]
        [ProtoContract]
        public class ComplexEntity
        {
            [Key(0)]
            [ProtoMember(1)]
            public int Id { get; set; }

            [Key(1)]
            [ProtoMember(2)]
            public string Name { get; set; }

            [Key(2)]
            [ProtoMember(3)]
            public string Email { get; set; }

            [Key(3)]
            [ProtoMember(4)]
            public DateTime CreatedAt { get; set; }

            [Key(4)]
            [ProtoMember(5)]
            public NestedData Data { get; set; }

            [Key(5)]
            [ProtoMember(6)]
            public List<string> Tags { get; set; }

            [Key(6)]
            [ProtoMember(7)]
            public Dictionary<string, int> Metadata { get; set; }
        }

        [MessagePackObject]
        [ProtoContract]
        public class NestedData
        {
            [Key(0)]
            [ProtoMember(1)]
            public string Field1 { get; set; }

            [Key(1)]
            [ProtoMember(2)]
            public string Field2 { get; set; }

            [Key(2)]
            [ProtoMember(3)]
            public int Field3 { get; set; }

            [Key(3)]
            [ProtoMember(4)]
            public double Field4 { get; set; }

            [Key(4)]
            [ProtoMember(5)]
            public List<int> Numbers { get; set; }
        }
    }

