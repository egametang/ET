using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using TrueSync;

namespace ET
{
    public static class LSWorldSystem
    {
        public static LSWorld LSWorld(this LSEntity entity)
        {
            return entity.Domain as LSWorld;
        }

        public static long GetId(this LSEntity entity)
        {
            return entity.LSWorld().GetId();
        }
        
        public static TSRandom GetRandom(this LSEntity entity)
        {
            return entity.LSWorld().Random;
        }
    }

    [EnableMethod]
    [ChildOf]
    [ComponentOf]
    public class LSWorld: LSEntity, IAwake, IScene
    {
        public LSWorld()
        {
        }
        
        public LSWorld(SceneType sceneType)
        {
            this.Id = this.GetId();

            this.SceneType = sceneType;
            
            Log.Info($"LSScene create: {this.Id} {this.InstanceId}");
        }

        private readonly LSUpdater updater = new();

        [BsonElement]
        private long idGenerator;

        public long GetId()
        {
            return ++this.idGenerator;
        }

        public TSRandom Random { get; set; }
        
        public SceneType SceneType { get; set; }
        
        public int Frame { get; set; }

        public long TimeNow()
        {
            return LSConstValue.UpdateInterval * this.Frame;
        }

        public void AddToUpdater(LSEntity lsEntity)
        {
            this.updater.Add(lsEntity);
        }

        public void Update()
        {
            this.updater.Update();
            ++this.Frame;
        }
    }
}