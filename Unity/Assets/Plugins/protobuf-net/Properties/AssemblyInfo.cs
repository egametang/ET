using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("protobuf-net")]
[assembly: AssemblyDescription("Protocol Buffers for .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Marc Gravell")]
[assembly: AssemblyProduct("protobuf-net")]
[assembly: AssemblyCopyright("See http://code.google.com/p/protobuf-net/")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if !PORTABLE
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("224e5fc5-09f7-4fe3-a0a3-cf72b9f3593e")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.0.0.668")]
#if !CF
[assembly: AssemblyFileVersion("2.0.0.668")]
#endif
#if !FX11
[assembly: InternalsVisibleTo("protobuf-net.unittest, PublicKey="
    + "002400000480000094000000060200000024000052534131000400000100010009ed9caa457bfc"
    + "205716c3d4e8b255a63ddf71c9e53b1b5f574ab6ffdba11e80ab4b50be9c46d43b75206280070d"
    + "dba67bd4c830f93f0317504a76ba6a48243c36d2590695991164592767a7bbc4453b34694e31e2"
    + "0815a096e4483605139a32a76ec2fef196507487329c12047bf6a68bca8ee9354155f4d01daf6e"
    + "ec5ff6bc")]
#endif

[assembly: CLSCompliant(false)]