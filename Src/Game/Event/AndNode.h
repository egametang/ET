#ifndef EVENT_ANDNODE_H
#define EVENT_ANDNODE_H

#include "Event/NodeIf.h"

namespace Egametang {

class AndNode: public NodeIf
{
private:
	NodeIf* left;
	NodeIf* right;

public:
	virtual ~AndNode();
	virtual bool Check(ContexIf* contex);
	virtual void AddChildNode(NodeIf *node, int type);
};

} // namespace Egametang


#endif // EVENT_ANDNODE_H
