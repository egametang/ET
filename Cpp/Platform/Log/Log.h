// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef BASE_LOG_H
#define BASE_LOG_H

#include <string>
#include <boost/scoped_ptr.hpp>
#include <boost/noncopyable.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/log/trivial.hpp>
#include <boost/log/sources/severity_logger.hpp>

namespace Egametang {

#define LOG(level) BOOST_LOG_SEV(Log::GetSLog(), level) << "[" << FileName(__FILE__) << ":" << __LINE__ << "] "

std::string FileName(std::string s);

enum SeverityLevel
{
	INFO       = 0,
	WARN       = 1,
	ERR        = 2,
	FATAL      = 3,
};

class Log: public boost::noncopyable
{
private:
	static boost::scoped_ptr< boost::log::sources::severity_logger<SeverityLevel> > slog;

public:
	static void Init(std::string fileName);
	static boost::log::sources::severity_logger<SeverityLevel>& GetSLog();
};
}
#endif // BASE_LOG_H
