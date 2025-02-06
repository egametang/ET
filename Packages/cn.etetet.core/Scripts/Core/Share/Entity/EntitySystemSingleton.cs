using System;
using System.Collections.Generic;

namespace ET
{
    [CodeProcess]
    public class EntitySystemSingleton: Singleton<EntitySystemSingleton>, ISingletonAwake
    {
        public TypeSystems TypeSystems { get; private set; }
        
        public void Awake()
        {
            this.TypeSystems = new TypeSystems();

            foreach (Type type in CodeTypes.Instance.GetTypes(typeof (EntitySystemAttribute)))
            {
                SystemObject obj = (SystemObject)Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    TypeSystems.OneTypeSystems oneTypeSystems = this.TypeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);
                    
                    if (iSystemType is IClassEventSystem)
                    {
                        oneTypeSystems.ClassType.Add(iSystemType.SystemType());
                    }
                }
            }
        }
        
        public void Serialize(Entity component)
        {
            if (component is not ISerialize)
            {
                return;
            }
            
            List<SystemObject> iSerializeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (ISerializeSystem));
            if (iSerializeSystems == null)
            {
                return;
            }

            foreach (ISerializeSystem serializeSystem in iSerializeSystems)
            {
                if (serializeSystem == null)
                {
                    continue;
                }

                try
                {
                    serializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        public void Deserialize(Entity component)
        {
            if (component is not IDeserialize)
            {
                return;
            }
            
            List<SystemObject> iDeserializeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IDeserializeSystem));
            if (iDeserializeSystems == null)
            {
                return;
            }

            foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
            {
                if (deserializeSystem == null)
                {
                    continue;
                }

                try
                {
                    deserializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        // GetComponentSystem
        public void GetComponentSys(Entity entity, Type type)
        {
            List<SystemObject> iGetSystem = this.TypeSystems.GetSystems(entity.GetType(), typeof (IGetComponentSysSystem));
            if (iGetSystem == null)
            {
                return;
            }

            foreach (IGetComponentSysSystem getSystem in iGetSystem)
            {
                if (getSystem == null)
                {
                    continue;
                }

                try
                {
                    getSystem.Run(entity, type);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        public void Awake(Entity component)
        {
            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1>(Entity component, P1 p1)
        {
            if (component is not IAwake<P1>)
            {
                return;
            }
            
            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            if (component is not IAwake<P1, P2>)
            {
                return;
            }
            
            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            if (component is not IAwake<P1, P2, P3>)
            {
                return;
            }
            
            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Destroy(Entity component)
        {
            if (component is not IDestroy)
            {
                return;
            }
            
            List<SystemObject> iDestroySystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IDestroySystem));
            if (iDestroySystems == null)
            {
                return;
            }

            foreach (IDestroySystem iDestroySystem in iDestroySystems)
            {
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}