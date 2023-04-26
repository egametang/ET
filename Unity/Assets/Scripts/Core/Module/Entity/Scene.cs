using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using MemoryPack;

namespace ET
{
    [MemoryPackable]
    public partial struct Aa
    {
        public int A;
        public int A2;
    }

    [MemoryPackable]
    public partial struct Bb
    {
        public int B;
    }

    [MemoryPackable]
    public partial class Cc
    {
        public Aa aaa;
        
        public List<Bb> bbb;
        
        public Dictionary<long, Bb> ccc;
    }

    public class FixedArrayBufferWriter: IBufferWriter<byte>
    {
        byte[] buffer;
        int written;

        public FixedArrayBufferWriter(byte[] buffer)
        {
            this.buffer = buffer;
            this.written = 0;
        }

        public void Reset(byte[] buffer)
        {
            this.buffer = buffer;
            this.written = 0;
        }

        public void Advance(int count)
        {
            this.written += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var memory = buffer.AsMemory(written);
            if (memory.Length >= sizeHint)
            {
                return memory;
            }

            MemoryPackSerializationException.ThrowMessage("Requested invalid sizeHint.");
            return memory;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            var span = buffer.AsSpan(written);
            if (span.Length >= sizeHint)
            {
                return span;
            }

            MemoryPackSerializationException.ThrowMessage("Requested invalid sizeHint.");
            return span;
        }

        public byte[] GetFilledBuffer()
        {
            if (written != buffer.Length)
            {
                MemoryPackSerializationException.ThrowMessage("Not filled buffer.");
            }

            return buffer;
        }
    }

    [MemoryPackable]
    public partial struct Dd
    {
        public int aaa;
        public string bbb;
        public List<int> ccc;
    }
    
    [EnableMethod]
    [ChildOf]
    public class Scene: Entity, IScene
    {
        public int Zone
        {
            get;
        }

        public SceneType SceneType
        {
            get;
            set;
        }

        public string Name
        {
            get;
        }

        public Scene()
        {
        }

        public Scene(long id, long instanceId, int zone, SceneType sceneType, string name)
        {
            this.Id = id;
            this.InstanceId = instanceId;
            this.Zone = zone;
            this.SceneType = sceneType;
            this.Name = name;
            this.IsCreated = true;
            this.IsNew = true;
            this.IsRegister = true;
            this.domain = this;
            Log.Info($"scene create: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            Log.Info($"scene dispose: {this.SceneType} {this.Name} {this.Id} {this.InstanceId} {this.Zone}");
        }
        
        protected override string ViewName
        {
            get
            {
                return $"{this.GetType().Name} ({this.SceneType})";
            }
        }
    }
}