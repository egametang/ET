using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    public class World
    {
        [StaticField]
        public static World Instance = new World();
        
        private World()
        {
        }
        
        private Stack<IWorldSingleton> singletons = new();

        private Queue<IWorldSingleton> updates = new();

        private Queue<IWorldSingleton> lateUpdates = new();

        private Queue<IWorldSingleton> loads = new();

        private readonly Queue<Game> loops = new Queue<Game>();

        private readonly Dictionary<int, Game> games = new Dictionary<int, Game>();

        private int idGenerate;

        public Game Create(bool loop = true)
        {
            Game game = new(++this.idGenerate);
            this.games.Add(game.Id, game);
            if (loop)
            {
                this.loops.Enqueue(game);
            }
            return game;
        }

        public void Remove(int id)
        {
            if (this.games.Remove(id, out Game game))
            {
                game.Dispose();    
            }
        }
        
        // 简单线程调度，每次Loop会把所有Game Loop一遍
        public void Loop()
        {
            int count = this.loops.Count;

            using Barrier barrier = new Barrier(1);
            
            while (count-- > 0)
            {
                Game game = this.loops.Dequeue();
                barrier.AddParticipant();
                game.Barrier = barrier;
                if (game.Id == 0)
                {
                    continue;
                }
                this.loops.Enqueue(game);
                ThreadPool.QueueUserWorkItem(game.Loop);
            }

            barrier.SignalAndWait();
        }
    }
}