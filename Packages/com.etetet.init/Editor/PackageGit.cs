using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [BsonIgnoreExtraElements]
    public class PackageGit
    {
        public int Id;
        public string Name;
        public Dictionary<string, string> GitDependencies;
        public Dictionary<string, string[]> ScriptsReferences;
    }

    public static class PackageGitHelper
    {
        public static PackageGit Load(string packageJsonPath)
        {
            if (!File.Exists(packageJsonPath))
            {
                throw new Exception($"not found packagegit.json: {packageJsonPath}, retry refresh unity!");
            }
            
            string packageJsonContent = File.ReadAllText(packageJsonPath);

            PackageGit packageGit = BsonSerializer.Deserialize<PackageGit>(packageJsonContent);
            return packageGit;
        }
    }
}