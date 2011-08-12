#ifndef EVENT_NOTNODE_H
#define EVENT_NOTNODE_H

#include "Event/LogicNodeIf.h"

namespace Egametang {

class NotNode: public LogicNodeIf
{
private:
	LogicNodeIf* node;

public:
	virtual ~NotNode();
	virtual bool Check(ContexIf* contex);
};

} // namespace Egametang


#endif // EVENT_NOTNODE_H
