﻿namespace Model
{
	public enum EventIdType
	{
		InitSceneStart,

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
	}
}