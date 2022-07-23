
namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void UILoadCallback();

    /// <summary>
    /// 
    /// </summary>
    public interface IUISource
    {
        /// <summary>
        /// 
        /// </summary>
        string fileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool loaded { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        void Load(UILoadCallback callback);

        /// <summary>
        /// 取消加载
        /// </summary>
        void Cancel();
        
    }
}
