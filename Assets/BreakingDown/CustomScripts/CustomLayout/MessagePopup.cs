using UnityEngine;
using UnityEngine.UI;

namespace BD
{
    public class MessagePopup : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text message;

        public void ShowMessage(string title, string message)
        {
            this.title.text = title;
            this.message.text = message;
            gameObject.SetActive(true);
        }
    }
}