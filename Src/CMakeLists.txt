project(Cpp)
cmake_minimum_required(VERSION 2.6)

enable_testing()

include_directories(Egametang)
include_directories(ThirdParty)

link_directories(/usr/local/lib)

add_subdirectory(Egametang)
add_subdirectory(ThirdParty)
