using System;
using System.Collections.Generic;

namespace ETModel
{
	public enum NodeParamType
	{
		Input,
		Output,
		None
	}

	public enum BehaviorTreeEnum
	{
		SkillShakingTree,
		SkillShakingTree2
	}

	public static class BTEnvKey
	{
		public const string None = "None";
		public const string Owner = "Owner";
		public const string Buff = "Buff";
		public const string Spell = "Spell";
		public const string Unit = "Unit";
		public const string BuffId = "BuffId";
		public const string Skill = "Skill";
		public const string SkillConfig = "SkillConfig";
		public const string SkillTime = "SkillTime";
		public const string SkillVector2 = "SkillVector2";
		public const string SkillAction = "SkillAction";
		public const string SkillEffect = "SkillEffect";
		public const string TargetID = "TargetID";
		public const string DelayRunID = "DelayRunID";
		public const string Attacker = "Attacker";
		public const string Tree = "Tree";
		public const string IsTimeMaxEffect = "IsTimeMaxEffect";
		public const string UnitID = "UnitID";
		public const string IsEnemy = "IsEnemy";
		public const string IsAlly = "IsAlly";
		public const string UnitOldActorID = "UnitOldActorID";
		public const string SkillEnum = "SkillEnum";
		public const string UnusualBreak = "UnusualBreak";
		public const string AssetBundleNameListKey = "AssetBundleNameListKey";
		public const string NodePath = "NodePath";
		public const string Self = "Self";
		public const string FlagID = "FlagID";
		public const string UnitPos = "UnitPos";
		public const string MousePos = "MousePos";
		public const string MouseClickType = "MouseClickType";
		public const string SwitchCase = "SwitchCase";
		public const string ActorID = "ActorID";
		public const string DamagePercentage = "DamagePercentage";
		public const string HittedUnitId = "HittedUnitId";
		public const string PlayerName = "PlayerName";
		public const string UnitName = "UnitName";
		public const string UnitRotation = "UnitRotation";
		public const string BTSource = "BTSource";
		public const string BuffSourceId = "BuffSourceId";
		public const string MyUnit = "MyUnit";
		public const string KillReport = "KillReport";
		public const string KillRadioSound = "KillRadioSound";
		public const string SKillSoundType = "SKillSoundType";
		public const string BuffEffectType = "BuffEffectType";
		public const string CombatType = "CombatType";
		public const string CombatValue = "CombatValue";
		public const string GameOverType = "GameOverType";
		public const string Numeric = "Numeric";
	}

	public static class BehaviorTreeInOutConstrain
	{
		public static string[] GetEnvKeyEnum(Type enumType)
		{
			List<string> list = new List<string>();
			Array array = Enum.GetValues(enumType);
			foreach (var item in array)
			{
				list.Add(item.ToString());
			}
			string[] strArr = new string[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				strArr[i] = list[i];
			}
			return strArr;
		}

		public static List<string> GetTreeEnumList(Dictionary<string, Type> envKeyDict)
		{
			List<string> list = new List<string>();
			foreach (var item in envKeyDict)
			{
				list.Add(item.Key);
			}
			return list;
		}

		public static List<string> GetTreeEnumFilterList(Dictionary<string, Type> envKeyDict, Type type)
		{
			List<string> list = new List<string>();
			foreach (var item in envKeyDict)
			{
				if (item.Value == type)
				{
					list.Add(item.Key);
				}
			}
			return list;
		}
	}
}