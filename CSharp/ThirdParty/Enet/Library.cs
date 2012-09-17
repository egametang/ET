#region License

/*
ENet for C#
Copyright (c) 2011 James F. Bellinger <jfb@zer7.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

#endregion

namespace ENet
{
	public static class Library
	{
		public static void Initialize()
		{
			var inits = new Native.ENetCallbacks();
			int ret = Native.enet_initialize_with_callbacks(Native.ENET_VERSION, ref inits);
			if (ret < 0)
			{
				throw new ENetException(ret, "Initialization failed.");
			}
		}

		public static void Deinitialize()
		{
			Native.enet_deinitialize();
		}

		public static uint Time
		{
			get
			{
				return Native.enet_time_get();
			}
			set
			{
				Native.enet_time_set(value);
			}
		}
	}
}