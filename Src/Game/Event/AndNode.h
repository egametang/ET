#ifndef EVENT_ANDNODE_H
#define EVENT_ANDNODE_H

#include "Event/LogicNodeIf.h"

namespace Egametang {

class AndNode: public LogicNodeIf
{
private:
	std::list<LogicNodeIf*> nodes;

public:
	virtual ~AndNode();
	virtual bool Check(ContexIf* contex);
};

} // namespace Egametang


#endif // EVENT_ANDNODE_H
