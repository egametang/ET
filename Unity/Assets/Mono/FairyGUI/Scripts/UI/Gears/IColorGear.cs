using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public interface IColorGear
    {
        /// <summary>
        /// 
        /// </summary>
        Color color { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ITextColorGear : IColorGear
    {
        /// <summary>
        /// 
        /// </summary>
        Color strokeColor { get; set; }
    }
}
