/***
 *	Tittle: "SUIFW" UI框架项目
 *		主题:UI窗体基础类
 *	Description:
 *		功能: 定义所有UI窗体共有方法。
 *		定义四个生命周期
 *		1：Display 显示状态。
 *		2：Hiding  隐藏状态。
 *		3：ReDisney 再显示状态。
 *		4：Freeze 冻结状态。
 *		
 *	Date:   2017
 *	version:    0.1版本
 *	Modify Record:
 *
 */

using System.Threading.Tasks;
using FairyGUI;

namespace ETModel
{
    public class FGUIBase : Entity
    {
        public FGUIType CurrentUIType { get; } = new FGUIType();
        
        protected string pakName;

        // 对应的FairyGUi的ui组件
        public GObject GObject;

        // Fairygui的窗口。如果是弹出窗体，使用Window展示。否则使用GRoot展示
        public ExWindow Window;


        #region 脚本生命周期

        public virtual void Awake()
        {
            if (string.IsNullOrEmpty(this.pakName))
            {
                Log.Error(this.GetType() + "/Awake() 初始化Ui界面失败，pakName为空！");
                return;
            }

            // 热更层的话, 根据自己需要从AB包中读取
            UIPackage.AddPackage("FGUI/" + pakName);

            //弹出窗体
            if (CurrentUIType.UIForms_Type == UIFormsType.Window)
            {
                this.Window = new ExWindow();
                this.Window.contentPane = this.GObject.asCom;
                this.Window.doShowAnimationEvent += DoShowAnimationEvent;
                this.Window.doHideAnimationEvent += DoHideAnimationEvent;
                this.Window.onHideEvent += OnHideEvent;
            }
        }

        /// <summary>
        /// 关闭当前UI窗体
        /// </summary>
        protected void Close()
        {
            if (CurrentUIType.UIForms_Type == UIFormsType.Window)
            {
                if (this.Window != null)
                {
                    this.Window.Hide();
                }
            }
            else
            {
                FGUIManagerComponent.Instance.CloseUIForms(this.GetType());
            }
        }

        #region 子类重写方法

        /// <summary>
        /// 重写窗口关闭时的逻辑。_窗口 弹出窗体。
        /// </summary>
        /// <returns></returns>
        protected virtual Task HideEvent()
        {
            return Task.CompletedTask;
        }


        /// <summary>
        /// 重写打开窗口时动画的展示方法 
        /// </summary>
        protected virtual void DoShowAnimationEvent()
        {
        }

        /// <summary>
        /// 重写关闭窗口的动画展示方法。
        /// </summary>
        /// <returns></returns>
        protected virtual Task HideAnimationEvent()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 窗口关闭动画
        /// </summary>
        private async void DoHideAnimationEvent()
        {
            await HideAnimationEvent();
            if (this.Window != null)
            {
                this.Window.HideImmediately();
            }
        }

        /// <summary>
        /// 关闭窗体事件
        /// </summary>
        private async void OnHideEvent()
        {
            Log.Debug("---OnHideEvent执行了:" + this.GetType());
            await HideEvent();
            //window窗体使用fairyGui的方式关闭必须要通过框架再关一次
            if (this.CurrentUIType.UIForms_Type == UIFormsType.Window)
            {
                FGUIManagerComponent.Instance.CloseUIForms(this.GetType());
            }
        }

        #endregion

        #endregion

        #region 窗体生命周期

        /// <summary>
        /// 显示状态
        /// </summary>
        public virtual void Display()
        {
            Log.Debug("执行了Display(),打开了：" + this.GetType());
            //弹出窗体
            if (CurrentUIType.UIForms_Type == UIFormsType.Window)
            {
                this.Window.Show();
            }
            else
            {
                GRoot.inst.AddChild(this.GObject);
                this.DoShowAnimationEvent();
            }
        }

        /// <summary>
        /// 隐藏状态(窗口关闭了)
        /// </summary>
        public virtual void Hiding()
        {
            //弹出窗体
            if (CurrentUIType.UIForms_Type == UIFormsType.Window)
            {
                Log.Debug("关闭了Window:" + this.GetType());
                if (this.GObject != null)
                {
                    this.GObject.Dispose();
                }
                if (this.Window != null)
                {
                    this.Window.Dispose();
                }
            }
            else if (this.GObject.parent != null)
            {
                this.OnHideEvent();
                this.DoHideAnimationEvent();
                this.GObject.Dispose();
            }
        }

        /// <summary>
        /// 再显示状态
        /// </summary>
        public virtual void ReDisplay()
        {
            //弹出窗体
            if (CurrentUIType.UIForms_Type == UIFormsType.Window && this.Window.parent != null)
            {
                Log.Debug("执行了Redisplay,打开了Window：" + this.GetType());
                this.Window.visible = true;
            }
            else
            {
                Log.Debug("执行了redisplay非window窗体:" + this.GetType());
                this.GObject.visible = true;
            }
        }

        /// <summary>
        /// 冻结状态（在栈集合中）
        /// </summary>
        public virtual void Freeze()
        {
            //弹出窗体
            if (CurrentUIType.UIForms_Type == UIFormsType.Window && this.Window.parent != null)
            {
                Log.Debug("执行了Freeze(),冻结了：" + this.GetType());
                this.Window.visible = false;
            }
            else
            {
                Log.Debug("执行了Freeze(),冻结了非window窗体：" + this.GetType());
                this.GObject.visible = false;
            }
        }

        #endregion

        public override void Dispose()
        {
            if (this.IsDisposed) return;

            this.pakName = null;

            this.GObject = null;

            this.Window?.Dispose();
            this.Window = null;

            base.Dispose();
        }
    }
}