namespace Model
{
	public enum EventIdType
	{
		InitSceneStart = 0,

		BehaviorTreeRunTreeEvent,
		BehaviorTreeOpenEditor,
		BehaviorTreeClickNode,
		BehaviorTreeAfterChangeNodeType,
		BehaviorTreeCreateNode,
		BehaviorTreePropertyDesignerNewCreateClick,
		BehaviorTreeMouseInNode,
		BehaviorTreeConnectState,
		BehaviorTreeReplaceClick,
		BehaviorTreeRightDesignerDrag,

		SessionRecvMessage,
		NumbericChange,

		MessageDeserializeFinish,
		SceneChange,
		FrameUpdate,
		
		LoadingBegin,
		LoadingFinish,

		MaxModelEvent = 10000,
	}
}