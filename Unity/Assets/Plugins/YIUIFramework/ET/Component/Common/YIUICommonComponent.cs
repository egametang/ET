//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace ET.Client
{
	/// <summary>
	/// UI通用组件
	/// </summary>
	[ComponentOf(typeof(YIUIComponent))]
	public partial class YIUICommonComponent: Entity, IAwake, IYIUIInitialize, IDestroy
	{
		private YIUIComponent _UiBase;
		public YIUIComponent UIBase 
		{
			get
			{
				return this._UiBase ??= this.GetParent<YIUIComponent>();
			}
		}
	}
}