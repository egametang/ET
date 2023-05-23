namespace ET
{
	 [ComponentOf(typeof(UIBaseWindow))]
	public  class DlgLobby :Entity,IAwake,IUILogic
	{

		public DlgLobbyViewComponent View { get => this.Parent.GetComponent<DlgLobbyViewComponent>();} 

		 

	}
}
