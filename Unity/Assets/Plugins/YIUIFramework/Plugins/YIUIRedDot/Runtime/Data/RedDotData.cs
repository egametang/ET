using System;
using System.Collections.Generic;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 每个具体红点的 运行时数据
    /// </summary>
    public partial class RedDotData
    {
        /// <summary>
        /// Key
        /// </summary>
        public ERedDotKeyType Key => Config.Key;

        /// <summary>
        /// 配置
        /// </summary>
        public RedDotConfigData Config { get; private set; }

        /// <summary>
        /// 操作堆栈
        /// </summary>
        public List<RedDotStack> StackList { get; private set; }

        /// <summary>
        /// 我的所有父级
        /// </summary>
        public HashSet<RedDotData> ParentList { get; private set; }

        /// <summary>
        /// 我的所有子级
        /// </summary>
        public HashSet<RedDotData> ChildList { get; private set; }

        /// <summary>
        /// 红点改变通知
        /// </summary>
        private Action<int> m_OnChangedAction;

        /// <summary>
        /// 当前是否提示
        /// </summary>
        public bool Tips { get; private set; }

        /// <summary>
        /// 当前红点的真实数量
        /// </summary>
        public int RealCount { get; private set; }

        /// <summary>
        /// 当前红点数量  如果关闭了红点永远=0
        /// </summary>
        public int Count => Tips ? RealCount : 0;

        /// <summary>
        /// 本地存储的唯一K
        /// </summary>
        private string m_PlayerTipsKey;

        /// <summary>
        /// 本地存储的值
        /// </summary>
        private BoolPrefs m_PlayerTipsKeyBoolPrefs;

        private RedDotData()
        {
        }

        internal RedDotData(RedDotConfigData config)
        {
            Config     = config;
            ParentList = new HashSet<RedDotData>();
            ChildList  = new HashSet<RedDotData>();
            InitTips();

            #if UNITY_EDITOR || YIUIMACRO_REDDOT_STACK
            StackList = new List<RedDotStack>();
            #endif
        }

        /// <summary>
        /// 初始化 当前红点是否可显示
        /// </summary>
        private void InitTips()
        {
            //如果配置档中不允许开关提示 那么这个提示永远 = true 且不可修改
            if (!Config.SwitchTips)
            {
                Tips = true;
                return;
            }

            m_PlayerTipsKey          = $"PlayerRedDotTips_{(int)Config.Key}";
            m_PlayerTipsKeyBoolPrefs = new BoolPrefs(m_PlayerTipsKey, null, Config.SwitchTips);
            Tips                     = m_PlayerTipsKeyBoolPrefs.Value;
        }

        internal void DeletePlayerTipsPrefs()
        {
            m_PlayerTipsKeyBoolPrefs.Delete();
        }

        /// <summary>
        /// 添加父级对象
        /// </summary>
        internal bool AddParent(RedDotData data)
        {
            //所有父子关联关系 都在编辑器中检查完成
            //要保证不会出现循环引用关系
            ParentList.Add(data);       //目标设定为我的父级
            return data.AddChild(this); //因为他是我的父级所以我为他的子级
        }

        /// <summary>
        /// 添加子对象
        /// </summary>
        private bool AddChild(RedDotData data)
        {
            return ChildList.Add(data);
        }

        /// <summary>
        /// 这个是实时同步修改
        /// 目前使用脏标修改 防止有人同步过快
        /// 没有子级 说明是最后一级 最后一级才可以设置
        /// </summary>
        internal bool TrySetCount(int count)
        {
            if (ChildList.Count <= 0)
            {
                return SetCount(count);
            }

            Debug.LogError($"{Config.Key} 配置 不是最后一级红点 请不要直接修改");
            return false;
        }

        /// <summary>
        /// 设置当前红点数量
        /// 返回值 false = 没有变化
        /// </summary>
        private bool SetCount(int count, RedDotStack stack = null)
        {
            if (RealCount == count)
            {
                return false;
            }


            #if UNITY_EDITOR || YIUIMACRO_REDDOT_STACK

            if (stack == null)
            {
                var firstData = new FirstRedDotChangeData(this, RealCount, count, Tips);
                stack = AddNewStack(ERedDotOSType.Count, RealCount, count, Tips, firstData);
            }
            else
            {
                AddStack(stack);
            }

            #endif

            RealCount = count;
            NotifyChange(stack);
            return true;
        }

        /// <summary>
        /// 通知父级 自己变化了
        /// 此处会递归 直到没有父级
        /// </summary>
        private void NotifyChange(RedDotStack stack)
        {
            InvokeOnChanged();

            foreach (var parent in ParentList)
            {
                parent.ChildChanged(stack);
            }
        }

        /// <summary>
        /// 子对象变化了
        /// 然后自己变化 然后又通知递归
        /// </summary>
        private void ChildChanged(RedDotStack stack)
        {
            var count = 0;
            foreach (var child in ChildList)
            {
                count += child.Count;
            }

            SetCount(count, stack);
        }

        /// <summary>
        /// 设置当前是否提示
        /// </summary>
        internal bool SetTips(bool tips, RedDotStack stack = null)
        {
            if (!Config.SwitchTips)
            {
                Debug.LogError($"{Config.Key} 配置 关闭了红点的开关提示 目前永久提示  禁止修改 ");
                return false;
            }

            if (Tips == tips)
            {
                return false;
            }

            Tips = tips;

            #if UNITY_EDITOR || YIUIMACRO_REDDOT_STACK

            if (stack == null)
            {
                var firstData = new FirstRedDotChangeData(this, RealCount, Count, Tips);
                stack = AddNewStack(ERedDotOSType.Tips, RealCount, Count, Tips, firstData);
            }
            else
            {
                AddStack(stack);
            }

            #endif

            m_PlayerTipsKeyBoolPrefs.Value = tips; //存储到本地
            NotifyChange(stack);                   //提示改变后 通知 监听改变
            return true;
        }

        /// <summary>
        /// 添加一个回调
        /// int = 当前红点数量 (非真实数量)
        /// 并马上设置相关数据
        /// (因为界面初始化时都是需要刷新界面的 所以默认调用一次)
        /// </summary>
        internal void AddOnChanged(Action<int> action)
        {
            m_OnChangedAction += action;
            InvokeOnChanged();
        }

        /// <summary>
        /// 移除回调
        /// </summary>
        /// <param name="action"></param>
        internal void RemoveChanged(Action<int> action)
        {
            m_OnChangedAction -= action;
        }

        //try回调
        private void InvokeOnChanged()
        {
            try
            {
                //回调不会使用真实数量
                m_OnChangedAction?.Invoke(Count);
            }
            catch (Exception e)
            {
                Debug.LogError($"红点改变回调 try报错 请检查原因 {Key}  {e}");
            }
        }

        #if UNITY_EDITOR || YIUIMACRO_REDDOT_STACK

        //因为有堆栈操作的需求 所以还是有可能 同一时间刷新2个节点  他们都有相同的路线时 会被刷新2次
        //因为需要这2次的堆栈操作信息  如果去掉堆栈 可以合并 TODO

        /// <summary>
        /// 添加操作堆栈
        /// 目前只有在编辑器下执行  以后可能修改成一个可开关的 这样在手机上也可以查看 debug模式
        /// </summary>
        private RedDotStack AddNewStack(
            ERedDotOSType         osType,
            int                   originalCount,
            int                   changeCount,
            bool                  changeTips,
            FirstRedDotChangeData firstData)
        {
            var stack = new RedDotStack
            {
                Id            = StackList.Count + 1,
                DataTime      = DateTime.Now,
                StackTrace    = new System.Diagnostics.StackTrace(true),
                RedDotOSType  = osType,
                OriginalCount = originalCount,
                ChangeCount   = changeCount,
                ChangeTips    = changeTips,
                FirstData     = firstData,
            };

            //新的在前 其他地方就不用翻转数据了
            StackList.Insert(0, stack);
            return stack;
        }

        private void AddStack(RedDotStack stack)
        {
            StackList.Insert(0, stack);
        }

        #endif
    }
}