using UnityEngine;

namespace BD
{
    // Enable or disable the lock icon in the character selection screen based on the character's availability.
    public class CharacterSelectionListElementLock : MonoBehaviour
    {
        //private void OnEnable()
        //{
        //    gameObject.SetActive(!GetComponentInParent<UnityEngine.UI.Button>().interactable);
        //}

        private void Start()
        {
            gameObject.SetActive(!GetComponentInParent<UnityEngine.UI.Button>().interactable);
        }
    }
}