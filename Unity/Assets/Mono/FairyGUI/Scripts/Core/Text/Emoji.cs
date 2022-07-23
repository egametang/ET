using System;
using System.Collections.Generic;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class Emoji
    {
        /// <summary>
        /// 代表图片资源url。
        /// </summary>
        public string url;

        /// <summary>
        /// 图片宽度。不设置（0）则表示使用原始宽度。
        /// </summary>
        public int width;

        /// <summary>
        /// 图片高度。不设置（0）则表示使用原始高度。
        /// </summary>
        public int height;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Emoji(string url, int width, int height)
        {
            this.url = url;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public Emoji(string url)
        {
            this.url = url;
        }
    }
}
