using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ET;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.PackageManager.Requests;
using Debug = UnityEngine.Debug;

namespace Hibzz.DependencyResolver
{
    [InitializeOnLoad]
    public class DependencyResolver
    {
        //[MenuItem("ET/MoveToPackage")]
        static void MoveToPackage(string package, string version)
        {
            string packageName = default;
            string moveFileName = default;
            #if UNITY_6000_0_OR_NEWER
            packageName = package;
            moveFileName = "MoveToPackages_6";
            #else 
            packageName =$"{package}@{version}";
            moveFileName = "MoveToPackages";
            #endif
            string dir = Path.Combine("Library/PackageCache", packageName);
            if (!Directory.Exists(dir))
            {
                return;
            }
        
            Debug.Log($"move package: {packageName}");
            Process process = ProcessHelper.PowerShell($"-NoExit -ExecutionPolicy Bypass -File ./Packages/com.etetet.init/{moveFileName}.ps1 {package} {version}", waitExit: true);
            Debug.Log(process.StandardOutput.ReadToEnd());
        }
        
        static DependencyResolver()
        {
            Events.registeredPackages += OnPackagesRegistered;
        }

        // Invoked when the package manager completes registering new packages
        static void OnPackagesRegistered(PackageRegistrationEventArgs packageRegistrationInfo)
        {
            if (packageRegistrationInfo.added.Count == 0 && packageRegistrationInfo.changedFrom.Count == 0)
            {
                return;
            }
            
            Debug.Log($"Packages Registered: {string.Join(" ", packageRegistrationInfo.added.Select(x=>x.name))}");
            
            // loop through all of the added packages and get their git
            // dependencies and add it to the list that contains all the
            // dependencies that need to be installed
            foreach (var package in packageRegistrationInfo.added)
            {
                if (!package.name.StartsWith("cn.etetet."))
                {
                    continue;
                }
                MoveToPackage(package.name, package.version);
            }
            
            foreach (var package in packageRegistrationInfo.changedFrom)
            {
                if (!package.name.StartsWith("cn.etetet."))
                {
                    continue;
                }
                MoveToPackage(package.name, package.version);
            }
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("ET/Init/RepairDependencies")]
        static void RepairDependencies()
        {
            foreach (var directory in Directory.GetDirectories("Library/PackageCache", "cn.etetet.*"))
            {
                string baseName = Path.GetFileName(directory);
                if (!baseName.StartsWith("cn.etetet."))
                {
                    continue;
                }
                
                string[] ss = baseName.Split("@");
                string packageName = ss[0];
                #if UNITY_6000_0_OR_NEWER
                string version = "";
                #else 
                string version = ss[1];
                #endif

                MoveToPackage(packageName, version);
            }
            
            AssetDatabase.Refresh();
            
            Debug.Log($"repaire package finish");
        }
    }
}
