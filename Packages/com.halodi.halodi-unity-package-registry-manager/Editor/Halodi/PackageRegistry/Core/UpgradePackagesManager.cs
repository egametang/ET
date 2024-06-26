using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Threading;
using System.Collections.Generic;
using Artees.UnitySemVer;
using System;

namespace Halodi.PackageRegistry.Core
{
    public class UpgradePackagesManager
    {

        public class PackageUpgradeState
        {
            public PackageUpgradeState(UnityEditor.PackageManager.PackageInfo info)
            {
                this.info = info;
                previewAvailable = false;
                stableAvailable = false;
                verifiedAvailable = false;
                hasVerified = false;
                stableVersion = SemVer.Parse(info.version);
                previewVersion = SemVer.Parse(info.version);
                


                try
                {
                    current = SemVer.Parse(info.version);
                }
                catch
                {
                    Debug.LogError("Cannot parse version for package " + info.displayName + ": " + info.version);
                }

                if (info.source == PackageSource.Git)
                {

                    previewAvailable = true;
                    preview = info.packageId;

                    stableAvailable = true;
                    stable = info.packageId;
                }
                else if (info.source == PackageSource.Registry)
                {

                    string[] compatible = info.versions.compatible;

                    foreach (string ver in compatible)
                    {
                        try
                        {
                            SemVer version = SemVer.Parse(ver);

                            if (string.IsNullOrWhiteSpace(version.preRelease))
                            {
                                if (version > stableVersion)
                                {
                                    stableVersion = version;
                                    stableAvailable = true;
                                    stable = info.name + "@" + ver;
                                }
                            }
                            else
                            {
                                // This is a pre-release
                                if (version > previewVersion)
                                {
                                    previewVersion = version;
                                    previewAvailable = true;
                                    preview = info.name + "@" + ver;
                                }

                            }
                        }
                        catch
                        {
                            Debug.LogError("Invalid version for package " + info.displayName + ": " + ver);
                        }
                    }
                    
#if UNITY_2022_2_OR_NEWER
                    string verified = info.versions.recommended;
#else
                    string verified = info.versions.verified;
#endif
                    
                    hasVerified = !String.IsNullOrWhiteSpace(verified);
                    
                    if(hasVerified)
                    {
                        try
                        {
                            verifiedVersion = SemVer.Parse(verified);
                            if(verifiedVersion > current)
                            {
                                verifiedAvailable = verifiedVersion > current;
                                verified = info.name + "@" + verified;
                            }
                            
                        }
                        catch
                        {
                            Debug.LogError("Cannot parse version for package " + info.displayName + ": " + verified);
                        }
                    }
                }
            }

            internal string GetCurrentVersion()
            {
                return info.packageId;
            }

            public UnityEditor.PackageManager.PackageInfo info;

            private SemVer current;

            private bool previewAvailable;
            private SemVer previewVersion;

            private string preview;

            private bool stableAvailable;
            private SemVer stableVersion;

            private string stable;

            private bool hasVerified;
            private bool verifiedAvailable;
            private SemVer verifiedVersion;
            private string verified;

            public bool HasNewVersion(bool showPreviewVersion, bool useVerified)
            {
                if(useVerified && hasVerified)
                {
                    return verifiedAvailable;
                }
                else if (showPreviewVersion)
                {
                    return previewAvailable || stableAvailable;

                }
                else
                {
                    return stableAvailable;
                }
            }

            public string GetNewestVersion(bool showPreviewVersion, bool useVerified)
            {
                if(useVerified && hasVerified)
                {
                    if(verifiedAvailable)
                    {
                        return verified;
                    }
                }
                else if (showPreviewVersion)
                {
                    if (previewAvailable)
                    {
                        if (!stableAvailable || previewVersion > stableVersion)
                        {
                            return preview;
                        }
                    }
                }
                if (stableAvailable)
                {
                    if (stableAvailable)
                    {
                        return stable;
                    }
                }

                return null;
            }
        }

        public List<PackageUpgradeState> UpgradeablePackages = new List<PackageUpgradeState>();

        private ListRequest request;

        public bool packagesLoaded = false;

        public UpgradePackagesManager()
        {
#if UNITY_2019_1_OR_NEWER
            request = Client.List(false, false);
#else
            request = Client.List();
#endif
        }

        public void Update()
        {
            if (!packagesLoaded && request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                {
                    PackageCollection collection = request.Result;
                    foreach (UnityEditor.PackageManager.PackageInfo info in collection)
                    {
                        UpgradeablePackages.Add(new PackageUpgradeState(info));
                    }
                }
                else
                {
                    Debug.LogError("Cannot query package manager for packages");
                }

                packagesLoaded = true;
            }
        }


        public bool UpgradePackage(String packageWithVersion, ref string error)
        {
            AddRequest request = UnityEditor.PackageManager.Client.Add(packageWithVersion);

            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (request.Status == StatusCode.Success)
            {
                return true;
            }
            else
            {
                error = request.Error.message;
                return false;
            }


        }


    }
}
