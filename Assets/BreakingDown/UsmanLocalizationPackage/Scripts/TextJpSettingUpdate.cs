using TMPro;
using UnityEngine;
using TMPro;

namespace Localization
{
    public class TextJpSettingUpdate : MonoBehaviour
    {
        public float JPTextSpacing = 0;
        public float JPTextSize;
        public TMP_Text localizeTextTMP;

        void Start()
        {
            localizeTextTMP = GetComponent<TMP_Text>();
        }
        void OnEnable()
        {
            EventManager.OnGameLanguageChangedComplete += LocalizeTextText;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void LocalizeTextText()
        {

            if (CustomLocalization.Instance.currentLanguage == "jp" && JPTextSpacing != 0)
            {
                localizeTextTMP.fontSize = 100;
            }
            if (CustomLocalization.Instance.currentLanguage == "jp" && JPTextSize != 0)
            {
                localizeTextTMP.characterSpacing = JPTextSize;
            }

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
