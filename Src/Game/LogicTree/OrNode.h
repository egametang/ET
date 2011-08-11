#ifndef LOGICTREE_ORNODE_H
#define LOGICTREE_ORNODE_H

#include "LogicTree/LogicNodeIf.h"

namespace Egametang {

class OrNode: public LogicNodeIf
{
private:
	std::list<LogicNodeIf*> nodes;

public:
	virtual bool Run(LogicContex* contex);
};

} // namespace Egametang


#endif // LOGICTREE_ORNODE_H
