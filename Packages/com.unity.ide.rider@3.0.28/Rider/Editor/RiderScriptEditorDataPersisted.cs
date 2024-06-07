using System;
using UnityEditor;
using UnityEngine;

namespace Packages.Rider.Editor
{
#if UNITY_2020_1_OR_NEWER // API doesn't exist in 2019.4
  [FilePath("Library/com.unity.ide.rider/PersistedState.asset", FilePathAttribute.Location.ProjectFolder)]
#endif
  internal class RiderScriptEditorPersistedState : ScriptableSingleton<RiderScriptEditorPersistedState>
  {
    [SerializeField] private long lastWriteTicks;
    [SerializeField] private long manifestJsonLastWriteTicks;

    public DateTime? LastWrite
    {
      get => DateTime.FromBinary(lastWriteTicks);
      set
      {
        if (!value.HasValue) return;
        lastWriteTicks = value.Value.ToBinary();
        Save(true);
      }
    }
    
    public DateTime? ManifestJsonLastWrite
    {
      get => DateTime.FromBinary(manifestJsonLastWriteTicks);
      set
      {
        if (!value.HasValue) return;
        manifestJsonLastWriteTicks = value.Value.ToBinary();
        Save(true);
      }
    }
  }
}