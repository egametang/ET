﻿using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace ET
{
	/// <summary>
	/// 用来缓存数据
	/// </summary>
	public class DBComponent : Entity
	{
		public static DBComponent Instance;
		
		public List<string> Transfers = new List<string>();
		
		public const int TaskCount = 32;
		
		public MongoClient mongoClient;
		public IMongoDatabase database;
		
		public IMongoCollection<T> GetCollection<T>(string collection=null)
		{
			return this.database.GetCollection<T>(collection ?? typeof (T).Name);
		} 
		
		public IMongoCollection<Entity> GetCollection(string name)
		{
			return this.database.GetCollection<Entity>(name);
		}
	}
}
