PROTOBUF_GENERATE_CPP(proto_srcs proto_hdrs 
	EventConf.proto
)

SET(EventSrc 
	${proto_srcs}
)

ADD_LIBRARY(Event ${EventSrc})
