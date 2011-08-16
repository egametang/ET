PROTOBUF_GENERATE_CPP(proto_srcs proto_hdrs
	BehaviorTreeConf.proto
)

SET(BehaviorTreeSrc
	BuffType.cc
	ChangeHealth.cc
	CombatContex.cc
	GameEvents.cc
	NodeFactories.cc
	NotNode.cc
	SelectorNode.cc
	SequenceNode.cc
	BehaviorTree.cc
	${proto_srcs}
)

ADD_LIBRARY(BehaviorTree ${BehaviorTreeSrc})

ADD_EXECUTABLE(GameEventsTest GameEventsTest.cc)

SET(Excutes
	GameEventsTest
)

FOREACH(Excute ${Excutes})
	TARGET_LINK_LIBRARIES(${Excute}
		BehaviorTree
		${ThirdPartyLibs}
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()
