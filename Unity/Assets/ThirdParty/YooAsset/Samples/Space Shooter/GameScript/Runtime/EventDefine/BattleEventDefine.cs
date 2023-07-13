using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Event;

public class BattleEventDefine
{
	/// <summary>
	/// 分数改变
	/// </summary>
	public class ScoreChange : IEventMessage
	{
		public int CurrentScores;

		public static void SendEventMessage(int currentScores)
		{
			var msg = new ScoreChange();
			msg.CurrentScores = currentScores;
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 游戏结束
	/// </summary>
	public class GameOver : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new GameOver();
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 敌人死亡
	/// </summary>
	public class EnemyDead : IEventMessage
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public static void SendEventMessage(Vector3 position, Quaternion rotation)
		{
			var msg = new EnemyDead();
			msg.Position = position;
			msg.Rotation = rotation;
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 玩家死亡
	/// </summary>
	public class PlayerDead : IEventMessage
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public static void SendEventMessage(Vector3 position, Quaternion rotation)
		{
			var msg = new PlayerDead();
			msg.Position = position;
			msg.Rotation = rotation;
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 小行星爆炸
	/// </summary>
	public class AsteroidExplosion : IEventMessage
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public static void SendEventMessage(Vector3 position, Quaternion rotation)
		{
			var msg = new AsteroidExplosion();
			msg.Position = position;
			msg.Rotation = rotation;
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 敌人发射子弹
	/// </summary>
	public class EnemyFireBullet : IEventMessage
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public static void SendEventMessage(Vector3 position, Quaternion rotation)
		{
			var msg = new EnemyFireBullet();
			msg.Position = position;
			msg.Rotation = rotation;
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 玩家发射子弹
	/// </summary>
	public class PlayerFireBullet : IEventMessage
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public static void SendEventMessage(Vector3 position, Quaternion rotation)
		{
			var msg = new PlayerFireBullet();
			msg.Position = position;
			msg.Rotation = rotation;
			UniEvent.SendMessage(msg);
		}
	}
}