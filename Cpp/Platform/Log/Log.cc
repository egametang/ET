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

#include "Log/Log.h"

namespace attrs = boost::log::attributes;
namespace fmt = boost::log::formatters;
namespace keywords = boost::log::keywords;

namespace Egametang {

std::string FileName(const char* s)
{
	boost::filesystem::path path(s);
	return path.filename().string();
}

BoostLogInit::BoostLogInit(const char* fileName)
{
	auto core = boost::log::core::get();
	core->add_global_attribute("TimeStamp", boost::make_shared<attrs::local_clock>());
	core->add_global_attribute("ThreadId", boost::make_shared<attrs::current_thread_id>());

	pSink = boost::make_shared<text_sink>();

	std::string logFileName = std::string(fileName) + ".log";
	auto logStream = boost::make_shared<std::ofstream>(
	        logFileName.c_str());
	if (!logStream->good())
	{
		throw std::runtime_error("Failed to open a log file");
	}
	pSink->locked_backend()->add_stream(logStream);

	pSink->locked_backend()->auto_flush(true);

	pSink->locked_backend()->set_formatter(
	        fmt::format("%1%: %2% - %3%") % fmt::attr<unsigned int>("LineID", keywords::format =
	                "%08x")
	                % fmt::attr<boost::log::attributes::current_thread_id::held_type>("ThreadID")
	                % fmt::attr<boost::log::trivial::severity_level>("Severity") % fmt::message());

	core->add_sink(pSink);
}

BoostLogInit::~BoostLogInit()
{
}

} // Egametang
