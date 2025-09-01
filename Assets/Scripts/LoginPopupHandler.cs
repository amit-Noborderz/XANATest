using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginPopupHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public static LoginPopupHandler Instance;
    public GameObject popup;

    public void EnablePopup()
    {
        popup.SetActive(true);
    }
    private void Awake()
    {
        Instance = this;
    }

}
