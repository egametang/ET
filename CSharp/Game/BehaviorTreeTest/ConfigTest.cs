using System;
using System.Collections.Generic;
using BehaviorTree;
using Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehaviorTreeTest
{
	[TestClass]
	public class ConfigTest
	{
		[TestMethod]
		public void TestJsonToConfig()
		{
			var config = new Config
			{
				Name = "selector",
				Id = 1,
				Args = new List<string> { "11" },
				SubConfigs = new List<Config>
				{
					new Config
					{
						Name = "selector",
						Id = 2,
						Args = new List<string> { "12" },
						SubConfigs = new List<Config>
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
								Args = new List<string> { "15", "17"},
							}
						}
					},

					new Config
					{
						Name = "selector",
						Id = 3,
						Args = new List<string> { "13" },
						SubConfigs = new List<Config>
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

			string json = MongoHelper.ToJson(config);
			Console.WriteLine(json);

			var newConfig = MongoHelper.FromJson<Config>(json);
			Assert.AreEqual(json, MongoHelper.ToJson(newConfig));
		}
	}
}
