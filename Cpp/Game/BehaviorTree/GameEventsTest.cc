#include <fcntl.h>
#include <fstream>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include <google/protobuf/text_format.h>
#include "BehaviorTree/GameEvents.h"
#include "BehaviorTree/NodeFactories.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"
#include "BehaviorTree/SpellBuff.h"
#include "BehaviorTree/CombatContex.h"
#include "BehaviorTree/EventDefine.h"

namespace Egametang {

class GameEventsTest: public testing::Test
{
protected:
	NodeFactories factories;
	GameEvents game_events;

public:
	GameEventsTest():factories(), game_events(factories)
	{
	}

	virtual ~GameEventsTest()
	{
	}
};

static void FileToString(const std::string& file, std::string& string)
{
	std::ifstream in(file.c_str());
	std::ostringstream os;
	os << in.rdbuf();
	string = os.str();
	in.close();
}

TEST_F(GameEventsTest, Vampire)
{
	std::string file = "../Cpp/Game/BehaviorTree/Vampire.txt";
	std::string string;
	FileToString(file, string);
	BehaviorTreeConf conf;
	google::protobuf::TextFormat::ParseFromString(string, &conf);
	game_events.AddEvent(conf);

	Unit caster;
	Unit victim;
	caster.health = 2000;
	victim.health = 2000;
	Spell spell;
	Buff buff;
	spell.caster = &caster;
	spell.victim = &victim;
	CombatContex contex(&spell, &buff);

	game_events.Excute(ON_HIT, &contex);
	ASSERT_EQ(2000, caster.health);
	ASSERT_EQ(2000, victim.health);

	buff.buff_type = 2;
	game_events.Excute(ON_HIT, &contex);
	ASSERT_EQ(2100, caster.health);
	ASSERT_EQ(1900, victim.health);
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
