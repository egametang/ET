namespace FairyGUI
{
    /// <summary>
    /// 用于文本输入的键盘接口
    /// </summary>
    public interface IKeyboard
    {
        /// <summary>
        /// 键盘已收回，输入已完成
        /// </summary>
        bool done { get; }

        /// <summary>
        /// 是否支持在光标处输入。如果为true，GetInput返回的是在当前光标处需要插入的文本，如果为false，GetInput返回的是整个文本。
        /// </summary>
        bool supportsCaret { get; }

        /// <summary>
        /// 用户输入的文本。
        /// </summary>
        /// <returns></returns>
        string GetInput();

        /// <summary>
        /// 打开键盘
        /// </summary>
        /// <param name="text"></param>
        /// <param name="autocorrection"></param>
        /// <param name="multiline"></param>
        /// <param name="secure"></param>
        /// <param name="alert"></param>
        /// <param name="textPlaceholder"></param>
        /// <param name="keyboardType"></param>
        /// <param name="hideInput"></param>
        void Open(string text, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder, int keyboardType, bool hideInput);

        /// <summary>
        /// 关闭键盘
        /// </summary>
        void Close();
    }
}
