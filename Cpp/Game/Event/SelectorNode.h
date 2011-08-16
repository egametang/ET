#ifndef EVENT_SELECTORNODE_H
#define EVENT_SELECTORNODE_H

#include <list>
#include "Event/NodeIf.h"

namespace Egametang {

class SelectorNode: public NodeIf
{
private:
	std::list<NodeIf*> nodes;

public:
	virtual ~SelectorNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(NodeIf *node);

	virtual std::string ToString();
};

class SelectorNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang


#endif // EVENT_SELECTORNODE_H
