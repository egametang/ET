using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.PackageManager.Requests;

namespace Hibzz.DependencyResolver
{
    [BsonIgnoreExtraElements]
    public class PackageGitDependency
    {
        public Dictionary<string, string> gitDependencies;
    }
    
    [InitializeOnLoad]
    public class DependencyResolver
    {
        static AddAndRemoveRequest packageInstallationRequest;

        // called by the attribute [InitializeOnLoad]
        static DependencyResolver()
        {
            Events.registeredPackages += OnPackagesRegistered;
        }

        // Invoked when the package manager completes registering new packages
        static void OnPackagesRegistered(PackageRegistrationEventArgs packageRegistrationInfo)
        {
            // stores all the dependencies that needs to be installed in this step
            Dictionary<string, string> dependencies = new ();

            // loop through all of the added packages and get their git
            // dependencies and add it to the list that contains all the
            // dependencies that need to be installed
            foreach (var package in packageRegistrationInfo.added)
            {
                // get the dependencies of the added package
                if (!GetDependencies(package, out PackageGitDependency package_dependencies))
                {
                    continue;
                }

                foreach (var gitDependency in package_dependencies.gitDependencies)
                {
                    dependencies[gitDependency.Key] = gitDependency.Value;
                }
            }

            // remove any dependencies that's already installed
            var installed_packages = PackageInfo.GetAllRegisteredPackages().ToList();
            foreach (string dependency in dependencies.Keys.ToArray())
            {
                if (IsInCollection(dependency, installed_packages))
                {
                    dependencies.Remove(dependency);
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
        static bool GetDependencies(PackageInfo packageInfo, out PackageGitDependency dependencies)
        {
            // Read the contents of the package.json file
            string package_json_path = $"{packageInfo.resolvedPath}/package.json";
            string package_json_content = File.ReadAllText(package_json_path);

            PackageGitDependency packageGitDependency = BsonSerializer.Deserialize<PackageGitDependency>(package_json_content);
            // if no token with the key git-dependecies is found, failed to get git dependencies
            if (packageGitDependency.gitDependencies is null || packageGitDependency.gitDependencies.Count == 0)
            {
                dependencies = null;
                return false;
            }

            // convert the git dependency token to a list of strings...
            // maybe we should check for errors in this process? what if git-dependency isn't array of string?
            dependencies = packageGitDependency;
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
            if (collection == null) { return false; }

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
        /// <param name="dependencies">A list of dependencies to install</param>
        static void InstallDependencies(Dictionary<string, string> dependencies)
        {
            // there are no dependencies to install, skip
            if (dependencies == null || dependencies.Count <= 0)
            {
                return;
            }

            // before installing the packages, make sure that user knows what
            // the dependencies to install are... additionally, check if the
            // application is being run on batch mode so that we can skip the
            // installation dialog
            if (!Application.isBatchMode &&
                !EditorUtility.DisplayDialog(
                    $"Dependency Resolver",
                    $"The following dependencies are required: \n\n{GetPrintFriendlyName(dependencies.Keys.ToList())}",
                    "Install Dependencies",
                    "Cancel"))
            {
                // user decided to cancel the installation of the dependencies...
                return;
            }

            // the user pressed install, perform the actual installation
            // (or the application was in batch mode)
            packageInstallationRequest = Client.AddAndRemove(dependencies.Values.ToArray(), null);

            // show the progress bar till the installation is complete
            EditorUtility.DisplayProgressBar("Dependency Resolver", "Preparing installation of dependencies...", 0);
            EditorApplication.update += DisplayProgress;
        }

        /// <summary>
        /// Get a print friendly name of all dependencies to show in the dialog box
        /// </summary>
        /// <param name="dependencies">The list of dependencies to parse through</param>
        /// <returns>A print friendly string representing all the dependencies</returns>
        static string GetPrintFriendlyName(List<string> dependencies)
        {
            // ideally, we want the package name, but that requires downloading the package.json and parsing through
            // it, which is kinda too much... i could ask for the users to give a package name along with the url in
            // package.json, but again too complicated just for a dialog message... username/repo will do fine for now

            string result = string.Join("\n", dependencies);    // concatenate dependencies on a new line

            return result;
        }

        /// <summary>
        /// Shows a progress bar till the AddAndRemoveRequest is completed
        /// </summary>
        static void DisplayProgress()
        {
            if(packageInstallationRequest.IsCompleted)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= DisplayProgress;
            }
        }
    }
}
