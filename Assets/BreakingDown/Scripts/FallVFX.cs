using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallVFX : MonoBehaviour
{
    public GameObject Fallvfx;
    public Transform vfxpos;

    private void OnTriggerEnter(Collider other)
    {Debug.Log("tRIGGER "+ other.name +"   "+other.tag);
        if (other.CompareTag("ring"))
        {
           var vfx= Instantiate(Fallvfx);
            vfx.transform.position = vfxpos.position;
            Destroy(vfx, 1.5f);
        }
    }
}
