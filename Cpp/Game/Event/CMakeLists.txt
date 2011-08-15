PROTOBUF_GENERATE_CPP(proto_srcs proto_hdrs 
	EventConf.proto
)

SET(EventSrc
	AndNode.cc
	BuffType.cc
	ChangeHealth.cc
	CombatContex.cc
	Event.cc
	GameEvents.cc
	NodeFactories.cc
	NotNode.cc
	OrNode.cc
	${proto_srcs}
)

ADD_LIBRARY(Event ${EventSrc})

ADD_EXECUTABLE(GameEventsTest GameEventsTest.cc)

SET(Excutes 
	GameEventsTest
)

FOREACH(Excute ${Excutes})
	TARGET_LINK_LIBRARIES(${Excute}
		Event
		${ThirdPartyLibs}
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()
