using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.Core
{
    public class RegistryManager
    {
        private string manifest = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

        public List<ScopedRegistry> registries
        {
            get; private set;
        }
        
        public CredentialManager credentialManager
        {
            get;
            private set;
        }

        public RegistryManager()
        {
            this.credentialManager = new CredentialManager();
            this.registries = new List<ScopedRegistry>();

            JObject manifestJSON = JObject.Parse(File.ReadAllText(manifest));

            JArray Jregistries = (JArray)manifestJSON["scopedRegistries"];
            if (Jregistries != null)
            {
                foreach (var JRegistry in Jregistries)
                {
                    registries.Add(LoadRegistry((JObject)JRegistry));
                }
            }
            else
            {
                Debug.Log("No scoped registries set");
            }
        }

        private ScopedRegistry LoadRegistry(JObject Jregistry)
        {
            ScopedRegistry registry = new ScopedRegistry();
            registry.name = (string)Jregistry["name"];
            registry.url = (string)Jregistry["url"];

            List<string> scopes = new List<string>();
            foreach (var scope in (JArray)Jregistry["scopes"])
            {
                scopes.Add((string)scope);
            }
            registry.scopes = new List<string>(scopes);

            if (credentialManager.HasRegistry(registry.url))
            {
                NPMCredential credential = credentialManager.GetCredential(registry.url);
                registry.auth = credential.alwaysAuth;
                registry.token = credential.token;
            }

            return registry;
        }

        private void UpdateScope(ScopedRegistry registry, JToken registryElement) {
            JArray scopes = new JArray();
            foreach (var scope in registry.scopes)
            {
                scopes.Add(scope);
            }
            registryElement["scopes"] = scopes;
        }

        private JToken GetOrCreateScopedRegistry(ScopedRegistry registry, JObject manifestJSON)
        {
            JArray Jregistries = (JArray)manifestJSON["scopedRegistries"];
            if (Jregistries == null)
            {
                Jregistries = new JArray();
                manifestJSON["scopedRegistries"] = Jregistries;
            }

            foreach (var JRegistryElement in Jregistries)
            {
                if (JRegistryElement["name"] != null && JRegistryElement["url"] != null &&
                    String.Equals(JRegistryElement["name"].Value<string>(), registry.name, StringComparison.Ordinal) &&
                    String.Equals(JRegistryElement["url" ].Value<string>(), registry.url, StringComparison.Ordinal))
                {
                    UpdateScope(registry, JRegistryElement);
                    return JRegistryElement;
                }
            }

            JObject JRegistry = new JObject();
            JRegistry["name"] = registry.name;
            JRegistry["url"] = registry.url;
            UpdateScope(registry, JRegistry);
            Jregistries.Add(JRegistry);

            return JRegistry;
        }

        public void Remove(ScopedRegistry registry)
        {
            JObject manifestJSON = JObject.Parse(File.ReadAllText(manifest));
            JArray Jregistries = (JArray)manifestJSON["scopedRegistries"];

            foreach (var JRegistryElement in Jregistries)
            {
                if (JRegistryElement["name"] != null && JRegistryElement["url"] != null &&
                JRegistryElement["name"].Value<string>().Equals(registry.name, StringComparison.Ordinal) &&
                JRegistryElement["url" ].Value<string>().Equals(registry.url, StringComparison.Ordinal))
                {
                    JRegistryElement.Remove();
                    break;
                }
            }

            write(manifestJSON);
        }

        public void Save(ScopedRegistry registry)
        {
            JObject manifestJSON = JObject.Parse(File.ReadAllText(manifest));

            JToken manifestRegistry = GetOrCreateScopedRegistry(registry, manifestJSON);

            if(!string.IsNullOrEmpty(registry.token))
            {
                credentialManager.SetCredential(registry.url, registry.auth, registry.token);
            }
            else
            {
                credentialManager.RemoveCredential(registry.url);
            }
            
            write(manifestJSON);

            credentialManager.Write();
        }

        private void write(JObject manifestJSON)
        {
            File.WriteAllText(manifest, manifestJSON.ToString());
            AssetDatabase.Refresh();
        }
    }
}