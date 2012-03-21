// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef BASE_LOG_H
#define BASE_LOG_H

#include <string>
#include <boost/shared_ptr.hpp>
#include <boost/log/trivial.hpp>
#include <boost/log/sources/severity_logger.hpp>

namespace Egametang {

#define ELOG(level) BOOST_LOG_SEV(ELog::GetSLog(), level) << "[" << FileName(__FILE__) << ":" << __LINE__ << "] "

std::string FileName(const char* s);

enum SeverityLevel
{
	INFO       = 0,
	WARNING    = 1,
	ERROR      = 2,
	FATAL      = 3,
};

class ELog: public boost::noncopyable
{
private:
	static bool isInit;
	static boost::log::sources::severity_logger<SeverityLevel> slog;

public:
	static void Init(const char* fileName);
	static boost::log::sources::severity_logger<SeverityLevel>& GetSLog();
};
}
#endif // BASE_LOG_H
