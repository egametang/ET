using UnityEngine;

namespace Model
{
    public static class Init
    {
		public static void Start()
		{
			GameObject.Find("/Global").GetComponent<Base.Init>().UpdateAction = Update;
		}

	    public static void Update()
	    {
	    }
    }
}
