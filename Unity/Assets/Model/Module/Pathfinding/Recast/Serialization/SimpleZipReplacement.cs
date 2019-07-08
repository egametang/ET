#if ASTAR_NO_ZIP
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding.Serialization.Zip {
	public enum ZipOption {
		Always
	}

	public class ZipFile {
		public System.Text.Encoding AlternateEncoding;
		public ZipOption AlternateEncodingUsage = ZipOption.Always;

		Dictionary<string, ZipEntry> dict = new Dictionary<string, ZipEntry>();

		public void AddEntry (string name, byte[] bytes) {
			dict[name] = new ZipEntry(name, bytes);
		}

		public bool ContainsEntry (string name) {
			return dict.ContainsKey(name);
		}

		public void Save (System.IO.Stream stream) {
			var writer = new System.IO.BinaryWriter(stream);

			writer.Write(dict.Count);
			foreach (KeyValuePair<string, ZipEntry> pair in dict) {
				writer.Write(pair.Key);
				writer.Write(pair.Value.bytes.Length);
				writer.Write(pair.Value.bytes);
			}
		}

		public static ZipFile Read (System.IO.Stream stream) {
			ZipFile file = new ZipFile();

			var reader = new System.IO.BinaryReader(stream);
			int count = reader.ReadInt32();

			for (int i = 0; i < count; i++) {
				var name = reader.ReadString();
				var length = reader.ReadInt32();
				var bytes = reader.ReadBytes(length);

				file.dict[name] = new ZipEntry(name, bytes);
			}

			return file;
		}

		public ZipEntry this[string index] {
			get {
				ZipEntry v;
				dict.TryGetValue(index, out v);
				return v;
			}
		}

		public void Dispose () {
		}
	}

	public class ZipEntry {
		internal string name;
		internal byte[] bytes;

		public ZipEntry (string name, byte[] bytes) {
			this.name = name;
			this.bytes = bytes;
		}

		public void Extract (System.IO.Stream stream) {
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
#endif
