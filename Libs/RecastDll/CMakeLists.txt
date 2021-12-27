file(GLOB SOURCES *.cpp ../Detour/Source/*.cpp ../DetourCrowd/Source/*.cpp ../DetourTileCache/Source/*.cpp ../Recast/Source/*.cpp)
add_library(RecastDll SHARED ${SOURCES})

include_directories(../DebugUtils/Include)
include_directories(../Detour/Include)
include_directories(../DetourCrowd/Include)
include_directories(../DetourTileCache/Include)
include_directories(../Recast/Include)
include_directories(.)


set(RecastDll_INCLUDE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/Include")

target_include_directories(RecastDll PUBLIC
    "$<BUILD_INTERFACE:${RecastDll_INCLUDE_DIR}>"
)

install(TARGETS RecastDll
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
        ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
        LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
        COMPONENT library
        )

file(GLOB INCLUDES Include/*.h)

install(FILES ${INCLUDES} DESTINATION
    ${CMAKE_INSTALL_INCLUDEDIR}/recastnavigation)
if(MSVC)
    install(FILES "$<TARGET_FILE_DIR:RecastDll>/RecastDll-d.pdb" CONFIGURATIONS "Debug" DESTINATION "lib")
endif()