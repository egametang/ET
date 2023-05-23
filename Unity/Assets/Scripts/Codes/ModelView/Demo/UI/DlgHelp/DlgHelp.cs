namespace ET
{
	 [ComponentOf(typeof(UIBaseWindow))]
	public  class DlgHelp :Entity,IAwake,IUILogic
	{

		public DlgHelpViewComponent View { get => this.Parent.GetComponent<DlgHelpViewComponent>();} 

		 

	}
}
