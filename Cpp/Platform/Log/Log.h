// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef BASE_LOG_H
#define BASE_LOG_H

#include <string>
#include <boost/shared_ptr.hpp>
#include <boost/log/trivial.hpp>
#include <boost/filesystem.hpp>
#include <boost/log/sinks/sync_frontend.hpp>
#include <boost/log/sinks/text_ostream_backend.hpp>

namespace Egametang {

#define ELOG(level) BOOST_LOG_TRIVIAL(level) << "[" << FileName(__FILE__) << ":" << __LINE__ << "] "

std::string FileName(const char* s);

class BoostLogInit
{
private:
	typedef boost::log::sinks::synchronous_sink<boost::log::sinks::text_ostream_backend> text_sink;
	boost::shared_ptr<text_sink> pSink;

public:
	BoostLogInit(const char* fileName);
	~BoostLogInit();
};
}
#endif // BASE_LOG_H
