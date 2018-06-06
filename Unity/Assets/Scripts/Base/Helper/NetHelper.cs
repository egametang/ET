using System.Collections.Generic;
using System.Net;

namespace ETModel
{
	public static class NetHelper
	{
		public static string[] GetAddressIPs()
		{
			//获取本地的IP地址
			List<string> addressIPs = new List<string>();
			foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (address.AddressFamily.ToString() == "InterNetwork")
				{
					addressIPs.Add(address.ToString());
				}
			}
			return addressIPs.ToArray();
		}

        public static bool IsLocalAddress(string address)
        {
            foreach (string localIp in GetAddressIPs())
            {
                if (localIp.Contains(address))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
