// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_DBRESULT_H
#define ORM_DBRESULT_H

#include <vector>
#include <cppconn/resultset.h>
#include "Base/Typedef.h"
#include "Orm/OrmTypedef.h"

namespace Egametang {

class DbResult
{
private:
	ResultSetPtr resultSet;

	void FillMessage(ProtobufMessagePtr message);

public:
	DbResult(ResultSetPtr resultSet);
	virtual ~DbResult();

	void All(std::vector<ProtobufMessagePtr>& messages);

	void One(ProtobufMessagePtr message);

	std::size_t Count();
};

} // namespace Egametang
#endif // ORM_DBRESULT_H
