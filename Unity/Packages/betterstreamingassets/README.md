# Better Streaming Assets

Better Streaming Assets is a plugin that lets you access Streaming Assets directly in an uniform and thread-safe way, with tiny overhead. Mostly beneficial for Android projects, where the alternatives are to use archaic and hugely inefficient WWW or embed data in Asset Bundles. API is based on Syste.IO.File and System.IO.Directory classes.

# Note on Android & App Bundles

App Bundles (.aab) builds are bugged when it comes to Streaming Assets. See https://github.com/gwiazdorrr/BetterStreamingAssets/issues/10 for details. The bottom line is:

⚠️ **Keep all file names in Streaming Assets lowercase!** ⚠️

Also, based on local tests with Unity 2020.3, using non-ASCII characters may result in a Streaming Assets file being compressed if one of the following is true:
- extension contains non-ASCII characters
- the file is extension-less, but contains non-ASCII characters in its path

⚠️ **Do not use non-ASCII characters in file names** ⚠️

# Getting started

This plugin can be installed in following ways:
* Select "Add package from git URL..." in the Unity Package Manager and use this URL: `https://github.com/gwiazdorrr/BetterStreamingAssets.git`
* Clone this repository and copy `Runtime` directory to your project.
* Download the latest release from the [Asset Store](https://assetstore.unity.com/packages/tools/input-management/better-streaming-assets-103788).

# Usage

Check examples below. Note that all the paths are relative to StreamingAssets directory. That is, if you have files

```
<project>/Assets/StreamingAssets/foo.bar
<project>/Assets/StreamingAssets/dir/foo.bar
````

You are expected to use following paths:

```
foo.bar (or /foo.bar)
dir/foo.bar (or /dir/foo.bar)
```

# Examples

Initialization (before first use, needs to be called on main thread):

```csharp
BetterStreamingAssets.Initialize();
```

Typical scenario, deserializing from Xml:

```csharp
public static Foo ReadFromXml(string path)
{
    if ( !BetterStreamingAssets.FileExists(path) )
    {
        Debug.LogErrorFormat("Streaming asset not found: {0}", path);
        return null;
    }

    using ( var stream = BetterStreamingAssets.OpenRead(path) )
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Foo));
        return (Foo)serializer.Deserialize(stream);
    }
}
```

Note that ReadFromXml can be called from any thread, as long as Foo's constructor doesn't make any UnityEngine calls.

Listing all Streaming Assets in with .xml extension:

```csharp
// all the xmls
string[] paths = BetterStreamingAssets.GetFiles("\\", "*.xml", SearchOption.AllDirectories); 
// just xmls in Config directory (and nested)
string[] paths = BetterStreamingAssets.GetFiles("Config", "*.xml", SearchOption.AllDirectories); 
```

Checking if a directory exists:

```csharp
Debug.Assert( BetterStreamingAssets.DirectoryExists("Config") );
```

Ways of reading a file:

```csharp
// all at once
byte[] data = BetterStreamingAssets.ReadAllBytes("Foo/bar.data");

// as stream, last 10 bytes
byte[] footer = new byte[10];
using (var stream = BetterStreamingAssets.OpenRead("Foo/bar.data"))
{
    stream.Seek(-footer.Length, SeekOrigin.End);
    stream.Read(footer, 0, footer.Length);
}
```
    
Asset bundles (again, main thread only):

```csharp
// synchronous
var bundle = BetterStreamingAssets.LoadAssetBundle(path);
// async
var bundleOp = BetterStreamingAssets.LoadAssetBundleAsync(path);
```

# (Android) False-positive compressed Streaming Assets messages

Streaming Assets end up in the same part of APK as files added by many custom plugins (`assets` directory), so it is impossible to tell whether a compressed file is a Streaming Asset (an indication something has gone terribly wrong) or not. This tool acts conservatively and logs errors whenever it finds a compressed file inside of `assets`, but outside of `assets/bin`. If you are annoyed by this and are certain a compressed file was not meant to be a Streaming Asset, add a file like this in the same assembly as Better Streaming Assets:

```csharp
partial class BetterStreamingAssets
{
    static partial void AndroidIsCompressedFileStreamingAsset(string path, ref bool result)
    {
        if ( path == "assets/my_custom_plugin_settings.json")
        {
            result = false;
        }
    }
}
```
