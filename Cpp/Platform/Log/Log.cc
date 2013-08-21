// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <fstream>
#include <iostream>
#include <string>
#include <boost/make_shared.hpp>
#include <boost/scoped_ptr.hpp>
#include <boost/date_time/posix_time/posix_time.hpp>
#include <boost/log/common.hpp>
#include <boost/log/expressions.hpp>
#include <boost/log/attributes.hpp>
#include <boost/log/sinks.hpp>
#include <boost/log/sources/logger.hpp>
#include <boost/log/utility/empty_deleter.hpp>
#include <boost/log/utility/manipulators/add_value.hpp>
#include <boost/log/attributes/scoped_attribute.hpp>
#include <boost/log/support/date_time.hpp>
#include "Log/Log.h"
#include "Base/Exception.h"

using namespace boost::log;

namespace Egametang {

std::string FileName(std::string s)
{
	boost::filesystem::path path(s);
	return path.filename().string();
}

boost::scoped_ptr< boost::log::sources::severity_logger<SeverityLevel> > Log::slog;

void Log::Init(std::string fileName)
{
	slog.reset(new boost::log::sources::severity_logger<SeverityLevel>());

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

	pSink->locked_backend()->add_stream(
		boost::shared_ptr<std::ostream>(&std::clog, boost::log::empty_deleter()));

	pSink->set_formatter(
		expressions::format("[%1%][%2%][%3%]%4%")
			% expressions::attr< unsigned int >("RecordID")
			% expressions::format_date_time< boost::posix_time::ptime >("TimeStamp", "%Y-%m-%d %H:%M:%S.%f")
			% expressions::attr<attributes::current_thread_id::value_type>("ThreadID")
			% expressions::smessage
	);

	core->add_global_attribute("RecordID", attributes::counter<unsigned int>(1));
	core->add_global_attribute("TimeStamp", attributes::local_clock());
	core->add_global_attribute("ThreadID", attributes::current_thread_id());

	core->add_sink(pSink);
}

boost::log::sources::severity_logger<SeverityLevel>& Log::GetSLog()
{
	if (!slog.get())
	{
		throw Exception() << ErrInfo("use log please Init in main function");
	}
	return *slog;
}

} // Egametang
