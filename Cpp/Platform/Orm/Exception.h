// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_EXCEPTION_H
#define ORM_EXCEPTION_H

#include "Base/Exception.h"

namespace Egametang {

struct ConnectionException: virtual Exception
{
};
typedef boost::error_info<struct TagConnectionErrNO, int> ConnectionErrNO;
typedef boost::error_info<struct TagConnectionErrStr, std::string> ConnectionErrStr;

}

#endif // ORM_EXCEPTION_H
