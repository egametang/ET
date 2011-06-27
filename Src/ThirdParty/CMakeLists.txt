# 请先下载源码包,跑一遍第三方库的test,保证所有test通过
# gtest-1.6.0
# gmock-1.6.0

ADD_SUBDIRECTORY(glog)
ADD_SUBDIRECTORY(gmock)
ADD_SUBDIRECTORY(gtest)
ADD_SUBDIRECTORY(gflags)