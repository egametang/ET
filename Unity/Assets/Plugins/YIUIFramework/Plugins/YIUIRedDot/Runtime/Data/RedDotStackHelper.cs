using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace YIUIFramework
{
    public static class RedDotStackHelper
    {
        #region 操作

        static readonly string m_OsCountFormat      = "自己改变数量 {0}  >>  {1}";
        static readonly string m_OsTipsFormat       = "自己改变提示 {0}  >>  {1}  >>  {2}";
        static readonly string m_OsChildCountFormat = "有子类改变数量 {0}  >>  {1}";
        static readonly string m_OsChildTipsFormat  = "有子类改变提示 {0}  >>  {1}  >>  {2}";

        /// <summary>
        /// 获取操作拼接
        /// </summary>
        public static string GetOS(this RedDotStack self, RedDotData currentData)
        {
            var sb = SbPool.Get();
            switch (self.RedDotOSType)
            {
                case ERedDotOSType.Count:
                    sb.AppendFormat(self.FirstData.ChangeData.Key == currentData.Key? m_OsCountFormat : m_OsChildCountFormat,
                        self.OriginalCount, self.ChangeCount);
                    break;
                case ERedDotOSType.Tips:
                    sb.AppendFormat(self.FirstData.ChangeData.Key == currentData.Key? m_OsTipsFormat : m_OsChildTipsFormat,
                        self.ChangeTips, self.OriginalCount, self.ChangeCount);
                    break;
                default:
                    Debug.LogError("此枚举没有实现 " + self.RedDotOSType);
                    break;
            }

            return SbPool.PutAndToStr(sb);
        }

        #endregion

        #region 时间

        static readonly string m_TimeFormat = "{0:D2}/{1:D2}/{2:D2} - {3:D2}:{4:D2}:{5:D2}";

        /// <summary>
        /// 获取操作时间
        /// </summary>
        public static string GetTime(this RedDotStack self)
        {
            var time = self.DataTime;
            var sb   = SbPool.Get();
            sb.AppendFormat(m_TimeFormat, time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
            return SbPool.PutAndToStr(sb);
        }

        #endregion

        #region 来源

        static readonly string m_SourceFormat = "改变的红点ID: {0}  名称: {1}";

        static readonly string m_SourceCountFormat = "    当前提示: {0}  >>  {1}  >>  {2}";

        /// <summary>
        /// 来源
        /// </summary>
        public static string GetSource(this RedDotStack self)
        {
            var data = self.FirstData;
            var sb   = SbPool.Get();
            sb.AppendFormat(m_SourceFormat, (int)data.ChangeData.Config.Key,
                RedDotMgr.Inst.GetKeyDes(data.ChangeData.Config.Key));
            sb.AppendFormat(m_SourceCountFormat, data.ChangeTips, data.OriginalCount, data.ChangeCount);
            return SbPool.PutAndToStr(sb);
        }

        #endregion

        #region 详细堆栈 带缓存

        /// <summary>
        /// 缓存堆栈解析数据
        /// </summary>
        private static Dictionary<StackTrace, string> m_StackContentDic = new Dictionary<StackTrace, string>();

        private static void ClearStackContentDic()
        {
            m_StackContentDic.Clear();
        }

        #region 存储的值

        private static BoolPrefs g_StackHideUnityEngineBoolPrefs =
                new BoolPrefs("RedDot_StackHideUnityEngine", null, true);

        public static bool StackHideUnityEngine
        {
            get => g_StackHideUnityEngineBoolPrefs.Value;
            set
            {
                g_StackHideUnityEngineBoolPrefs.Value = value;
                ClearStackContentDic();
            }
        }

        private static BoolPrefs g_StackHideYIUIBindBoolPrefs =
                new BoolPrefs("RedDot_StackHideYIUIBind", null, true);

        public static bool StackHideYIUIBind
        {
            get => g_StackHideYIUIBindBoolPrefs.Value;
            set
            {
                g_StackHideYIUIBindBoolPrefs.Value = value;
                ClearStackContentDic();
            }
        }

        private static BoolPrefs g_StackHideYIUIFrameworkBoolPrefs =
                new BoolPrefs("RedDot_StackHideYIUIFramework", null, true);

        public static bool StackHideYIUIFramework
        {
            get => g_StackHideYIUIFrameworkBoolPrefs.Value;
            set
            {
                g_StackHideYIUIFrameworkBoolPrefs.Value = value;
                ClearStackContentDic();
            }
        }

        private static BoolPrefs g_ShowStackIndexBoolPrefs =
                new BoolPrefs("RedDot_ShowStackIndex", null, false);

        public static bool ShowStackIndex
        {
            get => g_ShowStackIndexBoolPrefs.Value;
            set
            {
                g_ShowStackIndexBoolPrefs.Value = value;
                ClearStackContentDic();
            }
        }

        private static BoolPrefs g_ShowFileNameStackBoolPrefs =
                new BoolPrefs("RedDot_ShowFileNameStack", null, true);

        public static bool ShowFileNameStack
        {
            get => g_ShowFileNameStackBoolPrefs.Value;
            set
            {
                g_ShowFileNameStackBoolPrefs.Value = value;
                ClearStackContentDic();
            }
        }

        private static BoolPrefs g_ShowFilePathBoolPrefs =
                new BoolPrefs("RedDot_ShowFilePath", null, false);

        public static bool ShowFilePath
        {
            get => g_ShowFilePathBoolPrefs.Value;
            set
            {
                g_ShowFilePathBoolPrefs.Value = value;
                ClearStackContentDic();
            }
        }

        #endregion

        /// <summary>
        /// 根据堆栈 获取到已经解析后的详细信息
        /// </summary>
        public static string GetStackContent(this RedDotStack self)
        {
            var stackTrace = self.StackTrace;
            m_StackContentDic.TryGetValue(stackTrace, out var content);
            if (content != null)
            {
                return content;
            }

            content = AnalysisStack(stackTrace);
            if (string.IsNullOrEmpty(content))
            {
                content = "无堆栈显示 可能都被屏蔽了";
            }

            m_StackContentDic.Add(stackTrace, content);
            return content;
        }

        static readonly string m_FrameFormat = "{0} {1}.{2} : {3} : {4}";
        static readonly string m_FileFormat  = "\tFile: {0}";

        static readonly string m_StackContinueUnityEngine    = "UnityEngine";
        static readonly string m_StackContinueUnityEngineTMP = "TMP";
        static readonly string m_StackContinueYIUIBind       = "YIUIBind";
        static readonly string m_StackContinueYIUIFramework  = "YIUIFramework";

        /// <summary>
        /// 解析堆栈
        /// 会隐藏 某些堆栈 
        /// </summary>
        private static string AnalysisStack(StackTrace stackTrace)
        {
            var sb = SbPool.Get();

            var stackFrames = stackTrace.GetFrames();
            if (stackFrames != null)
            {
                for (int i = 0; i < stackFrames.Length; i++)
                {
                    var stackFrame = stackFrames[i];
                    var method     = stackFrame.GetMethod();
                    var declaring  = method.DeclaringType;
                    if (declaring == null)
                    {
                        continue;
                    }

                    var stackNamespace = declaring.Namespace ?? ""; //命名空间
                    var className      = declaring.Name;            //类名

                    if (StackHideUnityEngine)
                    {
                        if (stackNamespace.Contains(m_StackContinueUnityEngine))
                        {
                            continue;
                        }

                        if (stackNamespace.Contains(m_StackContinueUnityEngineTMP))
                        {
                            continue;
                        }
                    }

                    if (StackHideYIUIBind)
                    {
                        if (stackNamespace.Contains(m_StackContinueYIUIBind))
                        {
                            continue;
                        }
                    }

                    if (StackHideYIUIFramework)
                    {
                        if (stackNamespace.Contains(m_StackContinueYIUIFramework))
                        {
                            continue;
                        }
                    }

                    sb.Append(string.Format(m_FrameFormat,
                        ShowStackIndex? i.ToString() : "",           //堆栈索引
                        stackNamespace,                              //命名空间
                        className,                                   //类名
                        method.Name,                                 //方法名
                        stackFrame.GetFileLineNumber().ToString())); //所在行数

                    //所在的文件路径
                    if (ShowFileNameStack)
                    {
                        var fileName = stackFrame.GetFileName();
                        if (string.IsNullOrEmpty(fileName))
                        {
                            continue;
                        }

                        sb.AppendLine();
                        if (ShowFilePath)
                        {
                            sb.Append(string.Format(m_FileFormat, fileName));
                        }
                        else
                        {
                            var filePathArray = fileName.Split('\\');
                            sb.Append(string.Format(m_FileFormat, filePathArray[filePathArray.Length - 1]));
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine();
                }
            }

            return SbPool.PutAndToStr(sb);
        }

        #endregion
    }
}