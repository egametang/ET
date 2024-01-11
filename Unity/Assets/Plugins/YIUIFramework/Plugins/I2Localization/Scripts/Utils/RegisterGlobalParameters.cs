using UnityEngine;

namespace I2.Loc
{

    public class RegisterGlobalParameters : MonoBehaviour, ILocalizationParamsManager 
	{
		public virtual void OnEnable()
		{
            if (!LocalizationManager.ParamManagers.Contains(this))
            {
                LocalizationManager.ParamManagers.Add(this);
                LocalizationManager.LocalizeAll(true);
            }
		}

		public virtual void OnDisable()
        {
            LocalizationManager.ParamManagers.Remove(this);
        }

		public virtual string GetParameterValue( string ParamName )
        {
            return null;
        }

	}
}