#ifndef EVENT_BUFFTYPE_H
#define EVENT_BUFFTYPE_H

#include "Event/NodeIf.h"

namespace Egametang {

class ContexIf;

// 条件节点还可以预绑定一些配置参数,
// 例如下面的buff_type字段由策划配置
// 可配置成dot hot之类的, 由工厂类设置
class BuffType: public NodeIf
{
private:
	int buff_type;

public:
	BuffType(int buff_type);

	virtual ~BuffType();

	virtual bool Run(ContexIf* contex);

	virtual std::string ToString();
};

class BuffTypeFactory: public NodeFactoryIf
{
public:
	virtual ~BuffTypeFactory();

	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang


#endif // EVENT_BUFFTYPE_H
