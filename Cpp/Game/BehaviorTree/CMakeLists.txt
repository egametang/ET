PROTOBUF_GENERATE_CPP(proto_srcs proto_hdrs
	BehaviorTreeConf.proto
)

FILE(GLOB Header "*.h")

SET(BehaviorTreeSrc
	${Header}
	${proto_hdrs}
	${proto_srcs}
	BuffType.cc
	ChangeHealth.cc
	CombatContex.cc
	GameEvents.cc
	NodeFactories.cc
	NotNode.cc
	SelectorNode.cc
	SequenceNode.cc
	BehaviorTree.cc
)

ADD_LIBRARY(BehaviorTree ${BehaviorTreeSrc})
SET_PROPERTY(TARGET BehaviorTree PROPERTY FOLDER "Game")

ADD_EXECUTABLE(GameEventsTest GameEventsTest.cc)

SET(Tests
	GameEventsTest
)

FOREACH(Test ${Tests})
	TARGET_LINK_LIBRARIES(${Test}
		BehaviorTree
		${ThirdPartyLibs}
	)
	ADD_TEST(${Test} ${Test})
	SET_PROPERTY(TARGET ${Test} PROPERTY FOLDER "Tests/Game/BehaviorTree")
ENDFOREACH()
