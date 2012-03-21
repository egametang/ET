// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <fstream>
#include <iostream>
#include <boost/shared_ptr.hpp>
#include <boost/make_shared.hpp>
#include <boost/date_time/posix_time/posix_time_types.hpp>
#include <boost/log/common.hpp>
#include <boost/log/filters.hpp>
#include <boost/log/formatters.hpp>
#include <boost/log/attributes.hpp>
#include <boost/log/sinks/text_multifile_backend.hpp>
#include <boost/log/attributes/current_thread_id.hpp>
#include <boost/date_time/posix_time/posix_time_types.hpp>
#include <boost/log/sinks/sync_frontend.hpp>
#include <boost/log/sinks/text_ostream_backend.hpp>
#include <gflags/gflags.h>
#include "Log/Log.h"

DEFINE_bool(logtoconsole, false, "log messages go to stderr instead of logfiles");

using namespace boost::log;

namespace Egametang {

std::string FileName(const char* s)
{
	boost::filesystem::path path(s);
	return path.filename().string();
}

bool ELog::isInit = false;
sources::severity_logger<SeverityLevel> ELog::slog;

void ELog::Init(const char* fileName)
{
	if (isInit)
	{
		return;
	}
	isInit = true;

	auto core = core::get();
	typedef sinks::synchronous_sink<sinks::text_ostream_backend> text_sink;
	auto pSink = boost::make_shared<text_sink>();
	std::string logFileName = FileName(fileName) + ".log";
	auto logStream = boost::make_shared<std::ofstream>(logFileName.c_str());
	if (!logStream->good())
	{
		throw std::runtime_error("Failed to open a log file");
	}
	pSink->locked_backend()->add_stream(logStream);

	// 是否输出到标准错误
	if (FLAGS_logtoconsole)
	{
		pSink->locked_backend()->add_stream(
		        boost::shared_ptr<std::ostream>(&std::clog, boost::log::empty_deleter()));
	}

	pSink->locked_backend()->set_formatter(
			formatters::format("[%1%][%2%][%3%]%4%")
				% formatters::attr<unsigned int>("Line #", keywords::format = "%08x")
				% formatters::date_time<boost::posix_time::ptime>("TimeStamp")
				% formatters::attr<boost::thread::id>("ThreadID", keywords::format = "%05d")
				% formatters::message()
	);

	pSink->set_filter(boost::log::filters::attr<SeverityLevel>("Severity", std::nothrow) >= INFO);

    core->add_global_attribute("Line #", boost::make_shared<attributes::counter<unsigned int>>());
    core->add_global_attribute("TimeStamp", boost::make_shared<attributes::local_clock>());
    core->add_global_attribute("ThreadID", boost::make_shared<attributes::current_thread_id>());

	core->add_sink(pSink);
}

boost::log::sources::severity_logger<SeverityLevel>& ELog::GetSLog()
{
	return slog;
}

} // Egametang
