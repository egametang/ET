//
// BaseAssemblyResolver.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2011 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Mono.Collections.Generic;

namespace Mono.Cecil {

	public delegate AssemblyDefinition AssemblyResolveEventHandler (object sender, AssemblyNameReference reference);

	public sealed class AssemblyResolveEventArgs : EventArgs {

		readonly AssemblyNameReference reference;

		public AssemblyNameReference AssemblyReference {
			get { return reference; }
		}

		public AssemblyResolveEventArgs (AssemblyNameReference reference)
		{
			this.reference = reference;
		}
	}


	public class AssemblyResolutionException : FileNotFoundException {

		readonly AssemblyNameReference reference;

		public AssemblyNameReference AssemblyReference {
			get { return reference; }
		}

		public AssemblyResolutionException (AssemblyNameReference reference)
			: base (string.Format ("Failed to resolve assembly: '{0}'", reference))
		{
			this.reference = reference;
		}


	}

	public abstract class BaseAssemblyResolver : IAssemblyResolver {

		static readonly bool on_mono = Type.GetType ("Mono.Runtime") != null;

		readonly Collection<string> directories;



		public void AddSearchDirectory (string directory)
		{
			directories.Add (directory);
		}

		public void RemoveSearchDirectory (string directory)
		{
			directories.Remove (directory);
		}

		public string [] GetSearchDirectories ()
		{
			var directories = new string [this.directories.size];
			Array.Copy (this.directories.items, directories, directories.Length);
			return directories;
		}

		public virtual AssemblyDefinition Resolve (string fullName)
		{
			return Resolve (fullName, new ReaderParameters ());
		}

		public virtual AssemblyDefinition Resolve (string fullName, ReaderParameters parameters)
		{
			if (fullName == null)
				throw new ArgumentNullException ("fullName");

			return Resolve (AssemblyNameReference.Parse (fullName), parameters);
		}

		public event AssemblyResolveEventHandler ResolveFailure;

		protected BaseAssemblyResolver ()
		{
			directories = new Collection<string> (2) { ".", "bin" };
		}

		AssemblyDefinition GetAssembly (string file, ReaderParameters parameters)
		{
			if (parameters.AssemblyResolver == null)
				parameters.AssemblyResolver = this;

			return ModuleDefinition.ReadModule (file, parameters).Assembly;
		}

		public virtual AssemblyDefinition Resolve (AssemblyNameReference name)
		{
			return Resolve (name, new ReaderParameters ());
		}

		public virtual AssemblyDefinition Resolve (AssemblyNameReference name, ReaderParameters parameters)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (parameters == null)
				parameters = new ReaderParameters ();

			var assembly = SearchDirectory (name, directories, parameters);
			if (assembly != null)
				return assembly;



			if (ResolveFailure != null) {
				assembly = ResolveFailure (this, name);
				if (assembly != null)
					return assembly;
			}

			throw new AssemblyResolutionException (name);
		}

		AssemblyDefinition SearchDirectory (AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
		{
			var extensions = new [] { ".exe", ".dll" };
			foreach (var directory in directories) {
				foreach (var extension in extensions) {
					string file = Path.Combine (directory, name.Name + extension);
					if (File.Exists (file))
						return GetAssembly (file, parameters);
				}
			}

			return null;
		}

		static bool IsZero (Version version)
		{
			return version == null || (version.Major == 0 && version.Minor == 0 && version.Build == 0 && version.Revision == 0);
		}

	}
}
