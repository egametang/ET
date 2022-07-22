using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateBUISettingsObject")]
public class BUISettingsObject : ScriptableObject
{
    public string templatePath;
    public string outputFolder;
}
