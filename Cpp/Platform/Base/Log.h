// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef BASE_LOG_H
#define BASE_LOG_H

#include <boost/log/trivial.hpp>
#include <boost/filesystem.hpp>

namespace Egametang {

static inline std::string FileName(const char* s)
{
	boost::filesystem::path path(s);
	return path.filename().string();
}

#define LOG(level) BOOST_LOG_TRIVIAL(level) << "[" << FileName(__FILE__) << ":" << __LINE__ << "] "

}
#endif // BASE_LOG_H
