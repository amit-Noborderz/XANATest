using NPC;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PhotonLocalPlayer") && !other.GetComponent<NpcChatBillboard>() && other.GetComponent<PhotonView>().IsMine || GameplayEntityLoader.instance.isLocalPlayer)
        {
           other.GetComponentInParent<SwimmingController>()?.StartSwimming();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("PhotonLocalPlayer") && !other.GetComponent<NpcChatBillboard>() && other.GetComponent<PhotonView>().IsMine) || GameplayEntityLoader.instance.isLocalPlayer)
        {
           other.GetComponentInParent<SwimmingController>()?.StopSwimming();
        }
    }


}
