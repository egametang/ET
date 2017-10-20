﻿using System;

namespace Model
{
	public static class EntityFactory
	{
		public static T Create<T>() where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			ObjectEvents.Instance.Awake(disposer);
			return disposer;
		}

		public static T Create<T, A>(A a) where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			ObjectEvents.Instance.Awake(disposer, a);
			return disposer;
		}

		public static T Create<T, A, B>(A a, B b) where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			ObjectEvents.Instance.Awake(disposer, a, b);
			return disposer;
		}

		public static T Create<T, A, B, C>(A a, B b, C c) where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			ObjectEvents.Instance.Awake(disposer, a, b, c);
			return disposer;
		}

		public static T CreateWithId<T>(long id) where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Id = id;
			ObjectEvents.Instance.Awake(disposer);
			return disposer;
		}

		public static T CreateWithId<T, A>(long id, A a) where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Id = id;
			ObjectEvents.Instance.Awake(disposer, a);
			return disposer;
		}

		public static T CreateWithId<T, A, B>(long id, A a, B b) where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Id = id;
			ObjectEvents.Instance.Awake(disposer, a, b);
			return disposer;
		}

		public static T CreateWithId<T, A, B, C>(long id, A a, B b, C c) where T : Disposer
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Id = id;
			ObjectEvents.Instance.Awake(disposer, a, b, c);
			return disposer;
		}
	}
}
