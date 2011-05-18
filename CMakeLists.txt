project(Cpp)

cmake_minimum_required(VERSION 2.6)

enable_testing()

include_directories(Egametang)
include_directories(ThirdParty)

add_subdirectory(Egametang)
add_subdirectory(ThirdParty)
