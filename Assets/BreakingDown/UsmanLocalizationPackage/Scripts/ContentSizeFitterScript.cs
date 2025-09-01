using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentSizeFitterScript : MonoBehaviour
{
    void Start()
    {
        
    }
    private void OnEnable()
    {
        Invoke(nameof(SetSize), .01f);
    }
    public void SetSize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
