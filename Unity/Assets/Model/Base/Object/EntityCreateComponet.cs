﻿using System;

namespace ETModel
{
	public partial class Entity
	{
		private Entity CreateWithComponentParent(Type type)
		{
			Entity component;
			if (type.IsDefined(typeof (NoObjectPool), false))
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			
			this.Domain = parent.Domain;
			component.Id = parent.Id;
			component.ComponentParent = parent;
			
			Game.EventSystem.Awake(component);
			return component;
		}

		private T CreateWithComponentParent<T>(bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component);
			return (T)component;
		}

		private T CreateWithComponentParent<T, A>(A a, bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component, a);
			return (T)component;
		}

		private T CreateWithComponentParent<T, A, B>(A a, B b, bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component, a, b);
			return (T)component;
		}

		private T CreateWithComponentParent<T, A, B, C>(A a, B b, C c, bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component, a, b, c);
			return (T)component;
		}
	}
}