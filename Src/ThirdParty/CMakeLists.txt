# 请先下载源码包,跑一遍第三方库的test,保证所有test通过
# gtest-1.6.0
# gmock-1.6.0

add_subdirectory(glog)
add_subdirectory(gmock)
add_subdirectory(gtest)
add_subdirectory(gflags)