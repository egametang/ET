#ifndef EVENT_ORNODE_H
#define EVENT_ORNODE_H

#include <list>
#include "Event/NodeIf.h"

namespace Egametang {

class OrNode: public NodeIf
{
private:
	std::list<NodeIf*> nodes;

public:
	virtual ~OrNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(NodeIf *node);

	virtual std::string ToString();
};

class OrNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang


#endif // EVENT_ORNODE_H
