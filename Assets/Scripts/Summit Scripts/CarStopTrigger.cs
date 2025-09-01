using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarStopTrigger : MonoBehaviour
{

    private bool StopCar = false;

    public List<GameObject> Players = new List<GameObject>();
    private void OnEnable()
    {
        MutiplayerController.onRespawnPlayer += () => { Players.Clear(); StopCar = false; };
    }
    private async void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PhotonLocalPlayer")
        {
            if (MutiplayerController.instance.isShifting)
            {
                return;
            }

            var summitrpc = other.gameObject.GetComponent<SummitPlayerRPC>();
            if (summitrpc.isInsideCAr)
            {
                return;
            }
            if(other.gameObject.GetComponent<PhotonView>()!=null && other.gameObject.GetComponent<PhotonView>().IsMine)
            {
                ReferencesForGamePlay.instance.playerControllerNew.IsAtCarStop = true;
            }
            if (!Players.Contains(other.gameObject))
                Players.Add(other.gameObject);
            StopCar = true;

        }

        if (other.gameObject.tag == "CAR" && StopCar && (other.GetComponent<SplineFollower>().DriverSeatEmpty || other.GetComponent<SplineFollower>().PasengerSeatEmty))
        {
            if (Players.Count > 1)
                CarNavigationManager.CarNavigationInstance.StopCar(other.gameObject, 4);
            else
                CarNavigationManager.CarNavigationInstance.StopCar(other.gameObject, 2);
            if (PhotonNetwork.IsMasterClient)
            {
                while (Players.Count > 0)
                {
                    if (Players[0] == null)
                    {
                        Players.Remove(Players[0]);
                        continue;
                    }
                    await CarNavigationManager.CarNavigationInstance.TPlayer(other.gameObject, Players[0], this);
                }
            }
        }


    }

    public void Pop()
    {
        if (Players[0].GetComponent<PhotonView>()!=null && Players[0].GetComponent<PhotonView>().IsMine)
        {
            ReferencesForGamePlay.instance.playerControllerNew.IsAtCarStop = false;
        }
        Players.RemoveAt(0);
        if (Players.Count == 0)
            StopCar = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PhotonLocalPlayer")
        {

            var summitrpc = other.gameObject.GetComponent<SummitPlayerRPC>();

            if (summitrpc.isInsideCAr)
            {

                return;
            }

            if (other.gameObject.GetComponent<PhotonView>() != null && other.gameObject.GetComponent<PhotonView>().IsMine)
            {
                ReferencesForGamePlay.instance.playerControllerNew.IsAtCarStop = false;
            }
            Players.Remove(other.gameObject);
            if (Players.Count == 0)
                StopCar = false;

        }
    }

}
