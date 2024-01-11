using System;
using UnityEngine;

namespace YIUIFramework
{
    //生命周期
    public sealed partial class UIBindCDETable
    {
        
        internal Action UIBaseOnEnable;

        private void OnEnable()
        {
            try
            {
                UIBaseOnEnable?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        internal Action UIBaseStart;

        private void Start()
        {
            try
            {
                UIBaseStart?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        internal Action UIBaseOnDisable;

        private void OnDisable()
        {
            try
            {
                UIBaseOnDisable?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        internal Action UIBaseOnDestroy;

        private void OnDestroy()
        {
            try
            {
                UIBaseOnDestroy?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}