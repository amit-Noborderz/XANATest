using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BD
{

public class BreakingDownCloth : MonoBehaviour
{
    public GameObject character;
    public GameObject shirt;

    public SkinnedMeshRenderer Body;
    BD.Stitcher stitcher;



    
    void Start()
    {
        Debug.Log("YesCalled");
        stitcher = new BD.Stitcher();
            
     //   StartCoroutine(StichItem());
    }

    private void OnEnable()
    {
       // StartCoroutine(StichItem());
    }
    public IEnumerator StichItem()
    {
       while(stitcher==null) yield return null;
        if (shirt)
        {
            StichItem(shirt, character);
        }
       
     
    }

    void StichItem(GameObject item, GameObject applyOn)
    {    
        item = stitcher.Stitch(item, applyOn);
        item.layer = character.layer;
        shirt = item;
    }


}
}
