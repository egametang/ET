project(Cpp)
cmake_minimum_required(VERSION 2.6)

enable_testing()

include_directories(.)
include_directories(ThirdParty)

add_subdirectory(Python)
add_subdirectory(Thread)
add_subdirectory(ThirdParty)
