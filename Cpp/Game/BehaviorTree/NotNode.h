#ifndef BEHAVIORTREE_NOTNODE_H
#define BEHAVIORTREE_NOTNODE_H

#include "BehaviorTree/NodeIf.h"

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
	virtual NodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_NOTNODE_H
