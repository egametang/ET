using System.IO;
using System.Reflection;
using Common.Base;

namespace Model
{
    public class World : GameObject<World>
    {
        private static readonly World instance = new World();

        public Assembly Assembly { get; set; }

        public static World Instance
        {
            get
            {
                return instance;
            }
        }

        private World()
        {
            this.Assembly = Assembly.Load(File.ReadAllBytes(@"./Controller.dll"));
        }

        public void ReLoad()
        {
            this.Assembly = Assembly.Load(File.ReadAllBytes(@"./Controller.dll"));

            foreach (Component<World> component in this.GetComponents())
            {
                IAssemblyLoader assemblyLoader = component as IAssemblyLoader;
                if (assemblyLoader == null)
                {
                    continue;
                }
                assemblyLoader.Load(this.Assembly);
            }
        }
    }
}