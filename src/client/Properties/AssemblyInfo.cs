﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General assembly info
[assembly: AssemblyTitle("NPSharp Client Library")]
[assembly: AssemblyDescription(".NET library for communication with the RefInt NP server.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Carl Kittelberger")]
[assembly: AssemblyProduct("NPSharp")]
[assembly: AssemblyCopyright("© 2014-2015 Carl Kittelberger")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// COM
[assembly: ComVisible(false)]
[assembly: Guid("eb49c20b-b649-42fa-bff4-f62aba36e0b0")]

// Version information is generated by GitVersionTask in another file

// Make internals visible to the server counterpart of this library
[assembly: InternalsVisibleTo("npsharp_server")]