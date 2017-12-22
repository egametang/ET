using Model;

namespace Hotfix
{
    public static class RoomHelper
    {
        public static RoomConfig GetConfig(RoomLevel level)
        {
            RoomConfig config = new RoomConfig();
            switch (level)
            {
                case RoomLevel.Lv100:
                    config.BasePointPerMatch = 100;
                    config.Multiples = 1;
                    config.MinThreshold = 100 * 10;
                    break;
            }

            return config;
        }
    }
}
