using System;
using Base;
using System.Threading.Tasks;

namespace Controller
{
	[Message]
    public class Entry
    {
		public static async void Init()
		{
			try
			{
				await Task.Factory.StartNew(()=> {Log.Debug("aaaaaaaaaaaaaaaaa");});
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
    }
}
