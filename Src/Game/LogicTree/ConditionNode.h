#ifndef LOGICTREE_CONDITIONNODE_H
#define LOGICTREE_CONDITIONNODE_H

#include "LogicTree/ConditionNode.h"

namespace Egametang {

class ConditionNode: public LogicNodeIf
{
private:
	int world;
	int caster;
	int target;

public:
	virtual bool Run();
};

} // namespace Egametang


#endif // LOGICTREE_CONDITIONNODE_H
