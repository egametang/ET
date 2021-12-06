//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ILRuntime.Mono.Cecil {

	public class AssemblyNameReference : IMetadataScope {

		string name;
		string culture;
		Version version;
		uint attributes;
		byte [] public_key;
		byte [] public_key_token;
		AssemblyHashAlgorithm hash_algorithm;
		byte [] hash;

		internal MetadataToken token;

		string full_name;

		public string Name {
			get { return name; }
			set {
				name = value;
				full_name = null;
			}
		}

		public string Culture {
			get { return culture; }
			set {
				culture = value;
				full_name = null;
			}
		}

		public Version Version {
			get { return version; }
			set {
				version = Mixin.CheckVersion (value);
				full_name = null;
			}
		}

		public AssemblyAttributes Attributes {
			get { return (AssemblyAttributes) attributes; }
			set { attributes = (uint) value; }
		}

		public bool HasPublicKey {
			get { return attributes.GetAttributes ((uint) AssemblyAttributes.PublicKey); }
			set { attributes = attributes.SetAttributes ((uint) AssemblyAttributes.PublicKey, value); }
		}

		public bool IsSideBySideCompatible {
			get { return attributes.GetAttributes ((uint) AssemblyAttributes.SideBySideCompatible); }
			set { attributes = attributes.SetAttributes ((uint) AssemblyAttributes.SideBySideCompatible, value); }
		}

		public bool IsRetargetable {
			get { return attributes.GetAttributes ((uint) AssemblyAttributes.Retargetable); }
			set { attributes = attributes.SetAttributes ((uint) AssemblyAttributes.Retargetable, value); }
		}

		public bool IsWindowsRuntime {
			get { return attributes.GetAttributes ((uint) AssemblyAttributes.WindowsRuntime); }
			set { attributes = attributes.SetAttributes ((uint) AssemblyAttributes.WindowsRuntime, value); }
		}

		public byte [] PublicKey {
			get { return public_key ?? Empty<byte>.Array; }
			set {
				public_key = value;
				HasPublicKey = !public_key.IsNullOrEmpty ();
				public_key_token = null;
				full_name = null;
			}
		}

		public byte [] PublicKeyToken {
			get {
				if (public_key_token == null && !public_key.IsNullOrEmpty ()) {
					var hash = HashPublicKey ();
					// we need the last 8 bytes in reverse order
					var local_public_key_token = new byte [8];
					Array.Copy (hash, (hash.Length - 8), local_public_key_token, 0, 8);
					Array.Reverse (local_public_key_token, 0, 8);
					Interlocked.CompareExchange (ref public_key_token, local_public_key_token, null); // publish only once finished (required for thread-safety)
				}
				return public_key_token ?? Empty<byte>.Array;
			}
			set {
				public_key_token = value;
				full_name = null;
			}
		}

		byte [] HashPublicKey ()
		{
			HashAlgorithm algorithm;

			switch (hash_algorithm) {
			case AssemblyHashAlgorithm.Reserved:
				algorithm = MD5.Create ();
				break;
			default:
				// None default to SHA1
				algorithm = SHA1.Create ();
				break;
			}

			using (algorithm)
				return algorithm.ComputeHash (public_key);
		}

		public virtual MetadataScopeType MetadataScopeType {
			get { return MetadataScopeType.AssemblyNameReference; }
		}

		public string FullName {
			get {
				if (full_name != null)
					return full_name;

				const string sep = ", ";

				var builder = new StringBuilder ();
				builder.Append (name);
				builder.Append (sep);
				builder.Append ("Version=");
				builder.Append (version.ToString (fieldCount: 4));
				builder.Append (sep);
				builder.Append ("Culture=");
				builder.Append (string.IsNullOrEmpty (culture) ? "neutral" : culture);
				builder.Append (sep);
				builder.Append ("PublicKeyToken=");

				var pk_token = PublicKeyToken;
				if (!pk_token.IsNullOrEmpty () && pk_token.Length > 0) {
					for (int i = 0 ; i < pk_token.Length ; i++) {
						builder.Append (pk_token [i].ToString ("x2"));
					}
				} else
					builder.Append ("null");

				if (IsRetargetable) {
					builder.Append (sep);
					builder.Append ("Retargetable=Yes");
				}

				Interlocked.CompareExchange (ref full_name, builder.ToString (), null);

				return full_name;
			}
		}

		public static AssemblyNameReference Parse (string fullName)
		{
			if (fullName == null)
				throw new ArgumentNullException ("fullName");
			if (fullName.Length == 0)
				throw new ArgumentException ("Name can not be empty");

			var name = new AssemblyNameReference ();
			var tokens = fullName.Split (',');
			for (int i = 0; i < tokens.Length; i++) {
				var token = tokens [i].Trim ();

				if (i == 0) {
					name.Name = token;
					continue;
				}

				var parts = token.Split ('=');
				if (parts.Length != 2)
					throw new ArgumentException ("Malformed name");

				switch (parts [0].ToLowerInvariant ()) {
				case "version":
					name.Version = new Version (parts [1]);
					break;
				case "culture":
					name.Culture = parts [1] == "neutral" ? "" : parts [1];
					break;
				case "publickeytoken":
					var pk_token = parts [1];
					if (pk_token == "null")
						break;

					name.PublicKeyToken = new byte [pk_token.Length / 2];
					for (int j = 0; j < name.PublicKeyToken.Length; j++)
						name.PublicKeyToken [j] = Byte.Parse (pk_token.Substring (j * 2, 2), NumberStyles.HexNumber);

					break;
				}
			}

			return name;
		}

		public AssemblyHashAlgorithm HashAlgorithm {
			get { return hash_algorithm; }
			set { hash_algorithm = value; }
		}

		public virtual byte [] Hash {
			get { return hash; }
			set { hash = value; }
		}

		public MetadataToken MetadataToken {
			get { return token; }
			set { token = value; }
		}

		internal AssemblyNameReference ()
		{
			this.version = Mixin.ZeroVersion;
			this.token = new MetadataToken (TokenType.AssemblyRef);
		}

		public AssemblyNameReference (string name, Version version)
		{
			Mixin.CheckName (name);

			this.name = name;
			this.version = Mixin.CheckVersion (version);
			this.hash_algorithm = AssemblyHashAlgorithm.None;
			this.token = new MetadataToken (TokenType.AssemblyRef);
		}

		public override string ToString ()
		{
			return this.FullName;
		}
	}

	partial class Mixin {

		public static Version ZeroVersion = new Version (0, 0, 0 ,0);

		public static Version CheckVersion (Version version)
		{
			if (version == null)
				return ZeroVersion;

			if (version.Build == -1)
				return new Version (version.Major, version.Minor, 0, 0);

			if (version.Revision == -1)
				return new Version (version.Major, version.Minor, version.Build, 0);

			return version;
		}
	}
}
