using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IOS_Screen_Switcher : MonoBehaviour
{
    public MeshFilter m_filter;
    public Mesh iosScreen;
    public Mesh androidScreen;

    private void OnEnable()
    {
        BuilderEventManager.ChangeScreenWithPlayerType += SetScreen;
    }

    private void OnDisable()
    {
        BuilderEventManager.ChangeScreenWithPlayerType -= SetScreen;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Application.isEditor && WorldItemView.m_EnvName.Contains("D_Infinity_Labo"))
        {
            m_filter.transform.localRotation = Quaternion.Euler(0f, 0.426f, 180f);
        }
#if UNITY_ANDROID
        m_filter.mesh = androidScreen;
#elif UNITY_IOS
           m_filter.mesh = iosScreen;
#endif
    }

    void SetScreen(bool youtubeVideo, bool awsVideo)
    {
        if (youtubeVideo)
        {
#if UNITY_ANDROID
            m_filter.mesh = androidScreen;
#elif UNITY_IOS
           m_filter.mesh = iosScreen;
#endif
        }
        else
        {
#if UNITY_ANDROID
            m_filter.mesh = androidScreen;
#elif UNITY_IOS
           m_filter.mesh = androidScreen;
#endif
        }

    }
}
