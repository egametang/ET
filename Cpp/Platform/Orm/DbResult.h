// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_DBRESULT_H
#define ORM_DBRESULT_H

#include <vector>
#include <cppconn/resultset.h>
#include "Base/Typedef.h"
#include "Orm/Typedef.h"

namespace Egametang {

class DbResult
{
private:
	ResultSetPtr resultSet;

	void FillMessage(ProtobufMessagePtr message);

public:
	DbResult(ResultSetPtr resultSet);

	void One(ProtobufMessagePtr message);

	std::size_t Count();

	template <typename Table>
	void All(std::vector<boost::shared_ptr<Table> >& messages)
	{
		for (std::size_t i = 0; i < messages.size(); ++i)
		{
			if (!resultSet->next())
			{
				return;
			}
			ProtobufMessagePtr message = messages[i];
			FillMessage(message);
		}
	}
};

} // namespace Egametang
#endif // ORM_DBRESULT_H
