// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_TYPEDEF_H
#define ORM_TYPEDEF_H

#include <memory>
#include <cppconn/resultset.h>

namespace Egametang {

class DbResult;
class ResultSetMock;

typedef std::shared_ptr<sql::ResultSet> ResultSetPtr;
typedef std::shared_ptr<sql::Statement> StatementPtr;
typedef std::shared_ptr<DbResult> DbResultPtr;
typedef std::shared_ptr<ResultSetMock> ResultSetMockPtr;

} // namespace Egametang

#endif // ORM_TYPEDEF_H
