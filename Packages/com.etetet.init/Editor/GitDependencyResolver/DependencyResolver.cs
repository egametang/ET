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
        static void MoveToPackage()
        {
            Process process = ProcessHelper.PowerShell($"-NoExit -ExecutionPolicy Bypass -File ./Packages/com.etetet.init/MoveToPackages.ps1", waitExit: true);
            Debug.Log(process.StandardOutput.ReadToEnd());
            AssetDatabase.Refresh();
        }
        
        static AddAndRemoveRequest packageInstallationRequest;

        static DependencyResolver()
        {
            Events.registeredPackages += OnPackagesRegistered;
        }

        // Invoked when the package manager completes registering new packages
        static void OnPackagesRegistered(PackageRegistrationEventArgs packageRegistrationInfo)
        {
            if (packageRegistrationInfo.added.Count == 0)
            {
                return;
            }
            
            Debug.Log($"Packages Registered: {string.Join(" ", packageRegistrationInfo.added.Select(x=>x.name))}");
            
            // loop through all of the added packages and get their git
            // dependencies and add it to the list that contains all the
            // dependencies that need to be installed
            Dictionary<string, string> dependencies = new();
            List<PackageInfo> installedPackages = PackageInfo.GetAllRegisteredPackages().ToList();
            foreach (var package in packageRegistrationInfo.added)
            {
                if (!package.name.StartsWith("cn.etetet."))
                {
                    continue;
                }
                
                // get the dependencies of the added package
                if (!GetDependencies(package, out PackageGit packageDependencies))
                {
                    continue;
                }
                
                foreach (var gitDependency in packageDependencies.GitDependencies)
                {
                    if (IsInCollection(gitDependency.Key, installedPackages))
                    {
                        continue;
                    }
                    dependencies[gitDependency.Key] = gitDependency.Value;
                }
            }

            // Install the dependencies
            InstallDependencies(dependencies);
        }

        /// <summary>
        /// Request a list of git dependencies in the package
        /// </summary>
        /// <param name="packageInfo">The package to get the git dependencies from</param>
        /// <param name="dependencies">The retrieved list of git dependencies </param>
        /// <returns>Was the request successful?</returns>
        static bool GetDependencies(PackageInfo packageInfo, out PackageGit dependencies)
        {
            string packageJsonPath = $"{packageInfo.resolvedPath}/packagegit.json";

            if (!File.Exists(packageJsonPath))
            {
                throw new Exception($"package already move to packages dir, please refresh your unity project!  RepairDependencies retry please! {packageInfo.name} {packageJsonPath}");
            }
            
            dependencies = PackageGitHelper.Load(packageJsonPath);
            
            if (dependencies.GitDependencies is null || dependencies.GitDependencies.Count == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Is the given dependency url found in the given collection
        /// </summary>
        /// <param name="dependency">The url the dependency to check for</param>
        /// <param name="collection">The collection to look through</param>
        /// <returns></returns>
        static bool IsInCollection(string dependency, List<PackageInfo> collection)
        {
            // when package collection given is null, it's inferred that the dependency is not in the collection
            if (collection == null)
            {
                return false;
            }

            // check if any of the installed package has the dependency
            foreach (var package in collection)
            {
                // the package id for a package installed with git is `package_name@package_giturl`
                // get the repo url by performing some string manipulation on the package id
                //string repourl = package.packageId.Substring(package.packageId.IndexOf('@') + 1);

                // Found!
                if (package.name == dependency)
                {
                    return true;
                }
            }

            // the dependency wasn't found in the package collection
            return false;
        }

        /// <summary>
        /// Install all the given dependencies
        /// </summary>
        static void InstallDependencies(Dictionary<string, string> dependencies)
        {
            if (dependencies.Count == 0)
            {
                MoveToPackage();
                
                Debug.Log($"git Dependencies are all installed");
                return;
            }
            
            // before installing the packages, make sure that user knows what
            // the dependencies to install are... additionally, check if the
            // application is being run on batch mode so that we can skip the
            // installation dialog
            Debug.Log($"The following dependencies are required:\n{string.Join("\n", dependencies.Keys)}");

            // the user pressed install, perform the actual installation
            // (or the application was in batch mode)
            packageInstallationRequest = Client.AddAndRemove(dependencies.Values.ToArray());

            // show the progress bar till the installation is complete
            EditorUtility.DisplayProgressBar("Dependency Resolver", "Preparing installation of dependencies...", 0);
            EditorApplication.update += DisplayProgress;
        }


        /// <summary>
        /// Shows a progress bar till the AddAndRemoveRequest is completed
        /// </summary>
        static void DisplayProgress()
        {
            if (!packageInstallationRequest.IsCompleted)
            {
                return;
            }
            
            EditorUtility.ClearProgressBar();
            EditorApplication.update -= DisplayProgress;
        }
        
        [MenuItem("ET/Init/RepairDependencies")]
        static void RepairDependencies()
        {
            MoveToPackage();
            
            Dictionary<string, string> dependencies = new();
            List<PackageInfo> installedPackages = PackageInfo.GetAllRegisteredPackages().ToList();
            
            foreach (var package in installedPackages)
            {
                if (!package.name.StartsWith("cn.etetet."))
                {
                    continue;
                }
                
                if (!GetDependencies(package, out PackageGit packageDependencies))
                {
                    continue;
                }
                
                foreach (var gitDependency in packageDependencies.GitDependencies)
                {
                    if (IsInCollection(gitDependency.Key, installedPackages))
                    {
                        continue;
                    }
                    
                    if (dependencies.TryGetValue(gitDependency.Key, out string findV))
                    {
                        if (findV != gitDependency.Value)
                        {
                            Debug.Log($"package dup {gitDependency.Key} but git url diff: {findV} {gitDependency.Value}");
                        }
                        
                        continue;
                    }
                    
                    Debug.Log($"Dependency not found: {gitDependency.Key}");
                    dependencies.Add(gitDependency.Key, gitDependency.Value);
                }
            }

            if (dependencies.Count == 0)
            {
                Debug.Log($"git Dependencies are all installed");
                return;
            }
            
            InstallDependencies(dependencies);
        }
    }
}
