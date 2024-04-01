using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class MainTaskComponent: Entity, IAwake<int,int>, IDestroy
    {
        /// <summary>
        /// 配表Id
        /// </summary>
        public int TableId;
        public MainTaskConfig Table => MainTaskConfigCategory.Instance.Get(this.TableId);
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