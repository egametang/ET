using System;
using System.Collections.Generic;
using System.Reflection;
using ETModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETEditor
{
	public enum NodeChildLimitType
	{
		WithChild,
		WithoutChild,
		All
	}

	public static class NodeMetaHelper
	{
		public static Dictionary<NodeClassifyType, int> NodeTypeCountDict { get; } = new Dictionary<NodeClassifyType, int>
		{
			{ NodeClassifyType.Action, 0 },
			{ NodeClassifyType.Composite, 999 },
			{ NodeClassifyType.Condition, 0 },
			{ NodeClassifyType.Decorator, 999 },
			{ NodeClassifyType.Root, 999 },
			{ NodeClassifyType.DataTransform, 0 }
		};

		public static Dictionary<string, NodeMeta> ExportToDict()
		{
			Dictionary<string, NodeMeta> name2NodeProtoDict = new Dictionary<string, NodeMeta>();
			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				NodeMeta proto = GetNodeTypeProtoFromType(type);
				if (proto == null)
				{
					continue;
				}
				name2NodeProtoDict.Add(proto.name, proto);
			}
			return name2NodeProtoDict;
		}

		public static Assembly GetModelAssembly()
		{
			return typeof(Game).Assembly;
		}

		public static NodeMeta GetNodeTypeProtoFromType(Type type)
		{
			object[] nodeAttrs = type.GetCustomAttributes(typeof(NodeAttribute), false);
			if (nodeAttrs.Length == 0)
			{
				return null;
			}
			object[] nodeDeprecatedAttrs = type.GetCustomAttributes(typeof(NodeDeprecatedAttribute), false);
			NodeAttribute nodeAttribute = nodeAttrs[0] as NodeAttribute;
			NodeDeprecatedAttribute nodeDeprecatedAttribute = null;
			if (nodeDeprecatedAttrs.Length != 0)
			{
				nodeDeprecatedAttribute = nodeDeprecatedAttrs[0] as NodeDeprecatedAttribute;
			}

			NodeMeta proto = new NodeMeta()
			{
				type = nodeAttribute.ClassifytType.ToString(),
				name = type.Name,
				describe = nodeAttribute.Desc
			};
			if (nodeDeprecatedAttribute != null)
			{
				proto.isDeprecated = true;
				proto.deprecatedDesc = nodeDeprecatedAttribute.Desc;
			}

			proto.new_args_desc.AddRange(GetNodeFieldDesc(type, typeof(NodeInputAttribute)));
			proto.new_args_desc.AddRange(GetNodeFieldDesc(type, typeof(NodeOutputAttribute)));
			proto.new_args_desc.AddRange(GetNodeFieldDesc(type, typeof(NodeFieldAttribute)));

			proto.child_limit = NodeTypeCountDict[nodeAttribute.ClassifytType];
			proto.classify = nodeAttribute.ClassifytType.ToString();
			return proto;
		}

		public static List<NodeFieldDesc> GetNodeFieldInOutPutDescList(string nodeName, Type fieldAttributeType)
		{
			Type nodeType = GetNodeType(nodeName);
			if (nodeType == null)
			{
				Log.Error($"{nodeName}节点不存在！！！");
				return null;
			}
			return GetNodeFieldDesc(nodeType, fieldAttributeType);
		}

		public static List<NodeFieldDesc> GetNodeFieldInOutPutFilterDescList(string nodeName, Type fieldAttributeType, Type paramType)
		{
			List<NodeFieldDesc> list = GetNodeFieldInOutPutDescList(nodeName, fieldAttributeType);
			List<NodeFieldDesc> filterList = new List<NodeFieldDesc>();
			foreach (NodeFieldDesc item in list)
			{
				if (item.envKeyType == paramType || item.envKeyType.IsSubclassOf(paramType) || paramType.IsAssignableFrom(item.envKeyType))
				{
					filterList.Add(item);
				}
			}
			return filterList;
		}

		public static List<NodeFieldDesc> GetNodeFieldDesc(Type type, Type fieldAttributeType)
		{
			List<NodeFieldDesc> list = new List<NodeFieldDesc>();
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
			FieldInfo[] fields = type.GetFields(flag);
			foreach (FieldInfo field in fields)
			{
				object[] field_attrs = field.GetCustomAttributes(fieldAttributeType, false);
				if (field_attrs.Length > 0)
				{
					NodeFieldBaseAttribute attri = field_attrs[0] as NodeFieldBaseAttribute;
					NodeFieldDesc desc = new NodeFieldDesc();
					desc.name = field.Name;
					desc.desc = attri.Desc;
					desc.type = field.FieldType;
					desc.value = GetDefaultValue(field.FieldType, attri);
					desc.attributeType = fieldAttributeType;
					desc.envKeyType = attri.envKeyType;
					if ((typeof(NodeInputAttribute) == fieldAttributeType || typeof(NodeInputAttribute) == fieldAttributeType) && desc.envKeyType == null)
					{
						Log.Error($"Node:{type.Name} Field:{desc.name}  _envKeyType can not be null");
						return null;
					}
					object[] constraints = field.GetCustomAttributes(typeof(NodeFieldConstraintAttribute), false);
					if (constraints.Length > 0)
					{
						NodeFieldConstraintAttribute constraint = constraints[0] as NodeFieldConstraintAttribute;
						desc.constraintTypes = constraint.Types;
					}

					list.Add(desc);
				}
			}
			return list;
		}

		public static object GetDefaultValue(Type type, NodeFieldBaseAttribute att)
		{
			if (att.DefaultValue != null)
			{
				if ((TypeHelper.IsEnumType(type) && BTEnvKey.None != att.DefaultValue.ToString()) || !TypeHelper.IsEnumType(type))
				{
					return att.DefaultValue;
				}
			}
			object value = null;
			if (TypeHelper.IsDoubleType(type))
			{
				value = default(double);
			}
			else if (TypeHelper.IsStringType(type))
			{
				value = default(string);
			}
			else if (TypeHelper.IsFloatType(type))
			{
				value = default(float);
			}
			else if (TypeHelper.IsBoolType(type))
			{
				value = default(bool);
			}
			else if (TypeHelper.IsIntType(type))
			{
				value = default(int);
			}
			else if (TypeHelper.IsLongType(type))
			{
				value = default(long);
			}
			else if (TypeHelper.IsIntArrType(type))
			{
				value = default(int[]);
			}
			else if (TypeHelper.IsLongArrType(type))
			{
				value = default(long[]);
			}
			else if (TypeHelper.IsDoubleArrType(type))
			{
				value = default(double[]);
			}
			else if (TypeHelper.IsFloatArrType(type))
			{
				value = default(float[]);
			}
			else if (TypeHelper.IsStringArrType(type))
			{
				value = default(string[]);
			}
			else if (TypeHelper.IsObjectType(type))
			{
				value = default(Object);
			}
			else if (TypeHelper.IsEnumType(type))
			{
				Array array = Enum.GetValues(type);
				value = array.GetValue(0).ToString();
			}
			else if (TypeHelper.IsUnitConfigArrayType(type))
			{
			}
			else if (TypeHelper.IsSpriteArrayType(type))
			{
				value = default(Sprite[]);
			}
			else if (TypeHelper.IsObjectArrayType(type))
			{
				value = default(Object[]);
			}
			else if (TypeHelper.IsConvertble(type))
			{
				value = 1f;
			}
			else
			{
				Log.Error($"行为树节点暂时未支持此类型:{type}！");
				return null;
			}
			return value;
		}

		public static Type GetFieldType(string nodeName, string fieldName)
		{
			Type nodeType = GetNodeType(nodeName);
			FieldInfo fieldInfo = nodeType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (fieldInfo == null)
			{
				Log.Error($"{nodeName}节点不存在此属性:{fieldName}");
			}
			return fieldInfo.FieldType;
		}

		public static bool NodeHasField(string nodeName, string fieldName)
		{
			Type nodeType = GetNodeType(nodeName);
			FieldInfo fieldInfo = nodeType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			return fieldInfo != null;
		}

		public static FieldInfo[] GetFieldInfos(string nodeName)
		{
			Type nodeType = GetNodeType(nodeName);
			FieldInfo[] fieldInfos = nodeType.GetFields();
			return fieldInfos;
		}

		public static Type GetNodeType(string nodeName)
		{
			Assembly assembly = GetModelAssembly();
			Type nodeType = assembly.GetType("ETModel." + nodeName);
			if (nodeType == null)
			{
				Log.Error($"不存在此节点:{nodeName}");
				return null;
			}
			return nodeType;
		}
	}
}