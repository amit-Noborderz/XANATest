using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeObjectHandler : MonoBehaviour
{
    public GameObject jpObject;
    public GameObject enObject;

    private void OnEnable()
    {
        if (CustomLocalization.Instance.currentLanguage=="en")
        {
            jpObject.SetActive(false);
            enObject.SetActive(true);
        }
        else if(CustomLocalization.Instance.currentLanguage == "ja")
        {
            jpObject.SetActive(true);
            enObject.SetActive(false);
        }
    }
}
