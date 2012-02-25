PROTOBUF_GENERATE_CPP(ProtoSrcs ProtoHdrs 
	Person.proto
)

FILE(GLOB Header "*.h")

SET(OrmSrc 
	${Header}
	${ProtoHdrs}
	${ProtoSrcs}
	Column.cc
	DbHelper.cc
	DbResult.cc
	Expr.cc
	MessageField.cc
)

ADD_LIBRARY(Orm ${OrmSrc})
SET_PROPERTY(TARGET Orm PROPERTY FOLDER "Platform")

ADD_EXECUTABLE(SelectTest SelectTest.cc)
ADD_EXECUTABLE(MessageFieldTest MessageFieldTest.cc)
ADD_EXECUTABLE(ColumnTest ColumnTest.cc)
ADD_EXECUTABLE(ExprTest ExprTest.cc)

SET(Tests
	SelectTest
	MessageFieldTest
	ColumnTest
	ExprTest
)

FOREACH(Test ${Tests})
	TARGET_LINK_LIBRARIES(${Test}
		Orm
		${ThirdPartyLibs}
	)
	ADD_TEST(${Test} ${Test})
	SET_PROPERTY(TARGET ${Test} PROPERTY FOLDER "Tests/Platform/Orm")
ENDFOREACH()