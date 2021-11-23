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
using System.Collections.Generic;

using ILRuntime.Mono.Cecil.Metadata;

using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	using Slot = Row<string, string>;

	sealed class TypeDefinitionCollection : Collection<TypeDefinition> {

		readonly ModuleDefinition container;
		readonly Dictionary<Slot, TypeDefinition> name_cache;

		internal TypeDefinitionCollection (ModuleDefinition container)
		{
			this.container = container;
			this.name_cache = new Dictionary<Slot, TypeDefinition> (new RowEqualityComparer ());
		}

		internal TypeDefinitionCollection (ModuleDefinition container, int capacity)
			: base (capacity)
		{
			this.container = container;
			this.name_cache = new Dictionary<Slot, TypeDefinition> (capacity, new RowEqualityComparer ());
		}

		protected override void OnAdd (TypeDefinition item, int index)
		{
			Attach (item);
		}

		protected override void OnSet (TypeDefinition item, int index)
		{
			Attach (item);
		}

		protected override void OnInsert (TypeDefinition item, int index)
		{
			Attach (item);
		}

		protected override void OnRemove (TypeDefinition item, int index)
		{
			Detach (item);
		}

		protected override void OnClear ()
		{
			foreach (var type in this)
				Detach (type);
		}

		void Attach (TypeDefinition type)
		{
			if (type.Module != null && type.Module != container)
				throw new ArgumentException ("Type already attached");

			type.module = container;
			type.scope = container;
			name_cache [new Slot (type.Namespace, type.Name)] = type;
		}

		void Detach (TypeDefinition type)
		{
			type.module = null;
			type.scope = null;
			name_cache.Remove (new Slot (type.Namespace, type.Name));
		}

		public TypeDefinition GetType (string fullname)
		{
			string @namespace, name;
			TypeParser.SplitFullName (fullname, out @namespace, out name);

			return GetType (@namespace, name);
		}

		public TypeDefinition GetType (string @namespace, string name)
		{
			TypeDefinition type;
			if (name_cache.TryGetValue (new Slot (@namespace, name), out type))
				return type;

			return null;
		}
	}
}
