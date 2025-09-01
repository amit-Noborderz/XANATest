using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class CarNavigationManager : MonoBehaviour
{
    //Public Variables
    public static CarNavigationManager CarNavigationInstance;
    public Action OnExitpress;
    public Action OnCancelPress;
    public Dictionary<int, PhotonView> Cars = new Dictionary<int, PhotonView>();

    //Private variables
    [SerializeField]
    private GameObject CarCanvas;

    private void Awake()
    {
        CarNavigationInstance = this;
    }

    private void Start()
    {
        if (ConstantsHolder.xanaConstants.EnviornmentName == "SaudiExpo")
        {
            SummitEntityManager.instance.InstantiateCAR();
        }
        if (SummitCarUIHandler.SummitCarUIHandlerInstance)
        {
            CarCanvas = SummitCarUIHandler.SummitCarUIHandlerInstance.CarCanvas;
            //SummitCarUIHandler.SummitCarUIHandlerInstance.ExitButton.onClick.RemoveAllListeners();
            //SummitCarUIHandler.SummitCarUIHandlerInstance.ExitButton.onClick.AddListener(ExitCar);
        }
    }

    public void StopCar(GameObject car,int waitSec)
    {

        StartCoroutine(WaitAtCarStop(car,waitSec));


    }
    public async Task TPlayer(GameObject Car, GameObject Players, CarStopTrigger triger)
    {

        //yield return new WaitForSeconds(1.5f);
        await Task.Delay(1500);
        var car = Car.GetComponent<SplineFollower>();
        if(Players.GetComponent<PhotonView>() != null && Players.GetComponent<PhotonView>().IsMine)
            XANASummitSceneLoading.OnJoinSubItem?.Invoke(false);
        if (car.DriverSeatEmpty)
        {
            Players.GetComponent<SummitPlayerRPC>().EnterCar(car.View.ViewID, true);
            triger.Pop();
            car.DriverSeatEmpty = false;
            return;
            //yield break;
        }
        else if (car.PasengerSeatEmty)
        {
            Players.GetComponent<SummitPlayerRPC>().EnterCar(car.View.ViewID, false);
            triger.Pop();
            car.PasengerSeatEmty = false;
            return;
            //yield break;
        }
        //teleport to car pending.....

    }

    IEnumerator WaitAtCarStop(GameObject car,int waitSec)
    {
        yield return new WaitForSeconds(1f);
        car.GetComponent<SplineFollower>().Speed = 0;
        car.GetComponent<SplineFollower>().StopCar = true;
        yield return new WaitForSeconds(waitSec);
        car.GetComponent<SplineFollower>().Speed = 5;
        car.GetComponent<SplineFollower>().StopCar = false;
    }

    public void EnableExitCanvas()
    {

        ReferencesForGamePlay.instance.FullScreenMapStatus(false);
        CarCanvas.SetActive(true);
        if (SummitCarUIHandler.SummitCarUIHandlerInstance)
        {
            SummitCarUIHandler.SummitCarUIHandlerInstance.ExitButton.gameObject.SetActive(true);
            SummitCarUIHandler.SummitCarUIHandlerInstance.ExitButton.onClick.RemoveAllListeners();
            SummitCarUIHandler.SummitCarUIHandlerInstance.ExitButton.onClick.AddListener(ExitCar);
        }
    }
    public void DisableExitCanvas()
    {
        ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<PlayerController>().animator.SetBool("IsEmote", false);
        CarCanvas.SetActive(false);
        if (SummitCarUIHandler.SummitCarUIHandlerInstance)
        {
            SummitCarUIHandler.SummitCarUIHandlerInstance.ExitButton.gameObject.SetActive(false);
        }
    }

    public void ExitCar()
    {
        OnExitpress?.Invoke();

    }
    public void CancelExitCar()
    {
        OnCancelPress?.Invoke();

    }

    public void ExitCar(int carID)
    {
        StartCoroutine(WaitAtCarStop(Cars[carID].gameObject,2));
    }

    public void EnterCar(int id, bool isDriver)
    {

    }
}
