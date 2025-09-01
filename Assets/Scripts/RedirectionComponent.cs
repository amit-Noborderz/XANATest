using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedirectionComponent : MonoBehaviour
{
    public int ReDirectionIndex;
    public string Url;
    [Tooltip("This is not required added for special cases")]
    public string Msg;

    public bool ClickRedirection;
    public bool PlayerTriggerRedirection;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PhotonLocalPlayer" && other.GetComponent<PhotonView>().IsMine && PlayerTriggerRedirection)
        {
            BuilderEventManager.OpenRedirectionPopup?.Invoke(ReDirectionIndex,Url,Msg);
        }
    }


    private void OnMouseDown()
    {
        if (ClickRedirection)
        {
            BuilderEventManager.OpenRedirectionPopup?.Invoke(ReDirectionIndex, Url, Msg);
        }
    }
}
