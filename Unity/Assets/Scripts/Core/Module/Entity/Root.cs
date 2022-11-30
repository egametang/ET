using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ET
{
    // 管理根部的Scene
    public class Root: Singleton<Root>, ISingletonAwake
    {
        // 管理所有的Entity
        private readonly Dictionary<long, Entity> allEntities = new();
        
        public Scene Scene { get; private set; }

        public void Awake()
        {
            this.Scene = EntitySceneFactory.CreateScene(0, SceneType.Process, "Process");
        }

        public override void Dispose()
        {
            this.Scene.Dispose();
        }

        public void Add(Entity entity)
        {
            this.allEntities.Add(entity.InstanceId, entity);
        }
        
        public void Remove(long instanceId)
        {
            this.allEntities.Remove(instanceId);
        }

        public Entity Get(long instanceId)
        {
            Entity component = null;
            this.allEntities.TryGetValue(instanceId, out component);
            return component;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new();
            HashSet<Type> noParent = new HashSet<Type>();
            Dictionary<Type, int> typeCount = new Dictionary<Type, int>();

            HashSet<Type> noDomain = new HashSet<Type>();

            foreach (var kv in this.allEntities)
            {
                Type type = kv.Value.GetType();
                if (kv.Value.Parent == null)
                {
                    noParent.Add(type);
                }

                if (kv.Value.Domain == null)
                {
                    noDomain.Add(type);
                }

                if (typeCount.ContainsKey(type))
                {
                    typeCount[type]++;
                }
                else
                {
                    typeCount[type] = 1;
                }
            }

            sb.AppendLine("not set parent type: ");
            foreach (Type type in noParent)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            sb.AppendLine("not set domain type: ");
            foreach (Type type in noDomain)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            IOrderedEnumerable<KeyValuePair<Type, int>> orderByDescending = typeCount.OrderByDescending(s => s.Value);

            sb.AppendLine("Entity Count: ");
            foreach (var kv in orderByDescending)
            {
                if (kv.Value == 1)
                {
                    continue;
                }

                sb.AppendLine($"\t{kv.Key.Name}: {kv.Value}");
            }

            return sb.ToString();
        }
    }
}