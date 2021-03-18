#include <iostream>
#include <string>
#include "tools.h"

namespace Tools
{
// #ifdef _UNICODE
// 	// 来源：https://www.cnblogs.com/happykoukou/p/5427268.html
// 	//字符串分割函数
// 	//因为只用到了坐标分割，所以这个版本的split不是很必要
// 	std::vector<std::wstring> Tools::split(std::wstring str, std::wstring pattern)
// 	{
// 		std::wstring::size_type pos;
// 		std::vector<std::wstring> result;
// 		str += pattern;//扩展字符串以方便操作
// 		size_t size = str.size();
// 		for (size_t i = 0; i < size; i++)
// 		{
// 			pos = str.find(pattern, i);
// 			if (pos < size)
// 			{
// 				std::wstring s = str.substr(i, pos - i);
// 				result.push_back(s);
// 				i = pos + pattern.size() - 1;
// 			}
// 		}
// 		return result;
// 	}
//
// #else

	// 字符串分割函数
	std::vector<std::string> split(std::string str, std::string pattern)
	{
		std::string::size_type pos;
		std::vector<std::string> result;
		str += pattern;//扩展字符串以方便操作
		size_t size = str.size();
		for (size_t i = 0; i < size; i++)
		{
			pos = str.find(pattern, i);
			if (pos < size)
			{
				std::string s = str.substr(i, pos - i);
				result.push_back(s);
				i = pos + pattern.size() - 1;
			}
		}
		return result;
	}

	FN_Proc GetFunc(_moduleType module, const char* funcName) {
		_moduleType mod = reinterpret_cast<_moduleType>(module);
		FN_Proc func;
		func = (FN_Proc)(::GetProcAddress(mod, funcName));
		return func;
	}

// #endif

}