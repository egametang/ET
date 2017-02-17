namespace Model
{
	public static class EventHelper
	{
		public static void Run(int eventId)
		{
#if SERVER
			Game.Scene.GetComponent<EventComponent>().Run(eventId);
#else
			Game.Scene.GetComponent<ILEventComponent>().Run(eventId);
#endif
		}

		public static void Run<A>(int eventId, A a)
		{
#if SERVER
			Game.Scene.GetComponent<EventComponent>().Run(eventId, a);
#else
			Game.Scene.GetComponent<ILEventComponent>().Run(eventId, a);
#endif
		}

		public static void Run<A, B>(int eventId, A a, B b)
		{
#if SERVER
			Game.Scene.GetComponent<EventComponent>().Run(eventId, a, b);
#else
			Game.Scene.GetComponent<ILEventComponent>().Run(eventId, a, b);
#endif
		}

		public static void Run<A, B, C>(int eventId, A a, B b, C c)
		{
#if SERVER
			Game.Scene.GetComponent<EventComponent>().Run(eventId, a, b, c);
#else
			Game.Scene.GetComponent<ILEventComponent>().Run(eventId, a, b, c);
#endif
		}

		public static void Run<A, B, C, D>(int eventId, A a, B b, C c, D d)
		{
#if SERVER
			Game.Scene.GetComponent<EventComponent>().Run(eventId, a, b, c, d);
#else
			Game.Scene.GetComponent<ILEventComponent>().Run(eventId, a, b, c, d);
#endif
		}
	}
}