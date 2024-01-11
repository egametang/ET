using System;
using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
    public interface ILocalizationParamsManager
    {
        string GetParameterValue( string Param );
    }

    public class LocalizationParamsManager : MonoBehaviour, ILocalizationParamsManager
	{
        [Serializable]
        public struct ParamValue
        {
            public string Name, Value;
        
        }

        [SerializeField]
        public List<ParamValue> _Params = new List<ParamValue>();

        public bool _IsGlobalManager;
        
        public string GetParameterValue( string ParamName )
        {
            if (_Params != null)
            {
                for (int i = 0, imax = _Params.Count; i < imax; ++i)
                    if (_Params[i].Name == ParamName)
                        return _Params[i].Value;
            }
            return null; // not found
        }

        public void SetParameterValue( string ParamName, string ParamValue, bool localize = true )
        {
            bool setted = false;
            for (int i = 0, imax = _Params.Count; i < imax; ++i)
                if (_Params[i].Name == ParamName)
                {
                    var temp = _Params[i];
                    temp.Value = ParamValue;
                    _Params[i] = temp;
                    setted = true;
                    break;
                }
            if (!setted)
                _Params.Add(new ParamValue { Name = ParamName, Value = ParamValue });
        
			if (localize)
				OnLocalize();
		}
		
		public void OnLocalize()
		{
            var loc = GetComponent<Localize>();
            if (loc != null)
                loc.OnLocalize(true);
        }

        public virtual void OnEnable()
        {
            if (_IsGlobalManager)
                DoAutoRegister();
        }

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //public void AutoStart()
        //{
        //    if (_AutoRegister)
        //        DoAutoRegister();
        //}

        public void DoAutoRegister()
        {
            if (!LocalizationManager.ParamManagers.Contains(this))
            {
                LocalizationManager.ParamManagers.Add(this);
                LocalizationManager.LocalizeAll(true);
            }
        }

        public void OnDisable()
        {
            LocalizationManager.ParamManagers.Remove(this);
        }
    }
}