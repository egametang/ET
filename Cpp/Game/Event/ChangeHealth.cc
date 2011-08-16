#include <glog/logging.h>
#include "Event/ChangeHealth.h"
#include "Event/CombatContex.h"
#include "Event/EventConf.pb.h"
#include "Event/SpellBuff.h"

namespace Egametang {

ChangeHealth::ChangeHealth(int32 unit, int32 value):
		unit(unit), value(value)
{
}

ChangeHealth::~ChangeHealth()
{
}

bool ChangeHealth::Run(ContexIf *contex)
{
	Spell* spell = contex->GetSpell();

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

NodeIf* ChangeHealthFactory::GetInstance(const EventNode& conf)
{
	return new ChangeHealth(conf.args(0), conf.args(1));
}

} // namespace Egametang

