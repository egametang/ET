using PF;

namespace ETModel
{
    [ObjectSystem]
    public class PathfindingComponentAwakeSystem : AwakeSystem<PathfindingComponent>
    {
        public override void Awake(PathfindingComponent self)
        {
            self.PathReturnQueue = new PathReturnQueue(self);
            self.PathProcessor = new PathProcessor(self.PathReturnQueue, 1, false);
            
            // 读取寻路配置
            self.AStarConfig = new AStarConfig(); //MongoHelper.FromJson<AStarConfig>(File.ReadAllText("./pathfinding.config"));
            self.AStarConfig.pathProcessor = self.PathProcessor;
            
            // 读取地图数据
            self.AStarConfig.graphs = DeserializeHelper.Load("../Config/graph.bytes");
        }
    }
    
    public class PathfindingComponent: Component
    {
        public PathReturnQueue PathReturnQueue;
        
        public PathProcessor PathProcessor;

        public AStarConfig AStarConfig;
        
        public bool Search(ABPath path)
        {
            this.PathProcessor.queue.Push(path.Path);
            while (this.PathProcessor.CalculatePaths().MoveNext())
            {
                if (path.Path.CompleteState != PathCompleteState.NotCalculated)
                {
                    break;
                }
            }

            if (path.Path.CompleteState != PathCompleteState.Complete)
            {
                return false;
            }
            
            PathModifyHelper.StartEndModify((PF.ABPath)path.Path);
            PathModifyHelper.FunnelModify(path.Path);

            return true;
        }
    }
}