#include <fcntl.h>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include <iosfwd>
#include "Event/GameEvents.h"
#include "Event/NodeFactories.h"
#include "Event/EventConf.pb.h"

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

TEST_F(GameEventsTest, DotChangeHealth)
{
	std::string conf_file = "../../../Cpp/Game/Event/DotFirstDamage.txt";
	EventConf conf;
	int fd = -1;
	fd = open(conf_file.c_str(), O_RDONLY);
	conf.ParseFromFileDescriptor(fd);
	VLOG(2) << "conf: " << conf.DebugString();
	close(fd);
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
