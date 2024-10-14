# About JetBrains Rider Editor

The JetBrains Rider editor package integrates support for the [JetBrains Rider](https://www.jetbrains.com/rider/) .NET Integrated Development Environment (IDE), into the Unity Editor. This package provides an end-point for Rider to call different Unity APIs and to generate .csproj and .sln files, which Rider uses to implement support for Unity in its [plug-in](https://github.com/JetBrains/resharper-unity). 

This package ensures that IDE features like autocomplete suggestions and flagging dependency conflicts work in Rider. It uses .cproj and .sln files which store information about your project such as:

* Versioning information
* Build files
* Platform requirements
* Web server or database settings

Not all code in Unity is directly visible to code editors, particularly when using packages. This is because packages don’t provide their own .csproj files, and Unity doesn’t create them for installed packages by default. This means that IDE features like autocomplete suggestions and flagging dependency conflicts do not work with code in these packages. The purpose of this package is to produce the .csproj files that make these features possible by default when you use Rider.

## Installation

As of Unity version 2019.2, this package comes as a part of the default Unity installation. If you are updating your project from an older version of Unity, you might need to install this package via the Package Manager.

## Requirements

This version of the JetBrains Rider editor package is compatible with the following versions of the Unity Editor:

* 2019.2.6 or later

To use this package, you must have the following third-party products installed:

* JetBrains Rider version 2019.3 or newer

For more information about the Rider IDE, see the [JetBrains Rider documentation](https://www.jetbrains.com/rider/documentation/).

### Submitting issues

This package is maintained by JetBrains and Unity. Submit issues to the [JetBrains/resharper-unity/issues GitHub page](https://github.com/JetBrains/resharper-unity/issues). Unity intends for this package to become accessible to the public on GitHub in the future.
