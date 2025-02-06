using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityFS
{
    public class ScriptingAssembliesJsonPatcher
    {
        [Serializable]
        private class ScriptingAssemblies
        {
            public List<string> names;
            public List<int> types;
        }

        private string _file;
        ScriptingAssemblies _scriptingAssemblies;

        public void Load(string file)
        {
            _file = file;
            string content = File.ReadAllText(file);
            _scriptingAssemblies = JsonUtility.FromJson<ScriptingAssemblies>(content);
        }

        public void AddScriptingAssemblies(List<string> assemblies)
        {
            foreach (string name in assemblies)
            {
                if (!_scriptingAssemblies.names.Contains(name))
                {
                    _scriptingAssemblies.names.Add(name);
                    _scriptingAssemblies.types.Add(16); // user dll type
                    Debug.Log($"[PatchScriptAssembliesJson] add hotfix assembly:{name} to {_file}");
                }
            }
        }

        public void Save(string jsonFile)
        {
            string content = JsonUtility.ToJson(_scriptingAssemblies);

            File.WriteAllText(jsonFile, content);
        }
    }
}
