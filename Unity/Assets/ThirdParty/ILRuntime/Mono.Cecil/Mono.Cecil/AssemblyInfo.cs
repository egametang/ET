//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle (Consts.AssemblyName)]

#if !NET_CORE
[assembly: Guid ("fd225bb4-fa53-44b2-a6db-85f5e48dcb54")]
#endif

[assembly: InternalsVisibleTo ("Mono.Cecil.Pdb, PublicKey=" + Consts.PublicKey)]
[assembly: InternalsVisibleTo ("Mono.Cecil.Mdb, PublicKey=" + Consts.PublicKey)]
[assembly: InternalsVisibleTo ("Mono.Cecil.Rocks, PublicKey=" + Consts.PublicKey)]
[assembly: InternalsVisibleTo ("Mono.Cecil.Tests, PublicKey=" + Consts.PublicKey)]
