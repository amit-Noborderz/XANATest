using TMPro;
using UnityEngine;

namespace BD
{
    public class MainMenuMessagePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;

        private void OnEnable()
        {
            Canvas canvas = this.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = this.gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = 10;
        }

        internal void Show(string title, string message/*, var messageType*/)
        {
            titleText.text = title;
            messageText.text = message;
            gameObject.SetActive(true);

            // buttons
            //switch (messageType)
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}