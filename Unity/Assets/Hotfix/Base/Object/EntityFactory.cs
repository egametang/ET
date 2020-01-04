using System;
using ETModel;

namespace ETHotfix
{
	public static class EntityFactory
	{
		public static Entity CreateWithParent(Type type, Entity parent)
		{
			Entity component = Game.ObjectPool.Fetch(type);
			component.Domain = parent.Domain;
			component.Id = component.InstanceId;
			component.Parent = parent;
			
			Game.EventSystem.Awake(component);
			return component;
		}

		public static T CreateWithParent<T>(Entity parent) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = parent.Domain;
			component.Id = component.InstanceId;
			component.Parent = parent;
			
			Game.EventSystem.Awake(component);
			return component;
		}

		public static T CreateWithParent<T, A>(Entity parent, A a) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = parent.Domain;
			component.Id = component.InstanceId;
			component.Parent = parent;
			
			Game.EventSystem.Awake(component, a);
			return component;
		}

		public static T CreateWithParent<T, A, B>(Entity parent, A a, B b) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = parent.Domain;
			component.Id = component.InstanceId;
			component.Parent = parent;
			
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

		public static T CreateWithParent<T, A, B, C>(Entity parent, A a, B b, C c, bool fromPool = true) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = parent.Domain;
			component.Id = component.InstanceId;
			component.Parent = parent;
			
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}

        public static T CreateWithParent<T, A, B, C, D>(Entity parent, A a, B b, C c, D d, bool fromPool = true) where T : Entity
        {
            Type type = typeof(T);

            T component = (T)Game.ObjectPool.Fetch(type);
            component.Domain = parent.Domain;
            component.Id = component.InstanceId;
            component.Parent = parent;
            
            Game.EventSystem.Awake(component, a, b, c, d);
            return component;
        }
        
        
        public static Entity Create(Entity domain, Type type)
        {
	        Entity component = Game.ObjectPool.Fetch(type);
	        component.Domain = domain ?? component;
	        component.Id = component.InstanceId;
	        
	        Game.EventSystem.Awake(component);
	        return component;
        }


        public static T Create<T>(Entity domain) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = component.InstanceId;
			Game.EventSystem.Awake(component);
			return component;
		}

		public static T Create<T, A>(Entity domain, A a) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = component.InstanceId;
			Game.EventSystem.Awake(component, a);
			return component;
		}

		public static T Create<T, A, B>(Entity domain, A a, B b) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = component.InstanceId;
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

		public static T Create<T, A, B, C>(Entity domain, A a, B b, C c) where T : Entity
		{
			Type type = typeof (T);

			T component = (T) Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = component.InstanceId;
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}

		public static T CreateWithId<T>(Entity domain, long id) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = id;
			Game.EventSystem.Awake(component);
			return component;
		}

		public static T CreateWithId<T, A>(Entity domain, long id, A a) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = id;
			Game.EventSystem.Awake(component, a);
			return component;
		}

		public static T CreateWithId<T, A, B>(Entity domain, long id, A a, B b) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = id;
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

		public static T CreateWithId<T, A, B, C>(Entity domain, long id, A a, B b, C c) where T : Entity
		{
			Type type = typeof (T);
			
			T component = (T)Game.ObjectPool.Fetch(type);
			component.Domain = domain ?? component;
			component.Id = id;
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}
		
		public static Scene CreateScene(SceneType sceneType, string name, Scene parent = null,long id = 0)
		{
			Scene scene = (Scene)Game.ObjectPool.Fetch(typeof(Scene));
			scene.Id = id != 0 ? id : IdGenerater.GenerateId();
			scene.Name = name;
			scene.SceneType = sceneType;
			if (parent != null)
			{
				scene.Parent = parent;
			}
			scene.Domain = scene;
			return scene;
		}
	}
}
