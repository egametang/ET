using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 
    /// </summary>
    public class RealmDDZAddressComponent : Component
    {
        public readonly List<StartConfig> MapAddress = new List<StartConfig>();

        /// <summary>
        /// 随机获取一个启动配置
        /// </summary>
        /// <returns></returns>
        public StartConfig GetAddress()
        {
            int n = RandomHelper.RandomNumber(0, this.MapAddress.Count);
            return this.MapAddress[n];
        }
    }
}
