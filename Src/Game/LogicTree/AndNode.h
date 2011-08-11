#ifndef LOGICTREE_ANDNODE_H
#define LOGICTREE_ANDNODE_H

#include "LogicTree/LogicNodeIf.h"

namespace Egametang {

class AndNode: public LogicNodeIf
{
private:
	std::list<LogicNodeIf*> nodes;

public:
	virtual bool Run(LogicContex* contex);
};

} // namespace Egametang


#endif // LOGICTREE_ANDNODE_H
