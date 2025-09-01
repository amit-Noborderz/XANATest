using UnityEngine;

namespace BD
{
    public class ControlsCustomizationMenuButton : MonoBehaviour
    {
        public void OnClick() // Called by Button on this GameObject
        {
            BDButtonsLayoutManager.Instance.gameObject.SetActive(true);
        }
    }
}