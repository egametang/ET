#ifndef ORM_ORMCONN_H
#define ORM_ORMCONN_H

#include <vector>
#include <boost/shared_ptr.hpp>
#include <google/protobuf/message.h>

namespace Egametang {

class Condition;

class OrmConn
{
private:
	std::string GetInsertSQL(const google::protobuf::Message& message);

public:
	OrmConn();
	virtual ~OrmConn();

	void Select(std::vector<boost::shared_ptr<google::protobuf::Message> >& messages, Condition condition);

	void Insert(const google::protobuf::Message& message);

	void Update(const google::protobuf::Message& message, Condition condition);

	template<typename T>
	void Delete();
};

}

template<typename T> inline void Egametang::OrmConn::Delete()
{
}

 // namespace Egametang
#endif  // ORM_ORMCONN_H
