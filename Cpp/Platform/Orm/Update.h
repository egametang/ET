// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_UPDATE_H
#define ORM_UPDATE_H

#include <google/protobuf/message.h>
#include "Orm/Expr.h"

namespace Egametang {

class Update
{
private:
	google::protobuf::Message& message;
	Expr where;

public:
	explicit Update(google::protobuf::Message& message);

	Update& Where(Expr expr);

	std::string ToString() const;
};

} // namespace Egametang
#endif // ORM_UPDATE_H
