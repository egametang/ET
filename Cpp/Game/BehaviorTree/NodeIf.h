#ifndef BEHAVIORTREE_NODEIF_H
#define BEHAVIORTREE_NODEIF_H

#include <string>

namespace Egametang {

class ContexIf;
class BehaviorNodeConf;

class NodeIf
{
public:
	virtual ~NodeIf()
	{
	}
	virtual bool Run(ContexIf* contex) = 0;

	virtual void AddChildNode(NodeIf *node)
	{
	}

	virtual std::string ToString() = 0;
};

class NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const BehaviorNodeConf& conf) = 0;
};

} // namespace Egametang

#endif // BEHAVIORTREE_NODEIF_H
