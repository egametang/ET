## 目录介绍
./cmake/: 用于Android和IOS构建的cmake文件

./Include/: 用于P/Invoke的头文件

./Source/: 用于P/Invoke的Cpp文件

CmakeLists.txt: CmakeLists文件

其余批处理文件皆为一键生成对应平台动态/静态链接库的入口，直接执行即可

比如要编译linux版本的动态链接库，则需要通过在Linux上通过命令行来执行sh文件

## 编译步骤
clone [recastnavigation](https://github.com/recastnavigation/recastnavigation) 仓库到本地
将本目录直接复制粘贴到Clone的recastnavigation目录
![image](https://user-images.githubusercontent.com/35335061/149318618-bc62a6b1-2afa-41df-a5ea-83655bb1625f.png)
随后执行批处理程序即可
