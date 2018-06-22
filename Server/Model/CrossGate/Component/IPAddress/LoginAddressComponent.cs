using System.Collections.Generic;

namespace ETModel
{
    public class LoginAddressComponent : Component
    {
        public readonly List<StartConfig> GateAddress = new List<StartConfig>();

        public StartConfig GetAddress()
        {
            int n = RandomHelper.RandomNumber(0, this.GateAddress.Count);
            return this.GateAddress[n];
        }
    }
}
