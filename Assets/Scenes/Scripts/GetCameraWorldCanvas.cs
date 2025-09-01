using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCameraWorldCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        if (gameObject.GetComponent<Canvas>().renderMode == RenderMode.WorldSpace)
            gameObject.GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<Camera>();
    }
}
