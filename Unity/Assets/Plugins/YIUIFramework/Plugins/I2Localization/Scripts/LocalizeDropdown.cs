using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
	#if !UNITY_5_0 && !UNITY_5_1
    [AddComponentMenu("I2/Localization/Localize Dropdown")]
	public class LocalizeDropdown : MonoBehaviour
	{
        public List<string> _Terms = new List<string>();
		
		public void Start()
		{
			LocalizationManager.OnLocalizeEvent += OnLocalize;
            OnLocalize();
		}
		
		public void OnDestroy()
		{
			LocalizationManager.OnLocalizeEvent -= OnLocalize;
		}

        void OnEnable()
        {
            if (_Terms.Count == 0)
                FillValues();
            OnLocalize ();
        }
		
		public void OnLocalize()
		{
            if (!enabled || gameObject==null || !gameObject.activeInHierarchy)
                return;

            if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
                return;

            UpdateLocalization();
        }

        void FillValues()
        {
            var _Dropdown = GetComponent<Dropdown>();
            if (_Dropdown == null && I2Utils.IsPlaying())
            {
                #if TextMeshPro
                    FillValuesTMPro();
                #endif
                return;
            }

            foreach (var term in _Dropdown.options)
            {
                _Terms.Add(term.text);
            }
        }

        public void UpdateLocalization()
		{
			var _Dropdown = GetComponent<Dropdown>();
            if (_Dropdown == null)
            {
                #if TextMeshPro
                    UpdateLocalizationTMPro();
                #endif
                return;
            }
			
			_Dropdown.options.Clear();
			foreach (var term in _Terms)
			{
                var translation = LocalizationManager.GetTranslation(term);
				_Dropdown.options.Add( new Dropdown.OptionData( translation ) );
			}
            _Dropdown.RefreshShownValue();
		}

        #if TextMeshPro
        public void UpdateLocalizationTMPro()
        {
            var _Dropdown = GetComponent<TMP_Dropdown>();
            if (_Dropdown == null)
                return;

            _Dropdown.options.Clear();
            foreach (var term in _Terms)
            {
                var translation = LocalizationManager.GetTranslation(term);
                _Dropdown.options.Add(new TMP_Dropdown.OptionData(translation));
            }
            _Dropdown.RefreshShownValue();
        }

        void FillValuesTMPro()
        {
            var _Dropdown = GetComponent<TMP_Dropdown>();
            if (_Dropdown == null)
                return;

            foreach (var term in _Dropdown.options)
            {
                _Terms.Add(term.text);
            }
        }
#endif

    }
#endif
}