#ifndef EVENT_NOTNODE_H
#define EVENT_NOTNODE_H

#include "Event/NodeIf.h"

namespace Egametang {

class NotNode: public NodeIf
{
private:
	NodeIf* node;

public:
	NotNode();

	virtual ~NotNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(NodeIf *node);

	virtual std::string ToString();
};

class NotNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang


#endif // EVENT_NOTNODE_H
