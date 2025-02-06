using System;
using System.Text;
using UnityEngine;

namespace YooAsset
{
    internal enum ERemoteCommand
    {
        /// <summary>
        /// 采样一次
        /// </summary>
        SampleOnce = 0,
    }

    [Serializable]
    internal class RemoteCommand
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        public int CommandType;

        /// <summary>
        /// 命令附加参数
        /// </summary>
        public string CommandParam;


        /// <summary>
        /// 序列化
        /// </summary>
        public static byte[] Serialize(RemoteCommand command)
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(command));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static RemoteCommand Deserialize(byte[] data)
        {
            return JsonUtility.FromJson<RemoteCommand>(Encoding.UTF8.GetString(data));
        }
    }
}