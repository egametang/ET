// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_ORMTYPEDEF_H
#define ORM_ORMTYPEDEF_H

#include <boost/shared_ptr.hpp>
#include <cppconn/resultset.h>

namespace Egametang {

class DbResult;
class ResultSetMock;

typedef boost::shared_ptr<sql::ResultSet> ResultSetPtr;
typedef boost::shared_ptr<sql::Statement> StatementPtr;
typedef boost::shared_ptr<DbResult> DbResultPtr;
typedef boost::shared_ptr<ResultSetMock> ResultSetMockPtr;

} // namespace Egametang

#endif // ORM_ORMTYPEDEF_H
