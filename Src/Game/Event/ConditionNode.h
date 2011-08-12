#ifndef EVENT_CONDITIONNODE_H
#define EVENT_CONDITIONNODE_H

#include "Event/ConditionNode.h"

namespace Egametang {

// 条件节点还可以预绑定一些配置参数,例如下面的type字段由策划配置
// 可配置成dot hot之类的
class BuffType: public LogicNodeIf
{
public:
	int type;

public:
	virtual bool Check(ContexIf* contex);
};

} // namespace Egametang


#endif // EVENT_CONDITIONNODE_H
