using ProtoBuf.Meta;
using Unity.Mathematics;

namespace ET
{
    public static class ProtobufRegister
    {
        public static void Init()
        {
        }
        
        static ProtobufRegister()
        {
            RuntimeTypeModel.Default.Add(typeof(float2), false).Add("x", "y");
            RuntimeTypeModel.Default.Add(typeof(float3), false).Add("x", "y", "z");
            RuntimeTypeModel.Default.Add(typeof(float4), false).Add("x", "y", "z", "w");
            RuntimeTypeModel.Default.Add(typeof(quaternion), false).Add("value");
        }
    }
}