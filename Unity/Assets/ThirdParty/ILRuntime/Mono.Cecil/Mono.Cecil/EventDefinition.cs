//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System.Threading;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	public sealed class EventDefinition : EventReference, IMemberDefinition {

		ushort attributes;

		Collection<CustomAttribute> custom_attributes;

		internal MethodDefinition add_method;
		internal MethodDefinition invoke_method;
		internal MethodDefinition remove_method;
		internal Collection<MethodDefinition> other_methods;

		public EventAttributes Attributes {
			get { return (EventAttributes) attributes; }
			set { attributes = (ushort) value; }
		}

		public MethodDefinition AddMethod {
			get {
				if (add_method != null)
					return add_method;

				InitializeMethods ();
				return add_method;
			}
			set { add_method = value; }
		}

		public MethodDefinition InvokeMethod {
			get {
				if (invoke_method != null)
					return invoke_method;

				InitializeMethods ();
				return invoke_method;
			}
			set { invoke_method = value; }
		}

		public MethodDefinition RemoveMethod {
			get {
				if (remove_method != null)
					return remove_method;

				InitializeMethods ();
				return remove_method;
			}
			set { remove_method = value; }
		}

		public bool HasOtherMethods {
			get {
				if (other_methods != null)
					return other_methods.Count > 0;

				InitializeMethods ();
				return !other_methods.IsNullOrEmpty ();
			}
		}

		public Collection<MethodDefinition> OtherMethods {
			get {
				if (other_methods != null)
					return other_methods;

				InitializeMethods ();

				if (other_methods == null)
					Interlocked.CompareExchange (ref other_methods, new Collection<MethodDefinition> (), null);

				return other_methods;
			}
		}

		public bool HasCustomAttributes {
			get {
				if (custom_attributes != null)
					return custom_attributes.Count > 0;

				return this.GetHasCustomAttributes (Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes {
			get { return custom_attributes ?? (this.GetCustomAttributes (ref custom_attributes, Module)); }
		}

		#region EventAttributes

		public bool IsSpecialName {
			get { return attributes.GetAttributes ((ushort) EventAttributes.SpecialName); }
			set { attributes = attributes.SetAttributes ((ushort) EventAttributes.SpecialName, value); }
		}

		public bool IsRuntimeSpecialName {
			get { return attributes.GetAttributes ((ushort) EventAttributes.RTSpecialName); }
			set { attributes = attributes.SetAttributes ((ushort) EventAttributes.RTSpecialName, value); }
		}

		#endregion

		public new TypeDefinition DeclaringType {
			get { return (TypeDefinition) base.DeclaringType; }
			set { base.DeclaringType = value; }
		}

		public override bool IsDefinition {
			get { return true; }
		}

		public EventDefinition (string name, EventAttributes attributes, TypeReference eventType)
			: base (name, eventType)
		{
			this.attributes = (ushort) attributes;
			this.token = new MetadataToken (TokenType.Event);
		}

		void InitializeMethods ()
		{
			var module = this.Module;
			if (module == null)
				return;

			lock (module.SyncRoot) {
				if (add_method != null
					|| invoke_method != null
					|| remove_method != null)
					return;

				if (!module.HasImage ())
					return;

				module.Read (this, (@event, reader) => reader.ReadMethods (@event));
			}
		}

		public override EventDefinition Resolve ()
		{
			return this;
		}
	}
}
