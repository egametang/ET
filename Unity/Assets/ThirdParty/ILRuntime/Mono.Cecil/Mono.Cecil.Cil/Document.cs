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

namespace ILRuntime.Mono.Cecil.Cil {

	public enum DocumentType {
		Other,
		Text,
	}

	public enum DocumentHashAlgorithm {
		None,
		MD5,
		SHA1,
		SHA256,
	}

	public enum DocumentLanguage {
		Other,
		C,
		Cpp,
		CSharp,
		Basic,
		Java,
		Cobol,
		Pascal,
		Cil,
		JScript,
		Smc,
		MCpp,
		FSharp,
	}

	public enum DocumentLanguageVendor {
		Other,
		Microsoft,
	}

	public sealed class Document : DebugInformation {

		string url;

		Guid type;
		Guid hash_algorithm;
		Guid language;
		Guid language_vendor;

		byte [] hash;
		byte [] embedded_source;

		public string Url {
			get { return url; }
			set { url = value; }
		}

		public DocumentType Type {
			get { return type.ToType (); }
			set { type = value.ToGuid (); }
		}

		public Guid TypeGuid {
			get { return type; }
			set { type = value; }
		}

		public DocumentHashAlgorithm HashAlgorithm {
			get { return hash_algorithm.ToHashAlgorithm (); }
			set { hash_algorithm = value.ToGuid (); }
		}

		public Guid HashAlgorithmGuid {
			get { return hash_algorithm; }
			set { hash_algorithm = value; }
		}

		public DocumentLanguage Language {
			get { return language.ToLanguage (); }
			set { language = value.ToGuid (); }
		}

		public Guid LanguageGuid {
			get { return language; }
			set { language = value; }
		}

		public DocumentLanguageVendor LanguageVendor {
			get { return language_vendor.ToVendor (); }
			set { language_vendor = value.ToGuid (); }
		}

		public Guid LanguageVendorGuid {
			get { return language_vendor; }
			set { language_vendor = value; }
		}

		public byte [] Hash {
			get { return hash; }
			set { hash = value; }
		}

		public byte[] EmbeddedSource {
			get { return embedded_source; }
			set { embedded_source = value; }
		}

		public Document (string url)
		{
			this.url = url;
			this.hash = Empty<byte>.Array;
			this.embedded_source = Empty<byte>.Array;
			this.token = new MetadataToken (TokenType.Document);
		}
	}
}
