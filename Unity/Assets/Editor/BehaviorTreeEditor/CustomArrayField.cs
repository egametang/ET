using ETModel;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETEditor
{
	public static class CustomArrayField
	{
		public static int[] IntArrFieldValue(ref bool foldIntArr, string fieldDesc, int[] oldValue)
		{
			if (oldValue == null)
			{
				oldValue = new int[0];
			}
			int[] newValue = oldValue;

			foldIntArr = EditorGUILayout.Foldout(foldIntArr, fieldDesc);
			if (foldIntArr)
			{
				int size = EditorGUILayout.IntField("Size", oldValue.Length);

				newValue = new int[size];
				if (size >= oldValue.Length)
				{
					for (int i = 0; i < oldValue.Length; i++)
					{
						newValue[i] = EditorGUILayout.IntField(i.ToString(), oldValue[i]);
					}
					for (int i = oldValue.Length; i < size; i++)
					{
						newValue[i] = EditorGUILayout.IntField(i.ToString(), newValue[i]);
					}
				}
				else if (size < oldValue.Length)
				{
					for (int i = 0; i < size; i++)
					{
						newValue[i] = EditorGUILayout.IntField(i.ToString(), oldValue[i]);
					}
				}
			}
			return newValue;
		}

		public static long[] LongArrFieldValue(ref bool foldIntArr, string fieldDesc, long[] oldValue)
		{
			if (oldValue == null)
			{
				oldValue = new long[0];
			}
			long[] newValue = oldValue;

			foldIntArr = EditorGUILayout.Foldout(foldIntArr, fieldDesc);
			if (foldIntArr)
			{
				int size = EditorGUILayout.IntField("Size", oldValue.Length);

				newValue = new long[size];
				if (size >= oldValue.Length)
				{
					for (int i = 0; i < oldValue.Length; i++)
					{
						newValue[i] = EditorGUILayout.LongField(i.ToString(), oldValue[i]);
					}
					for (int i = oldValue.Length; i < size; i++)
					{
						newValue[i] = EditorGUILayout.LongField(i.ToString(), newValue[i]);
					}
				}
				else if (size < oldValue.Length)
				{
					for (int i = 0; i < size; i++)
					{
						newValue[i] = EditorGUILayout.LongField(i.ToString(), oldValue[i]);
					}
				}
			}
			return newValue;
		}

		public static float[] FloatArrFieldValue(ref bool foldIntArr, string fieldDesc, float[] oldValue)
		{
			if (oldValue == null)
			{
				oldValue = new float[0];
			}
			float[] newValue = oldValue;

			foldIntArr = EditorGUILayout.Foldout(foldIntArr, fieldDesc);
			if (foldIntArr)
			{
				int size = EditorGUILayout.IntField("Size", oldValue.Length);

				newValue = new float[size];
				if (size >= oldValue.Length)
				{
					for (int i = 0; i < oldValue.Length; i++)
					{
						newValue[i] = EditorGUILayout.FloatField(i.ToString(), oldValue[i]);
					}
					for (int i = oldValue.Length; i < size; i++)
					{
						newValue[i] = EditorGUILayout.FloatField(i.ToString(), newValue[i]);
					}
				}
				else if (size < oldValue.Length)
				{
					for (int i = 0; i < size; i++)
					{
						newValue[i] = EditorGUILayout.FloatField(i.ToString(), oldValue[i]);
					}
				}
			}
			return newValue;
		}

		public static double[] DoubleArrFieldValue(ref bool foldIntArr, string fieldDesc, double[] oldValue)
		{
			if (oldValue == null)
			{
				oldValue = new double[0];
			}
			double[] newValue = oldValue;

			foldIntArr = EditorGUILayout.Foldout(foldIntArr, fieldDesc);
			if (foldIntArr)
			{
				int size = EditorGUILayout.IntField("Size", oldValue.Length);

				newValue = new double[size];
				if (size >= oldValue.Length)
				{
					for (int i = 0; i < oldValue.Length; i++)
					{
						newValue[i] = EditorGUILayout.DoubleField(i.ToString(), oldValue[i]);
					}
					for (int i = oldValue.Length; i < size; i++)
					{
						newValue[i] = EditorGUILayout.DoubleField(i.ToString(), newValue[i]);
					}
				}
				else if (size < oldValue.Length)
				{
					for (int i = 0; i < size; i++)
					{
						newValue[i] = EditorGUILayout.DoubleField(i.ToString(), oldValue[i]);
					}
				}
			}
			return newValue;
		}

		public static string[] StringArrFieldValue(ref bool foldIntArr, string fieldDesc, string[] oldValue)
		{
			if (oldValue == null)
			{
				oldValue = new string[0];
			}
			string[] newValue = oldValue;

			foldIntArr = EditorGUILayout.Foldout(foldIntArr, fieldDesc);
			if (foldIntArr)
			{
				int size = EditorGUILayout.IntField("Size", oldValue.Length);

				newValue = new string[size];
				if (size >= oldValue.Length)
				{
					for (int i = 0; i < oldValue.Length; i++)
					{
						newValue[i] = EditorGUILayout.TextField(i.ToString(), oldValue[i]);
					}
					for (int i = oldValue.Length; i < size; i++)
					{
						newValue[i] = EditorGUILayout.TextField(i.ToString(), newValue[i]);
					}
				}
				else if (size < oldValue.Length)
				{
					for (int i = 0; i < size; i++)
					{
						newValue[i] = EditorGUILayout.TextField(i.ToString(), oldValue[i]);
					}
				}
			}
			return newValue;
		}

		public static Object[] ObjectArrFieldValue(ref bool fold, string fieldDesc, Object[] oldValue, NodeFieldDesc desc)
		{
			if (oldValue == null)
			{
				oldValue = new Object[0];
			}
			Object[] newValue = oldValue;

			fold = EditorGUILayout.Foldout(fold, fieldDesc);
			if (fold)
			{
				int size = EditorGUILayout.IntField("Size", oldValue.Length);

				newValue = new Object[size];
				if (size >= oldValue.Length)
				{
					for (int i = 0; i < oldValue.Length; i++)
					{
						newValue[i] = EditorGUILayout.ObjectField(i.ToString(), oldValue[i], desc.type.GetElementType(), false);
						if (TypeHelper.IsGameObjectArrayType(desc.type) &&
						    !BehaviorTreeArgsDict.SatisfyCondition((GameObject) newValue[i], desc.constraintTypes))
						{
							newValue[i] = null;
						}
					}
					for (int i = oldValue.Length; i < size; i++)
					{
						newValue[i] = EditorGUILayout.ObjectField(i.ToString(), newValue[i], desc.type.GetElementType(), false);
						if (TypeHelper.IsGameObjectArrayType(desc.type) &&
						    !BehaviorTreeArgsDict.SatisfyCondition((GameObject) newValue[i], desc.constraintTypes))
						{
							newValue[i] = null;
						}
					}
				}
				else if (size < oldValue.Length)
				{
					for (int i = 0; i < size; i++)
					{
						newValue[i] = EditorGUILayout.ObjectField(i.ToString(), oldValue[i], desc.type.GetElementType(), false);
						if (TypeHelper.IsGameObjectArrayType(desc.type) &&
						    !BehaviorTreeArgsDict.SatisfyCondition((GameObject) newValue[i], desc.constraintTypes))
						{
							newValue[i] = null;
						}
					}
				}
			}

			return newValue;
		}
	}
}