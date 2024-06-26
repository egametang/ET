using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Halodi.PackageRegistry.Core;

namespace Halodi.PackageRegistry.NPM
{
    /// <summary>
    /// Helper class to create the JSON data to upload to the package server
    /// </summary>
    internal class PublicationManifest
    {
        private JObject j = new JObject();

        public string name
        {
            get; private set;
        }

        private string base64Data;
        private long size;
        private string sha512;
        private string sha1;

        public string Request
        {
            get
            {
                return j.ToString(Formatting.None);
            }
        }

        internal static JObject LoadManifest(string packageFolder)
        {
            string manifestPath = Path.Combine(packageFolder, "package.json");

            if (!File.Exists(manifestPath))
            {
                throw new System.IO.IOException("Invalid package folder. Cannot find package.json in " + packageFolder);
            }

            JObject manifest = JObject.Parse(File.ReadAllText(manifestPath));

            if (manifest["name"] == null)
            {
                throw new System.IO.IOException("Package name not set");
            }

            if (manifest["version"] == null)
            {
                throw new System.IO.IOException("Package version not set");
            }

            if (manifest["description"] == null)
            {
                throw new System.IO.IOException("Package description not set");
            }

            return manifest;
        }



        internal PublicationManifest(string packageFolder, string registry)
        {

            CreateTarball(packageFolder);

            JObject manifest = LoadManifest(packageFolder);

            name = manifest["name"].ToString();
            string version = manifest["version"].ToString();
            string description = manifest["description"].ToString();

            string tarballName = name + "-" + version + ".tgz";
            string tarballPath = name + "/-/" + tarballName;


            string tarballUri = NPMLogin.UrlCombine(registry, tarballPath);
            tarballUri = Regex.Replace(tarballUri, @"^https:\/\/", "http://");


            string readmeFile = GetReadmeFilename(packageFolder);
            string readme = null;
            if(readmeFile != null)
            {
                readme = GetReadme(readmeFile);
            }




            j["_id"] = name;
            j["name"] = name;
            j["description"] = description;



            j["dist-tags"] = new JObject();
            j["dist-tags"]["latest"] = version;


            j["versions"] = new JObject();
            j["versions"][version] = manifest;

            if (!string.IsNullOrEmpty(readmeFile))
            {
                j["versions"][version]["readme"] = readme;
                j["versions"][version]["readmeFilename"] = readmeFile;
            }

            j["versions"][version]["_id"] = name + "@" + version;


            // Extra options set by the NPM client. Will not set here as they do not seem neccessary.

            // j["versions"][version]["_npmUser"] = new JObject();
            // j["versions"][version]["_npmUser"]["name"] = "";
            // j["versions"][version]["_npmUser"]["email"] = "";
            // j["versions"][version]["_npmVersion"] = "6.14.4";
            // j["versions"][version]["_nodeVersion"] = "12.16.2";

            j["versions"][version]["dist"] = new JObject();
            j["versions"][version]["dist"]["integrity"] = sha512;
            j["versions"][version]["dist"]["shasum"] = sha1;
            j["versions"][version]["dist"]["tarball"] = tarballUri.ToString();

            if (!string.IsNullOrEmpty(readme))
            {
                j["readme"] = readme;
            }

            j["_attachments"] = new JObject();
            j["_attachments"][tarballName] = new JObject();
            j["_attachments"][tarballName]["content_type"] = "application/octet-stream";
            j["_attachments"][tarballName]["length"] = new JValue(size);
            j["_attachments"][tarballName]["data"] = base64Data;

        }

        private string GetReadmeFilename(string packageFolder)
        {
            foreach (var path in Directory.EnumerateFiles(packageFolder))
            {
                string file = Path.GetFileName(path);
                if (file.Equals("readme.md", StringComparison.InvariantCultureIgnoreCase) ||
                file.Equals("readme.txt", StringComparison.InvariantCultureIgnoreCase) ||
                file.Equals("readme", StringComparison.InvariantCultureIgnoreCase))
                {
                    return path;
                }
            }
            return null;
        }

        private string GetReadme(string readmeFile)
        {
            return File.ReadAllText(readmeFile);
        }

        private string SHA512(byte[] data)
        {
            var sha = new SHA512Managed();
            byte[] checksum = sha.ComputeHash(data);
            return "sha512-" + Convert.ToBase64String(checksum);
        }
        private string SHA1(byte[] data)
        {
            var sha = new SHA1Managed();
            byte[] checksum = sha.ComputeHash(data);
            return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
        }



        public void CreateTarball(string packageFolder)
        {
            string folder = FileUtil.GetUniqueTempPathInProject();
            string file = PackageTarball.Create(packageFolder, folder);

            Byte[] bytes = File.ReadAllBytes(file);
            base64Data = Convert.ToBase64String(bytes);
            size = bytes.Length;

            sha1 = SHA1(bytes);
            sha512 = SHA512(bytes);

            File.Delete(file);
            Directory.Delete(folder);

        }


    }
}

