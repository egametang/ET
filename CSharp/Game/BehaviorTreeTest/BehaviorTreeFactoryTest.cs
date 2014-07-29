using System.Collections.Generic;
using BehaviorTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehaviorTreeTest
{
    [TestClass]
    public class BehaviorTreeFactoryTest
    {
        [TestMethod]
        public void TestCreateTree()
        {
            var config = new Config
            {
                Name = "selector",
                Id = 1,
                Args = new List<string> { "11" },
                SubConfigs =
                    new List<Config>
                    {
                        new Config
                        {
                            Name = "sequence",
                            Id = 2,
                            Args = new List<string> { "12" },
                            SubConfigs =
                                new List<Config>
                                {
                                    new Config
                                    {
                                        Name = "selector",
                                        Id = 4,
                                        Args = new List<string> { "14" },
                                    },
                                    new Config
                                    {
                                        Name = "selector",
                                        Id = 5,
                                        Args = new List<string> { "15", "17" },
                                    }
                                }
                        },
                        new Config
                        {
                            Name = "not",
                            Id = 3,
                            Args = new List<string> { "13" },
                            SubConfigs =
                                new List<Config>
                                {
                                    new Config
                                    {
                                        Name = "selector",
                                        Id = 6,
                                        Args = new List<string> { "16" },
                                    }
                                }
                        }
                    }
            };

            var behaviorTreeFactory = new BehaviorTreeFactory();
            BehaviorTree.BehaviorTree behaviorTree = behaviorTreeFactory.CreateTree(config);
            var blackBoard = new BlackBoard();

            Assert.IsTrue(behaviorTree.Run(blackBoard));
        }
    }
}