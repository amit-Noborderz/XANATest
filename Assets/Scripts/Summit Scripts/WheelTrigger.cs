using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrigger : MonoBehaviour
{
    bool waitforresponce = false;
    private void Start()
    {
        LoadingHandler.Instance.EnterWheel += ((bo) => { if (waitforresponce && bo) { waitforresponce = false; ChangeSector(); } });
    }
    private void OnTriggerEnter(Collider other)
    {
        if (MutiplayerController.instance.connectionState != ServerConnectionStates.ConnectedToServer) return;
        if (PhotonNetwork.InRoom)
        {
            string name = (string)PhotonNetwork.CurrentRoom.CustomProperties["Sector"];
            if (other.gameObject.tag == "PhotonLocalPlayer")
            {
                var photonView = other.GetComponent<PhotonView>();
                if (photonView == null)
                {
                    Debug.Log($"PhotonView is missing on {other.name}");
                    return;
                }

                print(other.name + " ~~~~ " + photonView);
                if (photonView.IsMine)
                {
                    var summitPlayerRPC = other.GetComponent<SummitPlayerRPC>();
                    if (summitPlayerRPC == null)
                    {
                        Debug.Log($"SummitPlayerRPC is missing on {other.name}");
                        return;
                    }

                    if (name != "Wheel" && !summitPlayerRPC.isInsideCAr)
                    {
                        waitforresponce = true;
                        LoadingHandler.Instance.showApprovalWheelloading();
                    }
                    else
                    {
                        // other.GetComponent<SummitPlayerRPC>().CheckForExitWheel();
                    }
                }
            }

            if (name == "Wheel")
            {
                if (other.gameObject.tag == "WheelCar")
                {
                    Debug.Log("WheelCar Triggered ");
                    GiantWheelManager.Instance.CheckForWheelCar();
                }
                if(GiantWheelManager.Instance.car == null && GiantWheelManager.Instance.CarAdded)
                {
                    GiantWheelManager.Instance.CarAdded = false;
                }
                if (other.gameObject.tag == "Wheel" && !GiantWheelManager.Instance.CarAdded)
                {
                    GiantWheelManager.Instance.AddCar();
                }
            }
        }
    }
    async void ChangeSector()
    {
        while (MutiplayerController.instance.isShifting)
        {
            await new WaitForSeconds(1f);
        }
        //XANASummitSceneLoading.OnJoinSubItem?.Invoke(false);
        MutiplayerController.instance.Ontriggered("Wheel", true);
        MutiplayerController.instance.disableSector = true;
    }
}
