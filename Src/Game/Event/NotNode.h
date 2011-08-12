#ifndef EVENT_NOTNODE_H
#define EVENT_NOTNODE_H

#include "Event/NodeIf.h"

namespace Egametang {

class NotNode: public NodeIf
{
private:
	NodeIf* node;

public:
	virtual ~NotNode();
	virtual bool Check(ContexIf* contex);
	virtual void AddChildNode(NodeIf *node, int type);
};

} // namespace Egametang


#endif // EVENT_NOTNODE_H
