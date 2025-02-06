
namespace YooAsset
{
    public interface IDeliveryQueryServices
    {
        /// <summary>
        /// 查询是否为开发者分发的资源文件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="fileName">文件名称（包含文件的后缀格式）</param>
        /// <param name="fileCRC">文件哈希值</param>
        /// <returns>返回查询结果</returns>
        bool Query(string packageName, string fileName, string fileCRC);

        /// <summary>
        /// 获取分发资源文件的路径
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="fileName">文件名称（包含文件的后缀格式）</param>
        /// <returns>返回资源文件的路径</returns>
        string GetFilePath(string packageName, string fileName);
    }
}