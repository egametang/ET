#ifndef EVENT_SEQUENCENODE_H
#define EVENT_SEQUENCENODE_H

#include <list>
#include "Event/NodeIf.h"

namespace Egametang {

class SequenceNode: public NodeIf
{
private:
	std::list<NodeIf*> nodes;

public:
	virtual ~SequenceNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(NodeIf *node);

	virtual std::string ToString();
};

class SequenceNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang


#endif // EVENT_SEQUENCENODE_H
