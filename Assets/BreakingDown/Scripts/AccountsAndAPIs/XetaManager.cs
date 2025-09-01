using TMPro;
using UnityEngine;

namespace BD
{
    public class XetaManager : MonoBehaviour
    {
        private static TextMeshProUGUI _xetaText;
        private static int _currentXeta = -1;
        //public static bool hasMinimumFuel;
        public static int CurrentXeta
        {
            get
            {
                if (_currentXeta == -1)
                {
                    Debug.LogWarning("Xeta has not been set from the server. Using debug value...");
                    _currentXeta = 100;
                }
                return _currentXeta;
            }
            set
            {
                _currentXeta = value;
                if (_xetaText != null)
                {
                    _xetaText.text = _currentXeta.ToString();
                }
            }
        }

        private void OnEnable()
        {
            if (_xetaText == null)
            {
                _xetaText = GetComponent<TextMeshProUGUI>();
            }

            _xetaText.text = CurrentXeta.ToString();
        }

        private void OnDisable()
        {
            _xetaText = null;
        }
    }
}