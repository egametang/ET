//-----------------------------------------------------------------------
// <copyright file="ValidationRunner.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Validation;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class ValidationRunner : IDisposable
    {
        private Dictionary<Type, PropertyTree> cachedPropertyTrees;

        public void Dispose()
        {
            if (this.cachedPropertyTrees != null)
            {
                foreach (var tree in this.cachedPropertyTrees.Values)
                {
                    tree.Dispose();
                }

                this.cachedPropertyTrees = null;
            }
        }

        public virtual List<ValidationResult> ValidateObjectRecursively(object value)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            this.ValidateObjectRecursively(value, ref results);
            return results;
        }

        public virtual void ValidateObjectRecursively(object value, ref List<ValidationResult> results)
        {
            if (results == null) results = new List<ValidationResult>();

            if (object.ReferenceEquals(value, null))
            {
                results.Add(new ValidationResult()
                {
                    Message = "Root object to scan is null; this could indicate a corrupted asset or a component/asset with a missing script file",
                    ResultType = ValidationResultType.Error,
                    Path = "",
                });

                return;
            }

            if (value is UnityEngine.Object)
            {
                UnityEngine.Object obj = value as UnityEngine.Object;

                if (obj == null)
                {
                    // Asset to scan is null; we should check if it has a missing script.
                    MonoScript script = null;
                    string typeOfThing;

                    if (obj is MonoBehaviour)
                    {
                        script = MonoScript.FromMonoBehaviour(obj as MonoBehaviour);
                        typeOfThing = "MonoBehaviour";
                    }
                    else if (obj is ScriptableObject)
                    {
                        script = MonoScript.FromScriptableObject(obj as ScriptableObject);
                        typeOfThing = "ScriptableObject";
                    }
                    else if (obj is Component)
                    {
                        // It's a component but not a MonoBehaviour; MonoScript apparently doesn't handle that, so, uh...
                        //  this is just a general error case, I suppose?
                        typeOfThing = "Component";
                        script = null;
                    }
                    else
                    {
                        typeOfThing = "UnityEngine.Object";
                        script = null;
                    }

                    if (script == null)
                    {
                        results.Add(new ValidationResult()
                        {
                            Message = typeOfThing + " of type '" + obj.GetType() + "' appears to have a missing script",
                            ResultType = ValidationResultType.Error,
                            Path = "",
                        });
                    }
                    else
                    {
                        results.Add(new ValidationResult()
                        {
                            Message = typeOfThing + " of type '" + obj.GetType() + "' was in a destroyed state while being scanned",
                            ResultType = ValidationResultType.Error,
                            Path = "",
                        });
                    }

                    return;
                }
            }


            PropertyTree tree;

            if (this.cachedPropertyTrees == null)
            {
                this.cachedPropertyTrees = new Dictionary<Type, PropertyTree>(FastTypeComparer.Instance);
            }

            if (!this.cachedPropertyTrees.TryGetValue(value.GetType(), out tree))
            {
                tree = PropertyTree.Create(value).SetUpForValidation();
                this.cachedPropertyTrees.Add(value.GetType(), tree);
            }
            else
            {
                tree.SetTargets(value);
                tree.SetUpForValidation();
            }

            try
            {
                {
                    var root = tree.RootProperty;

                    var validationComponent = root.GetComponent<ValidationComponent>();

                    if (validationComponent != null && root.GetAttribute<DontValidateAttribute>() == null && validationComponent.ValidatorLocator.PotentiallyHasValidatorsFor(root))
                    {
                        validationComponent.ValidateProperty(ref results);
                    }
                }

                foreach (var property in tree.EnumerateTree(true, true))
                {
                    var validationComponent = property.GetComponent<ValidationComponent>();

                    if (validationComponent == null) continue;
                    if (property.GetAttribute<DontValidateAttribute>() != null) continue;
                    if (!validationComponent.ValidatorLocator.PotentiallyHasValidatorsFor(property)) continue;

                    validationComponent.ValidateProperty(ref results);
                }
            }
            finally
            {
                tree.CleanForCachedReuse();
            }
        }

        public virtual List<ValidationResult> ValidateUnityObjectRecursively(UnityEngine.Object value)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            this.ValidateObjectRecursively(value, ref results);
            return results;
        }

        public virtual void ValidateUnityObjectRecursively(UnityEngine.Object value, ref List<ValidationResult> results)
        {
            this.ValidateObjectRecursively(value, ref results);
        }
    }
}
