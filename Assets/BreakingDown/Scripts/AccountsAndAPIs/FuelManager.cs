using TMPro;
using UnityEngine;

namespace BD
{
    public class FuelManager : MonoBehaviour
    {
        private static TextMeshProUGUI _fuelText;
        private static int _currentFuel = -1;
        public static bool hasMinimumFuel;
        public static int CurrentFuel
        {
            get
            {
                if (_currentFuel == -1)
                {
                    return 100;
                }
                else
                {
                    return _currentFuel;
                }
            }
            set
            {
                _currentFuel = value;
                if (_fuelText != null)
                {
                    _fuelText.text = _currentFuel.ToString();
                }
            }
        }

        private void OnEnable()
        {
            if (_fuelText == null)
            {
                _fuelText = GetComponent<TextMeshProUGUI>();
            }

            _fuelText.text = CurrentFuel.ToString();
        }

        private void OnDisable()
        {
            _fuelText = null;
        }
    }
}