using System;
using YooAsset;

namespace ET
{
    /// <summary>
    /// 资源文件查询服务类
    /// </summary>
    public class WebGLGameQueryServices : IBuildinQueryServices
    {
        public bool Query(string packageName, string fileName, string fileCRC)
        {
            return false;
        }

        public string GetFilePath(string packageName, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}