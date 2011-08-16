#ifndef EVENT_ANDNODE_H
#define EVENT_ANDNODE_H

#include <list>
#include "Event/NodeIf.h"

namespace Egametang {

class AndNode: public NodeIf
{
private:
	std::list<NodeIf*> nodes;

public:
	virtual ~AndNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(NodeIf *node);

	virtual std::string ToString();
};

class AndNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang


#endif // EVENT_ANDNODE_H
