using System.Collections.Generic;
using System;
using PF;
using Guid = PF.Guid;

#if NETFX_CORE
using System.Linq;
using WinRTLegacy;
#endif

namespace PF {
	public class JsonMemberAttribute : System.Attribute {
	}
	public class JsonOptInAttribute : System.Attribute {
	}

	/** A very tiny json serializer.
	 * It is not supposed to have lots of features, it is only intended to be able to serialize graph settings
	 * well enough.
	 */
	public class TinyJsonSerializer {
		System.Text.StringBuilder output = new System.Text.StringBuilder();

		Dictionary<Type, Action<System.Object> > serializers = new Dictionary<Type, Action<object> >();

		static readonly System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;

		public static void Serialize (System.Object obj, System.Text.StringBuilder output) {
			new TinyJsonSerializer() {
				output = output
			}.Serialize(obj);
		}

		TinyJsonSerializer () {
			serializers[typeof(float)] = v => output.Append(((float)v).ToString("R", invariantCulture));
			serializers[typeof(bool)] = v => output.Append((bool)v ? "true" : "false");
			serializers[typeof(Version)] = serializers[typeof(uint)] = serializers[typeof(int)] = v => output.Append(v.ToString());
			serializers[typeof(string)] = v => output.AppendFormat("\"{0}\"", v.ToString().Replace("\"", "\\\""));
			serializers[typeof(Vector2)] = v => output.AppendFormat("{{ \"x\": {0}, \"y\": {1} }}", ((Vector2)v).x.ToString("R", invariantCulture), ((Vector2)v).y.ToString("R", invariantCulture));
			serializers[typeof(Vector3)] = v => output.AppendFormat("{{ \"x\": {0}, \"y\": {1}, \"z\": {2} }}", ((Vector3)v).x.ToString("R", invariantCulture), ((Vector3)v).y.ToString("R", invariantCulture), ((Vector3)v).z.ToString("R", invariantCulture));
			serializers[typeof(Guid)] = v => output.AppendFormat("{{ \"value\": \"{0}\" }}", v.ToString());
		}

		void Serialize (System.Object obj) {
			if (obj == null) {
				output.Append("null");
				return;
			}

			var type = obj.GetType();
			var typeInfo = WindowsStoreCompatibility.GetTypeInfo(type);
			if (serializers.ContainsKey(type)) {
				serializers[type] (obj);
			} else if (typeInfo.IsEnum) {
				output.Append('"' + obj.ToString() + '"');
			} else if (obj is System.Collections.IList) {
				output.Append("[");
				var arr = obj as System.Collections.IList;
				for (int i = 0; i < arr.Count; i++) {
					if (i != 0)
						output.Append(", ");
					Serialize(arr[i]);
				}
				output.Append("]");
			} else {
#if NETFX_CORE
				var optIn = typeInfo.CustomAttributes.Any(attr => attr.GetType() == typeof(JsonOptInAttribute));
#else
				var optIn = typeInfo.GetCustomAttributes(typeof(JsonOptInAttribute), true).Length > 0;
#endif
				output.Append("{");
				bool earlier = false;

				while (true) {
#if NETFX_CORE
					var fields = typeInfo.DeclaredFields.Where(f => !f.IsStatic).ToArray();
#else
					var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
#endif
					foreach (var field in fields) {
						if (field.DeclaringType != type) continue;
						if ((!optIn && field.IsPublic) ||
#if NETFX_CORE
							field.CustomAttributes.Any(attr => attr.GetType() == typeof(JsonMemberAttribute))
#else
							field.GetCustomAttributes(typeof(JsonMemberAttribute), true).Length > 0
#endif
							) {
							if (earlier) {
								output.Append(", ");
							}

							earlier = true;
							output.AppendFormat("\"{0}\": ", field.Name);
							Serialize(field.GetValue(obj));
						}
					}

#if NETFX_CORE
					typeInfo = typeInfo.BaseType;
					if (typeInfo == null) break;
#else
					type = type.BaseType;
					if (type == null) break;
#endif
				}
				output.Append("}");
			}
		}

		void QuotedField (string name, string contents) {
			output.AppendFormat("\"{0}\": \"{1}\"", name, contents);
		}
	}

	/** A very tiny json deserializer.
	 * It is not supposed to have lots of features, it is only intended to be able to deserialize graph settings
	 * well enough. Not much validation of the input is done.
	 */
	public class TinyJsonDeserializer {
		System.IO.TextReader reader;

		static readonly System.Globalization.NumberFormatInfo numberFormat = System.Globalization.NumberFormatInfo.InvariantInfo;

		/** Deserializes an object of the specified type.
		 * Will load all fields into the \a populate object if it is set (only works for classes).
		 */
		public static System.Object Deserialize (string text, Type type, System.Object populate = null) {
			return new TinyJsonDeserializer() {
					   reader = new System.IO.StringReader(text)
			}.Deserialize(type, populate);
		}

		/** Deserializes an object of type tp.
		 * Will load all fields into the \a populate object if it is set (only works for classes).
		 */
		System.Object Deserialize (Type tp, System.Object populate = null) {
			var tpInfo = WindowsStoreCompatibility.GetTypeInfo(tp);

			if (tpInfo.IsEnum) {
				return Enum.Parse(tp, EatField());
			} else if (TryEat('n')) {
				Eat("ull");
				TryEat(',');
				return null;
			} else if (Type.Equals(tp, typeof(float))) {
				return float.Parse(EatField(), numberFormat);
			} else if (Type.Equals(tp, typeof(int))) {
				return int.Parse(EatField(), numberFormat);
			} else if (Type.Equals(tp, typeof(uint))) {
				return uint.Parse(EatField(), numberFormat);
			} else if (Type.Equals(tp, typeof(bool))) {
				return bool.Parse(EatField());
			} else if (Type.Equals(tp, typeof(string))) {
				return EatField();
			} else if (Type.Equals(tp, typeof(Version))) {
				return new Version(EatField());
			} else if (Type.Equals(tp, typeof(Vector2))) {
				Eat("{");
				var result = new Vector2();
				EatField();
				result.x = float.Parse(EatField(), numberFormat);
				EatField();
				result.y = float.Parse(EatField(), numberFormat);
				Eat("}");
				return result;
			} else if (Type.Equals(tp, typeof(Vector3))) {
				Eat("{");
				var result = new Vector3();
				EatField();
				result.x = float.Parse(EatField(), numberFormat);
				EatField();
				result.y = float.Parse(EatField(), numberFormat);
				EatField();
				result.z = float.Parse(EatField(), numberFormat);
				Eat("}");
				return result;
			} else if (Type.Equals(tp, typeof(Guid))) {
				Eat("{");
				EatField();
				var result = Guid.Parse(EatField());
				Eat("}");
				return result;
			} else if (Type.Equals(tp, typeof(List<string>))) {
				System.Collections.IList result = new List<string>();

				Eat("[");
				while (!TryEat(']')) {
					result.Add(Deserialize(typeof(string)));
					TryEat(',');
				}
				return result;
			} else if (tpInfo.IsArray) {
				List<System.Object> ls = new List<System.Object>();
				Eat("[");
				while (!TryEat(']')) {
					ls.Add(Deserialize(tp.GetElementType()));
					TryEat(',');
				}
				var arr = Array.CreateInstance(tp.GetElementType(), ls.Count);
				ls.ToArray().CopyTo(arr, 0);
				return arr;
			} else {
				var obj = populate ?? Activator.CreateInstance(tp);
				Eat("{");
				while (!TryEat('}')) {
					var name = EatField();
					var tmpType = tp;
					System.Reflection.FieldInfo field = null;
					while (field == null && tmpType != null) {
						field = tmpType.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
						tmpType = tmpType.BaseType;
					}

					if (field == null) {
						SkipFieldData();
					} else {
						field.SetValue(obj, Deserialize(field.FieldType));
					}
					TryEat(',');
				}
				return obj;
			}
		}

		void EatWhitespace () {
			while (char.IsWhiteSpace((char)reader.Peek()))
				reader.Read();
		}

		void Eat (string s) {
			EatWhitespace();
			for (int i = 0; i < s.Length; i++) {
				var c = (char)reader.Read();
				if (c != s[i]) {
					throw new Exception("Expected '" + s[i] + "' found '" + c + "'\n\n..." + reader.ReadLine());
				}
			}
		}

		System.Text.StringBuilder builder = new System.Text.StringBuilder();
		string EatUntil (string c, bool inString) {
			builder.Length = 0;
			bool escape = false;
			while (true) {
				var readInt = reader.Peek();
				if (!escape && (char)readInt == '"') {
					inString = !inString;
				}

				var readChar = (char)readInt;
				if (readInt == -1) {
					throw new Exception("Unexpected EOF");
				} else if (!escape && readChar == '\\') {
					escape = true;
					reader.Read();
				} else if (!inString && c.IndexOf(readChar) != -1) {
					break;
				} else {
					builder.Append(readChar);
					reader.Read();
					escape = false;
				}
			}

			return builder.ToString();
		}

		bool TryEat (char c) {
			EatWhitespace();
			if ((char)reader.Peek() == c) {
				reader.Read();
				return true;
			}
			return false;
		}

		string EatField () {
			var result = EatUntil("\",}]", TryEat('"'));

			TryEat('\"');
			TryEat(':');
			TryEat(',');
			return result;
		}

		void SkipFieldData () {
			var indent = 0;

			while (true) {
				EatUntil(",{}[]", false);
				var last = (char)reader.Peek();

				switch (last) {
				case '{':
				case '[':
					indent++;
					break;
				case '}':
				case ']':
					indent--;
					if (indent < 0) return;
					break;
				case ',':
					if (indent == 0) {
						reader.Read();
						return;
					}
					break;
				default:
					throw new System.Exception("Should not reach this part");
				}

				reader.Read();
			}
		}
	}
}
