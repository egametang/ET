using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class MainTaskComponent: Entity, IAwake, IDestroy
    {
        //todo:这几个只需要一个主Id即可，目前还没有配表，先写
        /// <summary>
        /// 主任务Id
        /// </summary>
        public int MainId;
        /// <summary>
        /// 子任务Id
        /// </summary>
        public int SubId;
        /// <summary>
        /// 任务类型
        /// </summary>
        public int Type;
        /// <summary>
        /// 需要的值
        /// </summary>
        public int NeedProgress;
        /// <summary>
        /// 是否自动完成 0=否 1=是
        /// </summary>
        public int IsAutoComplete;
        
        /// <summary>
        /// 进度
        /// </summary>
        public int Progress;
        /// <summary>
        /// 完成状态 0=未完成 1=已完成待领取
        /// </summary>
        public int Status;
        
    }
}