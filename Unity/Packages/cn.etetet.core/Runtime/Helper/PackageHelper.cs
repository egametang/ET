using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public class PackagesLock
    {
        [BsonIgnore]
        public string unityDir;
        
        public Dictionary<string, PackageInfo> dependencies = new();
    }
    
    public class PackageInfo
    {
        [BsonIgnore]
        public string name;
        [BsonIgnore]
        public string module;
        
        public string version;
        public int depth;
        public string source;
        public string hash;
        public string url;
        public string dir;
        public Dictionary<string, string> dependencies = new();
    }
    
    public static class PackageHelper
    {
        public static PackagesLock LoadEtPackagesLock(string unityDir)
        {
            string s = File.ReadAllText(Path.Combine(unityDir, "Packages/packages-lock.json"));

            PackagesLock packagesLock = MongoHelper.FromJson<PackagesLock>(s);
            packagesLock.unityDir = unityDir;
            
            foreach (string k in packagesLock.dependencies.Keys.ToArray())
            {
                if (!k.StartsWith("cn.etetet"))
                {
                    packagesLock.dependencies.Remove(k);
                    continue;
                }

                PackageInfo packageInfo = packagesLock.dependencies[k];

                packageInfo.name = k;
                packageInfo.module = k.Replace("cn.etetet.", "");
                
                if (packageInfo.source == "embedded")
                {
                    packageInfo.dir = Path.Combine(unityDir, "Packages", packageInfo.version.Replace("file:", ""));
                }
                else if (packageInfo.source == "git")
                {
                    string p1 = Path.Combine(unityDir, "Library/PackageCache", k + "@" + packageInfo.hash.Substring(0, 10));
                    string p2 = Path.Combine(unityDir, "Packages", k + "@" + packageInfo.hash);
                    if (Directory.Exists(p1))
                    {
                        packageInfo.dir = p1;
                    }
                    else if (Directory.Exists(p2))
                    {
                        packageInfo.dir = p2;
                    }
                    else
                    {
                        throw new Exception($"not found package: {p1} {p2}");
                    }
                }
                else
                {
                    packageInfo.dir = Path.Combine(unityDir, "Library/PackageCache", k + "@" + packageInfo.version);
                }
            }
            
            return packagesLock;
        }
    }
}