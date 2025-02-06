- [README 中文](./README_zh.md)
- [README English](./README.md)

# HybridCLR

[![license](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/focus-creative-games/hybridclr/blob/main/LICENSE)

![logo](https://github.com/focus-creative-games/hybridclr/raw/main/docs/images/logo.jpg)

<br/>
<br/>

HybridCLR是一个**特性完整、零成本、高性能、低内存**的**近乎完美**的Unity全平台原生c#热更新解决方案。

HybridCLR扩充了il2cpp运行时代码，使它由纯[AOT](https://en.wikipedia.org/wiki/Ahead-of-time_compilation) runtime变成AOT+Interpreter 混合runtime，进而原生支持动态加载assembly，从底层彻底支持了热更新。使用HybridCLR技术的游戏不仅能在Android平台，也能在IOS、Consoles、WebGL等所有il2cpp支持的平台上高效运行。

由于HybridCLR对ECMA-335规范 的良好支持以及对Unity开发工作流的高度兼容，Unity项目在接入HybridCLR后，可以几乎无缝地获得C#代码热更新的能力，开发者不需要改变日常开发习惯和要求。HybridCLR首次实现了将Unity平台的全平台代码热更新方案的工程难度降到几乎为零的水平。

欢迎拥抱现代原生C#热更新技术 ！！！

## 文档

- [官方文档](https://hybridclr.doc.code-philosophy.com/docs/intro)
- [快速上手](https://hybridclr.doc.code-philosophy.com/docs/beginner/quickstart)
- [商业项目案例](https://hybridclr.doc.code-philosophy.com/docs/other/businesscase)


## 特性

- 近乎完整实现了[ECMA-335规范](https://www.ecma-international.org/publications-and-standards/standards/ecma-335/)，只有极少量的[不支持的特性](https://hybridclr.doc.code-philosophy.com/docs/basic/notsupportedfeatures)。
- 零学习和使用成本。对绝大多数开发者来说写代码近乎没有限制。 热更新代码与AOT代码无缝工作，可以随意写继承、**泛型**、**反射**之类的代码。不需要额外写任何特殊代码、没有代码生成
- 完全支持多线程，包含但不限于 volatile、ThreadStatic、async Task等相关功能和特性。这是其他所有热更新方案都不支持的
- 几乎完全兼容Unity的工作流。包括且不限于支持热更新**MonoBehaviour**、ScriptableObject、**DOTS**技术，资源上挂载的热更新脚本可以正确实例化，这是其他所有热更新方案都不支持的
- 执行高效。实现了一个极其高效的寄存器解释器，所有指标都大幅优于其他热更新方案。[性能测试报告](https://hybridclr.doc.code-philosophy.com/docs/basic/performance)
- 内存高效。 热更新脚本中定义的类跟普通c#类占用一样的内存空间，远优于其他热更新方案。[内存占用报告](https://hybridclr.doc.code-philosophy.com/docs/basic/memory)
- 支持MonoPInvokeCallback，可以与native代码或者其他语言如lua、javascript、python良好交互
- 支持一些il2cpp不支持的特性，如__makeref、 __reftype、__refvalue指令
- 支持独创的 **Differential Hybrid Execution(DHE)** 差分混合执行技术，即可以对AOT dll任意增删改，会智能地让未改动的函数以AOT方式运行，变化或者新增的函数以interpreter模式运行，让热更新的游戏逻辑的运行性能基本达到原生AOT的水平
- 支持 **热重载** 技术，可以100%卸载程序集
- 支持现代的dll加密技术，有效保障代码安全

## 支持的版本与平台

- 支持2019.4.x、2020.3.x、2021.3.x、2022.3.x、2023.2.x、**6000.x.y**全系列LTS版本
- 支持所有il2cpp支持的平台
- 支持团结引擎和鸿蒙平台

## 工作原理

HybridCLR从mono的 [mixed mode execution](https://www.mono-project.com/news/2017/11/13/mono-interpreter/) 技术中得到启发，为unity的il2cpp之类的AOT runtime额外提供了interpreter模块，将它们由纯AOT运行时改造为"AOT + Interpreter"混合运行方式。

![icon](https://github.com/focus-creative-games/hybridclr/raw/main/docs/images/architecture.png)

更具体地说，HybridCLR做了以下几点工作：

- 实现了一个高效的元数据(dll)解析库
- 改造了元数据管理模块，实现了元数据的动态注册
- 实现了一个IL指令集到自定义的寄存器指令集的compiler
- 实现了一个高效的寄存器解释器
- 额外提供大量的instinct函数，提升解释器性能

## 稳定性状况

HybridCLR已经被广泛验证是非常高效、稳定的Unity热更新解决方案。

当前发布了**极其稳定**的1.x-5.x正式版本，足以满足大中型商业项目的稳定性要求。
目前有数千个商业游戏项目完成接入，其中有几百款已经双端上线，上线的项目中包括MMORPG、重度卡牌、重度塔防之类的游戏。**绝大多数头部游戏公司**（如腾讯、网易）都已经在使用HybridCLR。

可查看我们已知的头部公司中使用HybridCLR并且已经上线的[项目列表](https://hybridclr.doc.code-philosophy.com/docs/other/businesscase)。

## 支持与联系

- 官方1群：651188171（满）
- 新手1群：428404198（满）
- 新手2群：**680274677（推荐）**
- discord频道 https://discord.gg/BATfNfJnm2
- 商业合作邮箱: business#code-philosophy.com
- [商业化支持](https://hybridclr.doc.code-philosophy.com/docs/business/intro)

## 关于作者

**walon** ：**Code Philosophy（代码哲学）** 创始人

毕业于清华大学物理系，2006年CMO金牌，奥数国家集训队成员，保送清华基科班。专注于游戏技术，擅长开发架构和基础技术设施。

## license

HybridCLR is licensed under the [MIT](https://github.com/focus-creative-games/hybridclr/blob/main/LICENSE) license
