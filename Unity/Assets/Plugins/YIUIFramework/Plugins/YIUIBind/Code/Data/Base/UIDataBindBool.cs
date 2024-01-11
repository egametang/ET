//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    /// <summary>
    /// bool类型基类
    /// </summary>
    public abstract class UIDataBindBool : UIDataBindSelectBase
    {
        [SerializeField]
        [LabelText("所有结果逻辑")]
        #if UNITY_EDITOR
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        private UIBooleanLogic m_BooleanLogic = UIBooleanLogic.And;

        [OdinSerialize]
        [LabelText("所有计算结果的变量")]
        [ListDrawerSettings(IsReadOnly = true)]
        #if UNITY_EDITOR
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        private List<UIDataBoolRef> m_Datas = new List<UIDataBoolRef>();

        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Bool |
                1 << (int)EUIBindDataType.Int |
                1 << (int)EUIBindDataType.Float |
                1 << (int)EUIBindDataType.String;
        }

        /// <summary>
        /// 结果
        /// </summary>
        protected bool GetResult()
        {
            if (m_Datas == null || m_Datas.Count <= 0)
            {
                return false;
            }

            switch (m_BooleanLogic)
            {
                case UIBooleanLogic.And:
                    var resultAnd = true;
                    foreach (var v in m_Datas)
                    {
                        if (v != null)
                        {
                            resultAnd = resultAnd && v.GetResult();
                        }
                    }

                    return resultAnd;
                case UIBooleanLogic.Or:
                    var resultOr = false;
                    foreach (var v in m_Datas)
                    {
                        if (v != null)
                        {
                            resultOr = resultOr || v.GetResult();
                        }
                    }

                    return resultOr;
                default:
                    Logger.LogError($"没有其他逻辑 也不允许有其他逻辑 {m_BooleanLogic}");
                    return false;
            }
        }

        //刷新后 绑定后的事件
        protected override void OnRefreshData()
        {
            //循环比较
            //没有的就删除
            //缺少的就添加
            //且同步修改后的名字

            foreach (var target in DataSelectDic.Values)
            {
                var exist = false;

                //我的
                foreach (var self in m_Datas)
                {
                    if (target.Data.DataGuid == self.Data.DataGuid)
                    {
                        //存在则刷新
                        self.Refresh(target.Data);
                        exist = true;
                        break;
                    }
                }

                if (exist) continue;

                //说明不存在
                m_Datas.Add(new UIDataBoolRef(target.Data));
            }

            //需要移除 所以从后往前
            for (var i = m_Datas.Count - 1; i >= 0; i--)
            {
                var self  = m_Datas[i];
                var exist = false;
                foreach (var target in DataSelectDic.Values)
                {
                    if (target.Data.DataGuid == self.Data.DataGuid)
                    {
                        exist = true;
                        break;
                    }
                }

                if (exist) continue;

                //说明不一样
                m_Datas.RemoveAt(i);
            }
        }
    }
}