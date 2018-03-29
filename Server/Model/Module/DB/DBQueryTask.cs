﻿using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DBQueryTaskSystem : AwakeSystem<DBQueryTask, string, TaskCompletionSource<ComponentWithId>>
	{
		public override void Awake(DBQueryTask self, string collectionName, TaskCompletionSource<ComponentWithId> tcs)
		{
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBQueryTask : DBTask
	{
		public string CollectionName { get; set; }

		public TaskCompletionSource<ComponentWithId> Tcs { get; set; }

		public override async Task Run()
		{
			DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			// 执行查询前先看看cache中是否已经存在
			ComponentWithId component = dbCacheComponent.GetFromCache(this.CollectionName, this.Id);
			if (component != null)
			{
				this.Tcs.SetResult(component);
				return;
			}
			try
			{
				// 执行查询数据库任务
				component = await dbComponent.GetCollection(this.CollectionName).FindAsync((s) => s.Id == this.Id).Result.FirstOrDefaultAsync();
				this.Tcs.SetResult(component);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {Id}", e));
			}
		}
	}
}