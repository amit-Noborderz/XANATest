using BD;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HomeScreenPlayerAnim : MonoBehaviour
{
    public  RuntimeAnimatorController animator;
    public Transform parent;
    // Start is called before the first frame update
  
   public void SetAvatar()
    {
        var P1SelectedChar = FightingGameManager.instance.profiles[ProfileSelector.Instance.CurrentProfile];
        var p1object = Instantiate(P1SelectedChar.characterPrefab, parent); 
        p1object.GetComponent<Animator>().runtimeAnimatorController = animator;
        p1object.layer = 10;
        p1object.transform.localScale = Vector3.one; 
        p1object.transform.localPosition = Vector3.zero;
        foreach (var item in p1object.transform.GetAllChildren())
        {
            item.gameObject.layer = 10;
        }
        
    }
}
