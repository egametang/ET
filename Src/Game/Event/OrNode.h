#ifndef EVENT_ORNODE_H
#define EVENT_ORNODE_H

#include "Event/LogicNodeIf.h"

namespace Egametang {

class OrNode: public LogicNodeIf
{
private:
	std::list<LogicNodeIf*> nodes;

public:
	virtual ~OrNode();
	virtual bool Check(ContexIf* contex);
};

} // namespace Egametang


#endif // EVENT_ORNODE_H
