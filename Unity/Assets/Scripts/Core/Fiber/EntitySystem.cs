using System;
using System.Collections.Generic;

namespace ET
{
    public class EntitySystem
    {
        private readonly Queue<EntityRef<Entity>>[] queues = new Queue<EntityRef<Entity>>[InstanceQueueIndex.Max];
        
        public EntitySystem()
        {
            for (int i = 0; i < this.queues.Length; i++)
            {
                this.queues[i] = new Queue<EntityRef<Entity>>();
            }
        }
        
        
        public virtual void RegisterSystem(Entity component)
        {
            Type type = component.GetType();

            TypeSystems.OneTypeSystems oneTypeSystems = EntitySystemSingleton.Instance.TypeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }
            for (int i = 0; i < oneTypeSystems.QueueFlag.Length; ++i)
            {
                if (!oneTypeSystems.QueueFlag[i])
                {
                    continue;
                }
                this.queues[i].Enqueue(component);
            }
        }

        public void Update()
        {
            Queue<EntityRef<Entity>> queue = this.queues[InstanceQueueIndex.Update];
            int count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }
                
                if (component is not IUpdate)
                {
                    continue;
                }

                try
                {
                    List<SystemObject> iUpdateSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IUpdateSystem));
                    if (iUpdateSystems == null)
                    {
                        continue;
                    }

                    queue.Enqueue(component);

                    foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
                    {
                        try
                        {
                            iUpdateSystem.Run(component);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"entity system update fail: {component.GetType().FullName}", e);
                }

            }
        }

        public void LateUpdate()
        {
            Queue<EntityRef<Entity>> queue = this.queues[InstanceQueueIndex.LateUpdate];
            int count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }
                
                if (component is not ILateUpdate)
                {
                    continue;
                }

                List<SystemObject> iLateUpdateSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (ILateUpdateSystem));
                if (iLateUpdateSystems == null)
                {
                    continue;
                }

                queue.Enqueue(component);

                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
                {
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }
    }
}