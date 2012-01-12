#include "BehaviorTree/ChangeHealth.h"
#include "BehaviorTree/CombatContex.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"
#include "BehaviorTree/SpellBuff.h"

namespace Egametang {

ChangeHealth::ChangeHealth(int32 type, int32 unit, int32 value):
		BehaviorNode(type), unit(unit), value(value)
{
}

ChangeHealth::~ChangeHealth()
{
}

bool ChangeHealth::Run(ContexIf *contex)
{
	CombatContex* combat_contex = reinterpret_cast<CombatContex*>(contex);
	Spell* spell = combat_contex->GetSpell();

	Unit* target = NULL;
	if (unit == 0)
	{
		target = spell->caster;
	}
	else
	{
		target = spell->victim;
	}

	target->health += value;

	return true;
}

std::string ChangeHealth::ToString()
{
	std::string s;
	s += "ChangeHealth: \n";
	return s;
}

BehaviorNode* ChangeHealthFactory::GetInstance(const BehaviorNodeConf& conf)
{
	return new ChangeHealth(conf.type(), conf.args(0), conf.args(1));
}

} // namespace Egametang

