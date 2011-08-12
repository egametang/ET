#ifndef EVENT_BUFFTYPECONDITION_H
#define EVENT_BUFFTYPECONDITION_H

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

	virtual bool Check(ContexIf* contex);
};

class BuffTypeFactory: NodeFactoryIf
{
public:
	virtual ~BuffTypeFactory();

	virtual NodeIf* GetInstance(const LogicNode& conf);
};

} // namespace Egametang


#endif // EVENT_BUFFTYPECONDITION_H
