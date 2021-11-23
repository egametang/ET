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
using ILRuntime.Mono.Collections.Generic;
using SR = System.Reflection;

using ILRuntime.Mono.Cecil.Metadata;

namespace ILRuntime.Mono.Cecil {

	public interface IMetadataImporterProvider {
		IMetadataImporter GetMetadataImporter (ModuleDefinition module);
	}

	public interface IMetadataImporter {
		AssemblyNameReference ImportReference (AssemblyNameReference reference);
		TypeReference ImportReference (TypeReference type, IGenericParameterProvider context);
		FieldReference ImportReference (FieldReference field, IGenericParameterProvider context);
		MethodReference ImportReference (MethodReference method, IGenericParameterProvider context);
	}

	public interface IReflectionImporterProvider {
		IReflectionImporter GetReflectionImporter (ModuleDefinition module);
	}

	public interface IReflectionImporter {
		AssemblyNameReference ImportReference (SR.AssemblyName reference);
		TypeReference ImportReference (Type type, IGenericParameterProvider context);
		FieldReference ImportReference (SR.FieldInfo field, IGenericParameterProvider context);
		MethodReference ImportReference (SR.MethodBase method, IGenericParameterProvider context);
	}

	struct ImportGenericContext {

		Collection<IGenericParameterProvider> stack;

		public bool IsEmpty { get { return stack == null; } }

		public ImportGenericContext (IGenericParameterProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			stack = null;

			Push (provider);
		}

		public void Push (IGenericParameterProvider provider)
		{
			if (stack == null)
				stack = new Collection<IGenericParameterProvider> (1) { provider };
			else
				stack.Add (provider);
		}

		public void Pop ()
		{
			stack.RemoveAt (stack.Count - 1);
		}

		public TypeReference MethodParameter (string method, int position)
		{
			for (int i = stack.Count - 1; i >= 0; i--) {
				var candidate = stack [i] as MethodReference;
				if (candidate == null)
					continue;

				if (method != NormalizeMethodName (candidate))
					continue;

				return candidate.GenericParameters [position];
			}

			throw new InvalidOperationException ();
		}

		public string NormalizeMethodName (MethodReference method)
		{
			return method.DeclaringType.GetElementType ().FullName + "." + method.Name;
		}

		public TypeReference TypeParameter (string type, int position)
		{
			for (int i = stack.Count - 1; i >= 0; i--) {
				var candidate = GenericTypeFor (stack [i]);

				if (candidate.FullName != type)
					continue;

				return candidate.GenericParameters [position];
			}

			throw new InvalidOperationException ();
		}

		static TypeReference GenericTypeFor (IGenericParameterProvider context)
		{
			var type = context as TypeReference;
			if (type != null)
				return type.GetElementType ();

			var method = context as MethodReference;
			if (method != null)
				return method.DeclaringType.GetElementType ();

			throw new InvalidOperationException ();
		}

		public static ImportGenericContext For (IGenericParameterProvider context)
		{
			return context != null ? new ImportGenericContext (context) : default (ImportGenericContext);
		}
	}

	public class DefaultReflectionImporter : IReflectionImporter {

		readonly protected ModuleDefinition module;

		public DefaultReflectionImporter (ModuleDefinition module)
		{
			Mixin.CheckModule (module);

			this.module = module;
		}

		enum ImportGenericKind {
			Definition,
			Open,
		}

		static readonly Dictionary<Type, ElementType> type_etype_mapping = new Dictionary<Type, ElementType> (18) {
			{ typeof (void), ElementType.Void },
			{ typeof (bool), ElementType.Boolean },
			{ typeof (char), ElementType.Char },
			{ typeof (sbyte), ElementType.I1 },
			{ typeof (byte), ElementType.U1 },
			{ typeof (short), ElementType.I2 },
			{ typeof (ushort), ElementType.U2 },
			{ typeof (int), ElementType.I4 },
			{ typeof (uint), ElementType.U4 },
			{ typeof (long), ElementType.I8 },
			{ typeof (ulong), ElementType.U8 },
			{ typeof (float), ElementType.R4 },
			{ typeof (double), ElementType.R8 },
			{ typeof (string), ElementType.String },
			{ typeof (TypedReference), ElementType.TypedByRef },
			{ typeof (IntPtr), ElementType.I },
			{ typeof (UIntPtr), ElementType.U },
			{ typeof (object), ElementType.Object },
		};

		TypeReference ImportType (Type type, ImportGenericContext context)
		{
			return ImportType (type, context, ImportGenericKind.Open);
		}

		TypeReference ImportType (Type type, ImportGenericContext context, ImportGenericKind import_kind)
		{
			if (IsTypeSpecification (type) || ImportOpenGenericType (type, import_kind))
				return ImportTypeSpecification (type, context);

			var reference = new TypeReference (
				string.Empty,
				type.Name,
				module,
				ImportScope (type),
				type.IsValueType);

			reference.etype = ImportElementType (type);

			if (IsNestedType (type))
				reference.DeclaringType = ImportType (type.DeclaringType, context, import_kind);
			else
				reference.Namespace = type.Namespace ?? string.Empty;

			if (type.IsGenericType)
				ImportGenericParameters (reference, type.GetGenericArguments ());

			return reference;
		}

		protected virtual IMetadataScope ImportScope (Type type)
		{
			return ImportScope (type.Assembly);
		}

		static bool ImportOpenGenericType (Type type, ImportGenericKind import_kind)
		{
			return type.IsGenericType && type.IsGenericTypeDefinition && import_kind == ImportGenericKind.Open;
		}

		static bool ImportOpenGenericMethod (SR.MethodBase method, ImportGenericKind import_kind)
		{
			return method.IsGenericMethod && method.IsGenericMethodDefinition && import_kind == ImportGenericKind.Open;
		}

		static bool IsNestedType (Type type)
		{
			return type.IsNested;
		}

		TypeReference ImportTypeSpecification (Type type, ImportGenericContext context)
		{
			if (type.IsByRef)
				return new ByReferenceType (ImportType (type.GetElementType (), context));

			if (type.IsPointer)
				return new PointerType (ImportType (type.GetElementType (), context));

			if (type.IsArray)
				return new ArrayType (ImportType (type.GetElementType (), context), type.GetArrayRank ());

			if (type.IsGenericType)
				return ImportGenericInstance (type, context);

			if (type.IsGenericParameter)
				return ImportGenericParameter (type, context);

			throw new NotSupportedException (type.FullName);
		}

		static TypeReference ImportGenericParameter (Type type, ImportGenericContext context)
		{
			if (context.IsEmpty)
				throw new InvalidOperationException ();

			if (type.DeclaringMethod != null)
				return context.MethodParameter (NormalizeMethodName (type.DeclaringMethod), type.GenericParameterPosition);

			if (type.DeclaringType != null)
				return context.TypeParameter (NormalizeTypeFullName (type.DeclaringType), type.GenericParameterPosition);

			throw new InvalidOperationException();
		}

		static string NormalizeMethodName (SR.MethodBase method)
		{
			return NormalizeTypeFullName (method.DeclaringType) + "." + method.Name;
		}

		static string NormalizeTypeFullName (Type type)
		{
			if (IsNestedType (type))
				return NormalizeTypeFullName (type.DeclaringType) + "/" + type.Name;

			return type.FullName;
		}

		TypeReference ImportGenericInstance (Type type, ImportGenericContext context)
		{
			var element_type = ImportType (type.GetGenericTypeDefinition (), context, ImportGenericKind.Definition);
			var arguments = type.GetGenericArguments ();
			var instance = new GenericInstanceType (element_type, arguments.Length);
			var instance_arguments = instance.GenericArguments;

			context.Push (element_type);
			try {
				for (int i = 0; i < arguments.Length; i++)
					instance_arguments.Add (ImportType (arguments [i], context));

				return instance;
			} finally {
				context.Pop ();
			}
		}

		static bool IsTypeSpecification (Type type)
		{
			return type.HasElementType
				|| IsGenericInstance (type)
				|| type.IsGenericParameter;
		}

		static bool IsGenericInstance (Type type)
		{
			return type.IsGenericType && !type.IsGenericTypeDefinition;
		}

		static ElementType ImportElementType (Type type)
		{
			ElementType etype;
			if (!type_etype_mapping.TryGetValue (type, out etype))
				return ElementType.None;

			return etype;
		}

		protected AssemblyNameReference ImportScope (SR.Assembly assembly)
		{
			return ImportReference (assembly.GetName ());
		}

		public virtual AssemblyNameReference ImportReference (SR.AssemblyName name)
		{
			Mixin.CheckName (name);

			AssemblyNameReference reference;
			if (TryGetAssemblyNameReference (name, out reference))
				return reference;

			reference = new AssemblyNameReference (name.Name, name.Version)
			{
				PublicKeyToken = name.GetPublicKeyToken (),
				Culture = name.CultureInfo.Name,
				HashAlgorithm = (AssemblyHashAlgorithm) name.HashAlgorithm,
			};

			module.AssemblyReferences.Add (reference);

			return reference;
		}

		bool TryGetAssemblyNameReference (SR.AssemblyName name, out AssemblyNameReference assembly_reference)
		{
			var references = module.AssemblyReferences;

			for (int i = 0; i < references.Count; i++) {
				var reference = references [i];
				if (name.FullName != reference.FullName) // TODO compare field by field
					continue;

				assembly_reference = reference;
				return true;
			}

			assembly_reference = null;
			return false;
		}

		FieldReference ImportField (SR.FieldInfo field, ImportGenericContext context)
		{
			var declaring_type = ImportType (field.DeclaringType, context);

			if (IsGenericInstance (field.DeclaringType))
				field = ResolveFieldDefinition (field);

			context.Push (declaring_type);
			try {
				return new FieldReference {
					Name = field.Name,
					DeclaringType = declaring_type,
					FieldType = ImportType (field.FieldType, context),
				};
			} finally {
				context.Pop ();
			}
		}

		static SR.FieldInfo ResolveFieldDefinition (SR.FieldInfo field)
		{
			return field.Module.ResolveField (field.MetadataToken);
		}

		static SR.MethodBase ResolveMethodDefinition (SR.MethodBase method)
		{
			return method.Module.ResolveMethod (method.MetadataToken);
		}

		MethodReference ImportMethod (SR.MethodBase method, ImportGenericContext context, ImportGenericKind import_kind)
		{
			if (IsMethodSpecification (method) || ImportOpenGenericMethod (method, import_kind))
				return ImportMethodSpecification (method, context);

			var declaring_type = ImportType (method.DeclaringType, context);

			if (IsGenericInstance (method.DeclaringType))
				method = ResolveMethodDefinition (method);

			var reference = new MethodReference {
				Name = method.Name,
				HasThis = HasCallingConvention (method, SR.CallingConventions.HasThis),
				ExplicitThis = HasCallingConvention (method, SR.CallingConventions.ExplicitThis),
				DeclaringType = ImportType (method.DeclaringType, context, ImportGenericKind.Definition),
			};

			if (HasCallingConvention (method, SR.CallingConventions.VarArgs))
				reference.CallingConvention &= MethodCallingConvention.VarArg;

			if (method.IsGenericMethod)
				ImportGenericParameters (reference, method.GetGenericArguments ());

			context.Push (reference);
			try {
				var method_info = method as SR.MethodInfo;
				reference.ReturnType = method_info != null
					? ImportType (method_info.ReturnType, context)
					: ImportType (typeof (void), default (ImportGenericContext));

				var parameters = method.GetParameters ();
				var reference_parameters = reference.Parameters;

				for (int i = 0; i < parameters.Length; i++)
					reference_parameters.Add (
						new ParameterDefinition (ImportType (parameters [i].ParameterType, context)));

				reference.DeclaringType = declaring_type;

				return reference;
			} finally {
				context.Pop ();
			}
		}

		static void ImportGenericParameters (IGenericParameterProvider provider, Type [] arguments)
		{
			var provider_parameters = provider.GenericParameters;

			for (int i = 0; i < arguments.Length; i++)
				provider_parameters.Add (new GenericParameter (arguments [i].Name, provider));
		}

		static bool IsMethodSpecification (SR.MethodBase method)
		{
			return method.IsGenericMethod && !method.IsGenericMethodDefinition;
		}

		MethodReference ImportMethodSpecification (SR.MethodBase method, ImportGenericContext context)
		{
			var method_info = method as SR.MethodInfo;
			if (method_info == null)
				throw new InvalidOperationException ();

			var element_method = ImportMethod (method_info.GetGenericMethodDefinition (), context, ImportGenericKind.Definition);
			var instance = new GenericInstanceMethod (element_method);
			var arguments = method.GetGenericArguments ();
			var instance_arguments = instance.GenericArguments;

			context.Push (element_method);
			try {
				for (int i = 0; i < arguments.Length; i++)
					instance_arguments.Add (ImportType (arguments [i], context));

				return instance;
			} finally {
				context.Pop ();
			}
		}

		static bool HasCallingConvention (SR.MethodBase method, SR.CallingConventions conventions)
		{
			return (method.CallingConvention & conventions) != 0;
		}

		public virtual TypeReference ImportReference (Type type, IGenericParameterProvider context)
		{
			Mixin.CheckType (type);
			return ImportType (
				type,
				ImportGenericContext.For (context),
				context != null ? ImportGenericKind.Open : ImportGenericKind.Definition);
		}

		public virtual FieldReference ImportReference (SR.FieldInfo field, IGenericParameterProvider context)
		{
			Mixin.CheckField (field);
			return ImportField (field, ImportGenericContext.For (context));
		}

		public virtual MethodReference ImportReference (SR.MethodBase method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod (method);
			return ImportMethod (method,
				ImportGenericContext.For (context),
				context != null ? ImportGenericKind.Open : ImportGenericKind.Definition);
		}
	}

	public class DefaultMetadataImporter : IMetadataImporter {

		readonly protected ModuleDefinition module;

		public DefaultMetadataImporter (ModuleDefinition module)
		{
			Mixin.CheckModule (module);

			this.module = module;
		}

		TypeReference ImportType (TypeReference type, ImportGenericContext context)
		{
			if (type.IsTypeSpecification ())
				return ImportTypeSpecification (type, context);

			var reference = new TypeReference (
				type.Namespace,
				type.Name,
				module,
				ImportScope (type),
				type.IsValueType);

			MetadataSystem.TryProcessPrimitiveTypeReference (reference);

			if (type.IsNested)
				reference.DeclaringType = ImportType (type.DeclaringType, context);

			if (type.HasGenericParameters)
				ImportGenericParameters (reference, type);

			return reference;
		}

		protected virtual IMetadataScope ImportScope (TypeReference type)
		{
			return ImportScope (type.Scope);
		}

		protected IMetadataScope ImportScope (IMetadataScope scope)
		{
			switch (scope.MetadataScopeType) {
			case MetadataScopeType.AssemblyNameReference:
				return ImportReference ((AssemblyNameReference) scope);
			case MetadataScopeType.ModuleDefinition:
				if (scope == module) return scope;
				return ImportReference (((ModuleDefinition) scope).Assembly.Name);
			case MetadataScopeType.ModuleReference:
				throw new NotImplementedException ();
			}

			throw new NotSupportedException ();
		}

		public virtual AssemblyNameReference ImportReference (AssemblyNameReference name)
		{
			Mixin.CheckName (name);

			AssemblyNameReference reference;
			if (module.TryGetAssemblyNameReference (name, out reference))
				return reference;

			reference = new AssemblyNameReference (name.Name, name.Version) {
				Culture = name.Culture,
				HashAlgorithm = name.HashAlgorithm,
				IsRetargetable = name.IsRetargetable,
				IsWindowsRuntime = name.IsWindowsRuntime,
			};

			var pk_token = !name.PublicKeyToken.IsNullOrEmpty ()
				? new byte [name.PublicKeyToken.Length]
				: Empty<byte>.Array;

			if (pk_token.Length > 0)
				Buffer.BlockCopy (name.PublicKeyToken, 0, pk_token, 0, pk_token.Length);

			reference.PublicKeyToken = pk_token;

			module.AssemblyReferences.Add (reference);

			return reference;
		}

		static void ImportGenericParameters (IGenericParameterProvider imported, IGenericParameterProvider original)
		{
			var parameters = original.GenericParameters;
			var imported_parameters = imported.GenericParameters;

			for (int i = 0; i < parameters.Count; i++)
				imported_parameters.Add (new GenericParameter (parameters [i].Name, imported));
		}

		TypeReference ImportTypeSpecification (TypeReference type, ImportGenericContext context)
		{
			switch (type.etype) {
			case ElementType.SzArray:
				var vector = (ArrayType) type;
				return new ArrayType (ImportType (vector.ElementType, context));
			case ElementType.Ptr:
				var pointer = (PointerType) type;
				return new PointerType (ImportType (pointer.ElementType, context));
			case ElementType.ByRef:
				var byref = (ByReferenceType) type;
				return new ByReferenceType (ImportType (byref.ElementType, context));
			case ElementType.Pinned:
				var pinned = (PinnedType) type;
				return new PinnedType (ImportType (pinned.ElementType, context));
			case ElementType.Sentinel:
				var sentinel = (SentinelType) type;
				return new SentinelType (ImportType (sentinel.ElementType, context));
			case ElementType.FnPtr:
				var fnptr = (FunctionPointerType) type;
				var imported_fnptr = new FunctionPointerType () {
					HasThis = fnptr.HasThis,
					ExplicitThis = fnptr.ExplicitThis,
					CallingConvention = fnptr.CallingConvention,
					ReturnType = ImportType (fnptr.ReturnType, context),
				};

				if (!fnptr.HasParameters)
					return imported_fnptr;

				for (int i = 0; i < fnptr.Parameters.Count; i++)
					imported_fnptr.Parameters.Add (new ParameterDefinition (
						ImportType (fnptr.Parameters [i].ParameterType, context)));

				return imported_fnptr;
			case ElementType.CModOpt:
				var modopt = (OptionalModifierType) type;
				return new OptionalModifierType (
					ImportType (modopt.ModifierType, context),
					ImportType (modopt.ElementType, context));
			case ElementType.CModReqD:
				var modreq = (RequiredModifierType) type;
				return new RequiredModifierType (
					ImportType (modreq.ModifierType, context),
					ImportType (modreq.ElementType, context));
			case ElementType.Array:
				var array = (ArrayType) type;
				var imported_array = new ArrayType (ImportType (array.ElementType, context));
				if (array.IsVector)
					return imported_array;

				var dimensions = array.Dimensions;
				var imported_dimensions = imported_array.Dimensions;

				imported_dimensions.Clear ();

				for (int i = 0; i < dimensions.Count; i++) {
					var dimension = dimensions [i];

					imported_dimensions.Add (new ArrayDimension (dimension.LowerBound, dimension.UpperBound));
				}

				return imported_array;
			case ElementType.GenericInst:
				var instance = (GenericInstanceType) type;
				var element_type = ImportType (instance.ElementType, context);
				var arguments = instance.GenericArguments;
				var imported_instance = new GenericInstanceType (element_type, arguments.Count);
				var imported_arguments = imported_instance.GenericArguments;

				for (int i = 0; i < arguments.Count; i++)
					imported_arguments.Add (ImportType (arguments [i], context));

				return imported_instance;
			case ElementType.Var:
				var var_parameter = (GenericParameter) type;
				if (var_parameter.DeclaringType == null)
					throw new InvalidOperationException ();
				return context.TypeParameter (var_parameter.DeclaringType.FullName, var_parameter.Position);
			case ElementType.MVar:
				var mvar_parameter = (GenericParameter) type;
				if (mvar_parameter.DeclaringMethod == null)
					throw new InvalidOperationException ();
				return context.MethodParameter (context.NormalizeMethodName (mvar_parameter.DeclaringMethod), mvar_parameter.Position);
			}

			throw new NotSupportedException (type.etype.ToString ());
		}

		FieldReference ImportField (FieldReference field, ImportGenericContext context)
		{
			var declaring_type = ImportType (field.DeclaringType, context);

			context.Push (declaring_type);
			try {
				return new FieldReference {
					Name = field.Name,
					DeclaringType = declaring_type,
					FieldType = ImportType (field.FieldType, context),
				};
			} finally {
				context.Pop ();
			}
		}

		MethodReference ImportMethod (MethodReference method, ImportGenericContext context)
		{
			if (method.IsGenericInstance)
				return ImportMethodSpecification (method, context);

			var declaring_type = ImportType (method.DeclaringType, context);

			var reference = new MethodReference {
				Name = method.Name,
				HasThis = method.HasThis,
				ExplicitThis = method.ExplicitThis,
				DeclaringType = declaring_type,
				CallingConvention = method.CallingConvention,
			};

			if (method.HasGenericParameters)
				ImportGenericParameters (reference, method);

			context.Push (reference);
			try {
				reference.ReturnType = ImportType (method.ReturnType, context);

				if (!method.HasParameters)
					return reference;

				var parameters = method.Parameters;
				var reference_parameters = reference.parameters = new ParameterDefinitionCollection (reference, parameters.Count);
				for (int i = 0; i < parameters.Count; i++)
					reference_parameters.Add (
						new ParameterDefinition (ImportType (parameters [i].ParameterType, context)));

				return reference;
			} finally {
				context.Pop();
			}
		}

		MethodSpecification ImportMethodSpecification (MethodReference method, ImportGenericContext context)
		{
			if (!method.IsGenericInstance)
				throw new NotSupportedException ();

			var instance = (GenericInstanceMethod) method;
			var element_method = ImportMethod (instance.ElementMethod, context);
			var imported_instance = new GenericInstanceMethod (element_method);

			var arguments = instance.GenericArguments;
			var imported_arguments = imported_instance.GenericArguments;

			for (int i = 0; i < arguments.Count; i++)
				imported_arguments.Add (ImportType (arguments [i], context));

			return imported_instance;
		}

		public virtual TypeReference ImportReference (TypeReference type, IGenericParameterProvider context)
		{
			Mixin.CheckType (type);
			return ImportType (type, ImportGenericContext.For (context));
		}

		public virtual FieldReference ImportReference (FieldReference field, IGenericParameterProvider context)
		{
			Mixin.CheckField (field);
			return ImportField (field, ImportGenericContext.For (context));
		}

		public virtual MethodReference ImportReference (MethodReference method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod (method);
			return ImportMethod (method, ImportGenericContext.For (context));
		}
	}

	static partial class Mixin {

		public static void CheckModule (ModuleDefinition module)
		{
			if (module == null)
				throw new ArgumentNullException (Argument.module.ToString ());
		}

		public static bool TryGetAssemblyNameReference (this ModuleDefinition module, AssemblyNameReference name_reference, out AssemblyNameReference assembly_reference)
		{
			var references = module.AssemblyReferences;

			for (int i = 0; i < references.Count; i++) {
				var reference = references [i];
				if (!Equals (name_reference, reference))
					continue;

				assembly_reference = reference;
				return true;
			}

			assembly_reference = null;
			return false;
		}

		static bool Equals (byte [] a, byte [] b)
		{
			if (ReferenceEquals (a, b))
				return true;
			if (a == null)
				return false;
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; i++)
				if (a [i] != b [i])
					return false;
			return true;
		}

		static bool Equals<T> (T a, T b) where T : class, IEquatable<T>
		{
			if (ReferenceEquals (a, b))
				return true;
			if (a == null)
				return false;
			return a.Equals (b);
		}

		static bool Equals (AssemblyNameReference a, AssemblyNameReference b)
		{
			if (ReferenceEquals (a, b))
				return true;
			if (a.Name != b.Name)
				return false;
			if (!Equals (a.Version, b.Version))
				return false;
			if (a.Culture != b.Culture)
				return false;
			if (!Equals (a.PublicKeyToken, b.PublicKeyToken))
				return false;
			return true;
		}
	}
}
