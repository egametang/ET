/***
 *	Tittle: "SUIFW" UI框架项目
 *		主题:全局配置类
 *	Description:
 *		功能:1.定义系统常量。
 *		     2.定义系统全局方法。
 *		     3.定义系统的枚举类型。
 *		     4.系统的委托定义。
 *
 *	Date:   2017
 *	version:    0.1版本
 *	Modify Record:
 *
 */
using UnityEngine;

namespace ETModel
{
    #region  系统枚举类

        /// <summary>
        /// UI窗体（位置）类型。
        /// </summary>
            public enum UIFormsType
        {
            //普通窗体
            Normal,
            //固定窗体
            Fixed,
            //弹出窗体
            Window
        }

        /// <summary>
        /// UI窗体显示类型
        /// </summary>
            public enum UIFormsShowMode
        {
            //普通
            Normal,
            //反向切换
            ReverseChange,
            //隐藏其他
            HideOther
        }

    #endregion
}

