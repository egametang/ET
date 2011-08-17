#ifndef BEHAVIORTREE_BUFFTYPE_H
#define BEHAVIORTREE_BUFFTYPE_H

#include "BehaviorTree/BehaviorNodeIf.h"

namespace Egametang {

class ContexIf;

// 条件节点还可以预绑定一些配置参数,
// 例如下面的buff_type字段由策划配置
// 可配置成dot hot之类的, 由工厂类设置
class BuffType: public BehaviorNodeIf
{
private:
	int32 buff_type;

public:
	BuffType(int32 type, int buff_type);

	virtual ~BuffType();

	virtual bool Run(ContexIf* contex);

	virtual std::string ToString();
};

class BuffTypeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual ~BuffTypeFactory();

	virtual BehaviorNodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_BUFFTYPE_H
