#ifndef EVENT_ORNODE_H
#define EVENT_ORNODE_H

#include "Event/NodeIf.h"

namespace Egametang {

class OrNode: public NodeIf
{
private:
	NodeIf* left;
	NodeIf* right;

public:
	virtual ~OrNode();
	virtual bool Check(ContexIf* contex);
	virtual void AddChildNode(NodeIf *node, int type);
};

} // namespace Egametang


#endif // EVENT_ORNODE_H
