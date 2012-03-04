#ifndef BASE_EXCEPTION_H
#define BASE_EXCEPTION_H

#include <string>
#include <exception>
#include <boost/exception/all.hpp>

namespace Egametang {

struct Exception: virtual std::exception, virtual boost::exception
{
};
typedef boost::error_info<struct TagErrNO, int> ErrNO;
typedef boost::error_info<struct TagErrStr, std::string> ErrStr;

}

#endif // BASE_EXCEPTION_H
