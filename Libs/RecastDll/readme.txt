把这个目录复制到recastnavigation下，修改顶层的CMakeLists.txt 加上add_subdirectory(RecastDll)即可编译
注意windows下把 C/C++ -> 代码生成->运行库 修改成/MT