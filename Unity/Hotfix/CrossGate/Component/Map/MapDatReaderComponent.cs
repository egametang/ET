using System.IO;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class MapDatReaderComponentAwakeSystem : AwakeSystem<MapDatReaderComponent>
    {
        public override void Awake(MapDatReaderComponent self)
        {
            self.Awake();
        }
    }

    public class MapDatReaderComponent : Component
    {
        private MemoryStream stream;
        private BinaryReader reader;
        private string CurrentMapId;

        public void Awake()
        {
        }

        public void Init(string mapid)
        {
            CurrentMapId = mapid;

        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();

            CurrentMapId = null;
            stream = null;
            reader = null;
        }
    }
}
