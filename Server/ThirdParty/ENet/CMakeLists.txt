INCLUDE_DIRECTORIES(.)

FILE(GLOB Header "enet/*.h")
FILE(GLOB CCer "*.c")

SET(ENetSrc 
	${Header}
	${CCer}
)

ADD_LIBRARY(ENet SHARED ${ENetSrc})

set_target_properties(ENet PROPERTIES 
  LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/"
)