using Cinemachine;
using MagicaCloth2;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using Photon.Voice.PUN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SummitPlayerRPC : MonoBehaviour, IInRoomCallbacks
{
    [SerializeField]
    public PhotonView view;
    private PunVoiceClient voiceNetwork;


    public CharacterController charcontroller;
    public ArrowManager arrowManager;
    public PhotonTransformView Transformview;
    public PhotonAnimatorView AnimatorView;
    public PhotonVoiceView VoiceView;

    public Animator animator;
    private CharacterController parentCharacterController;
    private PlayerController parentPlayerController;
    private Camera camera;

    private Transform Parent;
    private bool isdriver;
    private bool StopCar = false;
    private string Group;

    public bool isInsideCAr = false;
    private bool showExit = false;
    int carID;

    private GameplayEntityLoader loader;

    public bool isInsideWheel = false;
    int WheelSeat = -1, MyPlayerPos = 0;
    byte defaultGroup;
    AvatarController avatarController;
    public float _oldLodBias;
    private void Awake()
    {
        isInsideCAr = false;
        _oldLodBias = QualitySettings.lodBias;

        if (view.IsMine)
        {
            loader = GameplayEntityLoader.instance;
            parentCharacterController = loader.mainController.GetComponent<CharacterController>();
            parentPlayerController = loader.mainController.GetComponent<PlayerController>();
            camera = parentPlayerController.firstPersonCameraObj.GetComponent<Camera>();
        }
        avatarController = this.gameObject.GetComponent<AvatarController>();
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (view.IsMine && CarNavigationManager.CarNavigationInstance != null)
        {
            CarNavigationManager.CarNavigationInstance.OnExitpress += Exit;
        }
    }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        if (view.IsMine && CarNavigationManager.CarNavigationInstance != null)
        {
            CarNavigationManager.CarNavigationInstance.OnExitpress -= Exit;
        }
    }
    // Start is called before the first frame update
    public void ExitCar()
    {
        view.RPC(nameof(ExitCAr), RpcTarget.All);
        GameplayEntityLoader.instance.IsJoinSummitWorld = false;
    }

    private void FixedUpdate()
    {

        if (StopCar && CarNavigationManager.CarNavigationInstance)
        {

            PhotonView carview;
            CarNavigationManager.CarNavigationInstance.Cars.TryGetValue(carID, out carview);
            if (carview == null)
            {
                return;
            }

            StopCar = false;
            var car = carview.gameObject.GetComponent<SplineFollower>();


            if (view.IsMine)
            {
                ConstantsHolder.DisableFppRotation = true;
                if (GameplayEntityLoader.instance._uiReferences.Onfreecam.gameObject.activeInHierarchy)
                {
                    GameplayEntityLoader.instance._uiReferences.Onfreecam.onClick.Invoke();
                    GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = false;
                    GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = false;
                }
                else
                {
                    GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = false;
                    GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = false;
                }
            }


            if (isdriver)
            {

                car.DriverSeatEmpty = false;
                //GamePlayUIHandler.inst.CloseAnimationButtonClick();
                GamePlayUIHandler.inst.AnimationBtnClose.SetActive(false);
                //PlayerSelfieController.Instance.DisableSelfieFeature();

                if (view.IsMine)
                {
                    ConstantsHolder.TempDiasableMultiPartPhoton = true;
                    parentCharacterController.enabled = false;
                    parentPlayerController.enabled = false;
                    charcontroller.enabled = false;
                    arrowManager.enabled = false;
                    Transformview.enabled = false;


                    Parent = loader.mainPlayer.transform;
                    loader.mainController.transform.parent = car.transform;
                    transform.localPosition = Vector3.zero;
                    loader.mainController.transform.localPosition = car.DriverPosition.transform.localPosition;


                    PlayerCameraController.instance.EnableCameraRecenter();
                    CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                    SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false,false);


                    transform.rotation = new Quaternion(0, 0, 0, 0);
                    loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);


                    if (voiceNetwork == null) { voiceNetwork = PunVoiceClient.Instance; }
                    Debug.Log("RoomChanger " + voiceNetwork.Client.OpChangeGroups(new byte[] { defaultGroup }, new byte[] { car.PrivateRoomName }));


                    CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                    SummitMiniMapStatusOnSceneChange(false);
                    if (parentPlayerController.isFirstPerson)
                        GamePlayButtonEvents.inst.OnSwitchCameraClick();
                }
                else
                {
                    Transformview.enabled = false;
                    charcontroller.enabled = false;
                    arrowManager.enabled = false;


                    Parent = transform.parent;
                    GameObject gasme = new GameObject();
                    gasme.transform.parent = car.transform;
                    transform.parent = gasme.transform;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = new Quaternion(0, 0, 0, 0);
                    gasme.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                    gasme.transform.localPosition = car.DriverPosition.transform.localPosition;
                    gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
                }
                car.PlayerListinCar.Add(view.Owner, 0);
                //transform.position = car.DriverPos;
                if (gameObject.name.Contains("XanaAvatar2.0_Female"))
                {
                    car._isDriverMale = false;
                }
                if (!car._isPassengerMale && car._isDriverMale && !car.PasengerSeatEmty && !car.DriverSeatEmpty)
                {
                    car.showLove();
                }
                else if (car._isPassengerMale && !car._isDriverMale && !car.PasengerSeatEmty && car.DriverSeatEmpty)
                {
                    car.showLove();
                }
                animator.SetTrigger("EnterCar");
            }
            else
            {
                car.PasengerSeatEmty = false;
                if (view.IsMine)
                {
                    ConstantsHolder.TempDiasableMultiPartPhoton = true;
                    parentCharacterController.enabled = false;
                    parentPlayerController.enabled = false;
                    charcontroller.enabled = false;
                    arrowManager.enabled = false;
                    Transformview.enabled = false;
                    PlayerCameraController.instance.EnableCameraRecenter();
                    Parent = loader.mainPlayer.transform;
                    loader.mainController.transform.parent = car.transform;
                    transform.localPosition = Vector3.zero;
                    loader.mainController.transform.localPosition = car.PacengerPosition.transform.localPosition;
                    CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                    SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false,false);
                    transform.rotation = new Quaternion(0, 0, 0, 0);
                    loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);

                    CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;

                    if (voiceNetwork == null) { voiceNetwork = PunVoiceClient.Instance; }
                    Debug.Log("RoomChanger " + voiceNetwork.Client.OpChangeGroups(new byte[] { defaultGroup }, new byte[] { car.PrivateRoomName }));
                    SummitMiniMapStatusOnSceneChange(false);
                    if (parentPlayerController.isFirstPerson)
                        GamePlayButtonEvents.inst.OnSwitchCameraClick();
                }
                else
                {
                    charcontroller.enabled = false;
                    arrowManager.enabled = false;
                    Transformview.enabled = false;


                    Parent = transform.parent;
                    GameObject gasme = new GameObject();
                    gasme.transform.parent = car.transform;
                    transform.parent = gasme.transform;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = new Quaternion(0, 0, 0, 0);
                    gasme.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                    gasme.transform.localPosition = car.PacengerPosition.transform.localPosition;
                    gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
                }
                car.PlayerListinCar.Add(view.Owner, 1);
                animator.SetTrigger("EnterCar");
                if (gameObject.name.Contains("XanaAvatar2.0_Female"))
                {
                    car._isPassengerMale = false;
                }
                if (!car._isPassengerMale && car._isDriverMale && !car.PasengerSeatEmty && !car.DriverSeatEmpty)
                {
                    car.showLove();
                }
                else if (car._isPassengerMale && !car._isDriverMale && !car.PasengerSeatEmty && !car.DriverSeatEmpty)
                {
                    car.showLove();
                }


            }
            SetClothEnabled(false);
            isInsideCAr = true;
        }

    }

    [PunRPC]
    void ExitCAr()
    {

        CarNavigationManager.CarNavigationInstance.ExitCar(carID);
        StartCoroutine(exitCoolDown());

    }
    IEnumerator exitCoolDown()
    {
        yield return new WaitForSeconds(2f);
        var car = CarNavigationManager.CarNavigationInstance.Cars[carID].gameObject.GetComponent<SplineFollower>();
        animator.SetTrigger("ExitCar");
        showExit = false;

        if (isdriver)
        {

            car.DriverSeatEmpty = true;
            if (view.IsMine)
            {

                ConstantsHolder.TempDiasableMultiPartPhoton = false;
                CarNavigationManager.CarNavigationInstance.DisableExitCanvas();
                SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(true,false);
                loader.mainController.transform.parent = Parent;
                loader.mainController.transform.position = car.DriverExitPosition.transform.position;
                PlayerCameraController.instance.DisableCameraRecenter();
                parentCharacterController.enabled = true;
                parentPlayerController.enabled = true;
                charcontroller.enabled = true;
                arrowManager.enabled = true;
                Transformview.enabled = true;
                SummitMiniMapStatusOnSceneChange(true);
                if (voiceNetwork == null) { voiceNetwork = PunVoiceClient.Instance; }

                Debug.Log("RoomChanger " + voiceNetwork.Client.OpChangeGroups(new byte[] { car.PrivateRoomName }, new byte[] { defaultGroup }));

            }
            else
            {
                transform.parent = loader.mainPlayer.transform.transform.parent;
                transform.position = car.DriverExitPosition.transform.position;
                transform.localScale = Vector3.one * 1.14f;
                charcontroller.enabled = true;
                arrowManager.enabled = true;
                Transformview.enabled = true;

            }
            car.PlayerListinCar.Remove(view.Owner);
            //transform.position = car.DriverPos;
            car.hidelove();
            SetClothEnabled(true);
        }
        else
        {
            car.PasengerSeatEmty = true;
            if (view.IsMine)
            {

                ConstantsHolder.TempDiasableMultiPartPhoton = false;
                CarNavigationManager.CarNavigationInstance.DisableExitCanvas();
                SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(true, false);
                loader.mainController.transform.parent = Parent;
                loader.mainController.transform.position = car.PassengerExitPosition.transform.position;
                parentCharacterController.enabled = true;
                parentPlayerController.enabled = true;
                charcontroller.enabled = true;
                arrowManager.enabled = true;
                Transformview.enabled = true;
                PlayerCameraController.instance.DisableCameraRecenter();
                if (voiceNetwork == null) { voiceNetwork = PunVoiceClient.Instance; }
                SummitMiniMapStatusOnSceneChange(true);

            }
            else
            {
                transform.parent = loader.mainPlayer.transform.transform.parent; ;
                transform.position = car.PassengerExitPosition.transform.position;
                transform.localScale = Vector3.one * 1.14f;
                charcontroller.enabled = true;
                arrowManager.enabled = true;
                Transformview.enabled = true;

            }
            car.PlayerListinCar.Remove(view.Owner);

            car.hidelove();
        }



        yield return new WaitForSeconds(2);
        if (view.IsMine)
        {
            ConstantsHolder.DisableFppRotation = false;


            GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = true;
            GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = true;

        }
        isInsideCAr = false;
    }

    public void EnterCar(int id, bool isDriver)
    {
        view.RPC(nameof(EnterCAr), RpcTarget.All, id, isDriver);
    }

    [PunRPC]
    void EnterCAr(int id, bool isDriver)
    {
        carID = id;
        this.isdriver = isDriver;
        StopCar = true;
        if (view.IsMine)
        {
            if(PlayerSelfieController.Instance.m_IsSelfieFeatureActive)
                PlayerSelfieController.Instance.DisableSelfieFeature();
            GameplayEntityLoader.instance.PlayerCamera.GetComponent<CinemachineCollider>().m_MinimumDistanceFromTarget = 1.5f; //Set the Minimum Distance of Camera on Car Enterance
        }
        //   WaitforInstance(id, isDriver);

    }



    public async Task WaitforInstance(int id, bool isDriver)
    {
        while (!CarNavigationManager.CarNavigationInstance || CarNavigationManager.CarNavigationInstance.Cars.Count < 8)
        {
            await new WaitForSeconds(1f);
        }
    }


    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (ConstantsHolder.xanaConstants.EnviornmentName != "SaudiExpo")
        {
            return;
        }
        if (isInsideCAr && view.IsMine)
        {
            view.RPC(nameof(EnterCAr), newPlayer, carID, isdriver);
        }

        if (isInsideWheel && view.IsMine)
        {
            view.RPC(nameof(EnterWheelCar), newPlayer, MyPlayerPos);
        }
    }
    private IEnumerator Start()
    {
        while (XanaVoiceChat.instance.recorder == null)
        {
            yield return null; // Wait for the next frame
        }
        defaultGroup = XanaVoiceChat.instance.recorder.InterestGroup;
        loader = GameplayEntityLoader.instance;
        if (ConstantsHolder.xanaConstants.EnviornmentName != "SaudiExpo") { yield break; }
        string name = PhotonNetwork.CurrentRoom.CustomProperties["Sector"].ToString();
        if (name == "Wheel" && !isInsideWheel && !AvatarSpawnerOnDisconnect.Instance.IsRjoining)
        {

            getcar();
        }
    }

    async void getcar()
    {
        while (GiantWheelManager.Instance.car == null)
        {
            await new WaitForSeconds(1);
        }

        if (view.IsMine)
        {
            CallEnterWheelRPC();
        }
        /*  else
          {
              waitForWheel();
          }*/


    }


    /*  [PunRPC]
      public void EnterWheel(int wheelSeat)
      {
          string name = PhotonNetwork.CurrentRoom.CustomProperties["Sector"].ToString();
          if (name == "Wheel")
          {

                  ConstantsHolder.DisableFppRotation = true;
                  isInsideCAr = true;
                  if (wheelSeat==1)
                  {

                      var car = GiantWheelManager.Instance.car;
                      car.isfirstPlayerEmpty = false;
                      WheelSeat = 1;
                      car.PlayerSeat.Add(view.Owner, 1);
                      if (view.IsMine)
                      {
                          LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>());
                          foreach (var item in LOD)
                          {
                              item.enabled = false;
                          }

                           parentCharacterController.enabled = false;
                           parentPlayerController.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;
                          Transformview.enabled = false;
                          Parent = loader.mainPlayer.transform;
                          loader.mainController.transform.parent =  car.FirstPlayerPos;
                          transform.localPosition = Vector3.zero;
                           loader.mainController.transform.localPosition = Vector3.zero;
                          GiantWheelManager.Instance.WheelCar.SetActive(false);
                          CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                          transform.rotation = new Quaternion(0, 0, 0, 0);
                           loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);
                      SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false);

                          CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                         camera.useOcclusionCulling = false;
                          GamePlayButtonEvents.inst.OnSwitchCameraClick();
                      }
                      else
                      {
                          Transformview.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;


                          Parent = transform.parent;
                          GameObject gasme = new GameObject();
                          gasme.transform.parent = car.FirstPlayerPos;
                          transform.parent = gasme.transform;
                          transform.localPosition = Vector3.zero;
                          transform.localRotation = new Quaternion(0, 0, 0, 0);
                          gasme.transform.localScale = Vector3.one;
                          gasme.transform.localPosition = Vector3.zero;
                          gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
                      }


                  }
                  else
                           if (wheelSeat ==2)
                  {
                      var car = GiantWheelManager.Instance.car;
                      car.issecondPlayerEmpty = false;
                      WheelSeat = 2;
                      car.PlayerSeat.Add(view.Owner, 2);
                      if (view.IsMine)
                      {
                          LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>());
                          foreach (var item in LOD)
                          {
                              item.enabled = false;
                          }
                           parentCharacterController.enabled = false;
                           parentPlayerController.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;
                          Transformview.enabled = false;
                          Parent = loader.mainPlayer.transform;
                          loader.mainController.transform.parent =  car.SecondPlayerPos;
                          transform.localPosition = Vector3.zero;
                           loader.mainController.transform.localPosition = Vector3.zero;
                      SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false);
                      CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                          transform.rotation = new Quaternion(0, 0, 0, 0);
                           loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);


                          CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                         camera.useOcclusionCulling = false;
                          GamePlayButtonEvents.inst.OnSwitchCameraClick();
                      }
                      else
                      {
                          Transformview.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;


                          Parent = transform.parent;
                          GameObject gasme = new GameObject();
                          gasme.transform.parent = car.SecondPlayerPos;
                          transform.parent = gasme.transform;
                          transform.localPosition = Vector3.zero;
                          transform.localRotation = new Quaternion(0, 0, 0, 0);
                          gasme.transform.localScale = Vector3.one;
                          gasme.transform.localPosition = Vector3.zero;
                          gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
                      }


                  }
                  else
                           if (wheelSeat == 3)
                  {
                      var car = GiantWheelManager.Instance.car;
                      car.isThirdPlayerEmpty = false;
                      WheelSeat = 3;
                      car.PlayerSeat.Add(view.Owner, 3);
                      if (view.IsMine)
                      {
                          LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>());
                          foreach (var item in LOD)
                          {
                              item.enabled = false;
                          }
                           parentCharacterController.enabled = false;
                           parentPlayerController.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;
                          Transformview.enabled = false;
                          Parent = loader.mainPlayer.transform;
                          loader.mainController.transform.parent =  car.ThirdPlayerPos;
                          transform.localPosition = Vector3.zero;
                           loader.mainController.transform.localPosition = Vector3.zero;
                      SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false);
                      CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                          transform.rotation = new Quaternion(0, 0, 0, 0);
                           loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);


                          CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                         camera.useOcclusionCulling = false;
                          GamePlayButtonEvents.inst.OnSwitchCameraClick();
                      }
                      else
                      {
                          Transformview.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;


                          Parent = transform.parent;
                          GameObject gasme = new GameObject();
                          gasme.transform.parent = car.ThirdPlayerPos;
                          transform.parent = gasme.transform;
                          transform.localPosition = Vector3.zero;
                          transform.localRotation = new Quaternion(0, 0, 0, 0);
                          gasme.transform.localScale = Vector3.one;
                          gasme.transform.localPosition = Vector3.zero;
                          gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
                      }


                  }
                  else
                           if (wheelSeat==4)
                  {
                      var car = GiantWheelManager.Instance.car;
                      car.isfirstPlayerEmpty = false;
                      WheelSeat = 4;
                      car.PlayerSeat.Add(view.Owner, 4);
                      if (view.IsMine)
                      {
                          LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>());
                          foreach (var item in LOD)
                          {
                              item.enabled = false;
                          }
                           parentCharacterController.enabled = false;
                           parentPlayerController.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;
                          Transformview.enabled = false;
                          Parent = loader.mainPlayer.transform;
                          loader.mainController.transform.parent =  car.ForthPlayerPos;
                          transform.localPosition = Vector3.zero;
                           loader.mainController.transform.localPosition = Vector3.zero;
                      SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false);
                      CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                          transform.rotation = new Quaternion(0, 0, 0, 0);
                           loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);


                          CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                         camera.useOcclusionCulling = false;

                          GamePlayButtonEvents.inst.OnSwitchCameraClick();
                      }
                      else
                      {
                          Transformview.enabled = false;
                         charcontroller.enabled = false;
                         arrowManager.enabled = false;


                          Parent = transform.parent;
                          GameObject gasme = new GameObject();
                          gasme.transform.parent = car.ForthPlayerPos;
                          transform.parent = gasme.transform;
                          transform.localPosition = Vector3.zero;
                          transform.localRotation = new Quaternion(0, 0, 0, 0);
                          gasme.transform.localScale = Vector3.one;
                          gasme.transform.localPosition = Vector3.zero;
                          gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
                      }


                  }
                 animator.SetTrigger("EnterCar");


          }

      }*/

    [PunRPC]
    public void WheelStoped()
    {
        GiantWheelManager.Instance.EnterwheelCar?.Invoke();
    }
    //private List<LODGroup> LOD; //Not used anymore
    [PunRPC]
    public void EnterWheelCar(int playpos)
    {
        StartCoroutine(EnterWheel(playpos));
    }

    IEnumerator EnterWheel(int playpos)
    {
        yield return new WaitUntil(() => GiantWheelManager.Instance != null || GiantWheelManager.Instance.car != null);
        if (view.IsMine)
        {
            LoadingHandler.Instance.DisableDomeLoading();
            // ConstantsHolder.DisableFppRotation = true;
            if (GameplayEntityLoader.instance._uiReferences.Onfreecam.gameObject.activeInHierarchy)
            {
                GameplayEntityLoader.instance._uiReferences.Onfreecam.onClick.Invoke();
                GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = false;
                GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = false;
            }
            else
            {
                GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = false;
                GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = false;
            }
            SummitMiniMapStatusOnSceneChange(false);
            GiantWheelManager.Instance.WheelCar.SetActive(false);

        }
        isInsideWheel = true;

        if (playpos == 1)
        {

            var car = GiantWheelManager.Instance.car;
            //car.isfirstPlayerEmpty = false; //Updating player seats in Custom Room Properties
            //WheelSeat = 1;
            //car.PlayerSeat.Add(view.Owner, 1);

            if (view.IsMine)
            {
                //LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>()); // Not used anymore
                //foreach (var item in LOD)
                //{
                //    item.enabled = false;
                //}
                // Store the current LOD bias and set it to 0
                _oldLodBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 0f;

                parentCharacterController.enabled = false;
                parentPlayerController.enabled = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;
                Transformview.m_SynchronizePosition = false;

                Parent = loader.mainPlayer.transform;
                loader.mainController.transform.parent = car.FirstPlayerPos;
                transform.localPosition = Vector3.zero;
                loader.mainController.transform.localPosition = Vector3.zero;

                CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                transform.rotation = new Quaternion(0, 0, 0, 0);
                loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);
                SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false,true);

                CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                camera.useOcclusionCulling = false;
                if (!parentPlayerController.isFirstPerson)
                    GamePlayButtonEvents.inst.OnSwitchCameraClick();
            }
            else
            {
                Transformview.m_SynchronizePosition = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;


                Parent = transform.parent;
                GameObject gasme = new GameObject();
                gasme.transform.parent = car.FirstPlayerPos;
                transform.parent = gasme.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = new Quaternion(0, 0, 0, 0);
                gasme.transform.localScale = Vector3.one;
                gasme.transform.localPosition = Vector3.zero;
                gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }


        }
        else
                     if (playpos == 2)
        {
            var car = GiantWheelManager.Instance.car;
            //car.issecondPlayerEmpty = false; //Updating player seats in Custom Room Properties
            //WheelSeat = 2;
            //car.PlayerSeat.Add(view.Owner, 2);

            if (view.IsMine)
            {
                //LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>()); // Not used anymore
                //foreach (var item in LOD)
                //{
                //    item.enabled = false;
                //}
                // Store the current LOD bias and set it to 0
                _oldLodBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 0f;

                parentCharacterController.enabled = false;
                parentPlayerController.enabled = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;
                Transformview.m_SynchronizePosition = false;
                Parent = loader.mainPlayer.transform;
                loader.mainController.transform.parent = car.SecondPlayerPos;
                transform.localPosition = Vector3.zero;
                loader.mainController.transform.localPosition = Vector3.zero;
                SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false,true);
                CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                transform.rotation = new Quaternion(0, 0, 0, 0);
                loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);


                CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                camera.useOcclusionCulling = false;
                if (!parentPlayerController.isFirstPerson)
                    GamePlayButtonEvents.inst.OnSwitchCameraClick();
            }
            else
            {
                Transformview.m_SynchronizePosition = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;


                Parent = transform.parent;
                GameObject gasme = new GameObject();
                gasme.transform.parent = car.SecondPlayerPos;
                transform.parent = gasme.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = new Quaternion(0, 0, 0, 0);
                gasme.transform.localScale = Vector3.one;
                gasme.transform.localPosition = Vector3.zero;
                gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }


        }
        else
                     if (playpos == 3)
        {
            var car = GiantWheelManager.Instance.car;
            //car.isThirdPlayerEmpty = false; //Updating player seats in Custom Room Properties
            //WheelSeat = 3;
            //car.PlayerSeat.Add(view.Owner, 3);

            if (view.IsMine)
            {
                //LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>()); // Not used anymore
                //foreach (var item in LOD)
                //{
                //    item.enabled = false;
                //}
                // Store the current LOD bias and set it to 0
                _oldLodBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 0f;

                parentCharacterController.enabled = false;
                parentPlayerController.enabled = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;
                Transformview.m_SynchronizePosition = false;
                Parent = loader.mainPlayer.transform;
                loader.mainController.transform.parent = car.ThirdPlayerPos;
                transform.localPosition = Vector3.zero;
                loader.mainController.transform.localPosition = Vector3.zero;
                SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false, false);
                CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                transform.rotation = new Quaternion(0, 0, 0, 0);
                loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);


                CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                camera.useOcclusionCulling = false;
                if (!parentPlayerController.isFirstPerson)
                    GamePlayButtonEvents.inst.OnSwitchCameraClick();
            }
            else
            {
                Transformview.m_SynchronizePosition = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;


                Parent = transform.parent;
                GameObject gasme = new GameObject();
                gasme.transform.parent = car.ThirdPlayerPos;
                transform.parent = gasme.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = new Quaternion(0, 0, 0, 0);
                gasme.transform.localScale = Vector3.one;
                gasme.transform.localPosition = Vector3.zero;
                gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }


        }
        else
                     if (playpos == 4)
        {
            var car = GiantWheelManager.Instance.car;
            //car.isfirstPlayerEmpty = false; //Updating player seats in Custom Room Properties
            //WheelSeat = 4;
            //car.PlayerSeat.Add(view.Owner, 4);

            if (view.IsMine)
            {
                //LOD = new List<LODGroup>(FindObjectsOfType<LODGroup>()); // Not used anymore
                //foreach (var item in LOD)
                //{
                //    item.enabled = false;
                //}
                // Store the current LOD bias and set it to 0
                _oldLodBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 0f;

                parentCharacterController.enabled = false;
                parentPlayerController.enabled = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;
                Transformview.m_SynchronizePosition = false;
                Parent = loader.mainPlayer.transform;
                loader.mainController.transform.parent = car.ForthPlayerPos;
                transform.localPosition = Vector3.zero;
                loader.mainController.transform.localPosition = Vector3.zero;
                SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(false,false);
                CarNavigationManager.CarNavigationInstance.EnableExitCanvas();
                transform.rotation = new Quaternion(0, 0, 0, 0);
                loader.mainController.transform.rotation = new Quaternion(0, 0, 0, 0);


                CarNavigationManager.CarNavigationInstance.OnCancelPress += CancelExit;
                camera.useOcclusionCulling = false;
                if (!parentPlayerController.isFirstPerson)
                    GamePlayButtonEvents.inst.OnSwitchCameraClick();
            }
            else
            {
                Transformview.m_SynchronizePosition = false;
                charcontroller.enabled = false;
                arrowManager.enabled = false;


                Parent = transform.parent;
                GameObject gasme = new GameObject();
                gasme.transform.parent = car.ForthPlayerPos;
                transform.parent = gasme.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = new Quaternion(0, 0, 0, 0);
                gasme.transform.localScale = Vector3.one;
                gasme.transform.localPosition = Vector3.zero;
                gasme.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }



        }
        SetClothEnabled(false);


    }
    void SummitMiniMapStatusOnSceneChange(bool makeActive)
    {
        if (makeActive)
            ConstantsHolder.xanaConstants.minimap = 1;
        else
            ConstantsHolder.xanaConstants.minimap = 0;

        if (makeActive && ConstantsHolder.xanaConstants.minimap == 1)
        {
            ReferencesForGamePlay.instance.minimap.SetActive(true);
            ReferencesForGamePlay.instance.SumitMapStatus(true);
        }
        else
        {
            ReferencesForGamePlay.instance.SumitMapStatus(false);
            ReferencesForGamePlay.instance.minimap.SetActive(false);
        }

    }

    [PunRPC]
    void ExitWheelCar(int pos)
    {
        isInsideWheel = false;
       
        if (view.IsMine)
        {
            LoadingHandler.Instance.ReturnlWheelloading();
            LoadingHandler.Instance.DomeLoading.SetActive(true);
            //ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<PlayerController>().m_IsMovementActive = false;

            //foreach (var item in LOD) // Not used anymore
            //{
            //    item.enabled = true;
            //}
            // Restore the old LOD bias
            QualitySettings.lodBias = _oldLodBias;

            ConstantsHolder.DisableFppRotation = false;


            GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = true;
            GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = true;

            if (XanaChatSystem.instance.isChatOpen)
            {
                XanaChatSystem.instance.chatDialogBox.SetActive(false);
                XanaChatSystem.instance.chatNotificationIcon.SetActive(false);
                XanaChatSystem.instance.chatButton.GetComponent<Image>().enabled = false;
            }

            MutiplayerController.instance.disableSector = false;
            CarNavigationManager.CarNavigationInstance.DisableExitCanvas();
            loader.mainController.transform.parent = Parent;
            loader.mainController.transform.position = GiantWheelManager.Instance.Exit.position;
            SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(true, false);
            parentCharacterController.enabled = true;
            parentPlayerController.enabled = true;
            charcontroller.enabled = true;
            arrowManager.enabled = true;
            Transformview.m_SynchronizePosition = true;


            GiantWheelManager.Instance.WheelCar.SetActive(true);
            GamePlayButtonEvents.inst.OnSwitchCameraClick();
            StartCoroutine(ExitSectorAfterDelay(1));
            MyPlayerPos = 0;
            SummitMiniMapStatusOnSceneChange(true);
        }
        else
        {
            var car = GiantWheelManager.Instance.car;
            transform.parent = loader.mainPlayer.transform.transform.parent;
            transform.position = GiantWheelManager.Instance.Exit.position;
            transform.localScale = Vector3.one * 1.14f;
            charcontroller.enabled = true;
            arrowManager.enabled = true;
            Transformview.m_SynchronizePosition = true;

            MyPlayerPos = 0;
            //if (pos == 1) //Updating player seats in Custom Room Properties
            //{
                //car.isfirstPlayerEmpty = true;
            //}
            //if (pos == 2)
            //{
            //    car.issecondPlayerEmpty = true;
            //}
            //if (pos == 3)
            //{
            //    car.isThirdPlayerEmpty = true;
            //}
            //if (pos == 4)
            //{
            //    car.isForthPlayerEmpty = true;
            //}

        }
        SetClothEnabled(true);
    }
    IEnumerator ExitSectorAfterDelay(int time)
    {
        yield return new WaitForSeconds(time);
        MutiplayerController.instance.Ontriggered("Default");
        GiantWheelManager.Instance.CarAdded = false;
    }

    public void waitForWheel()
    {

        GiantWheelManager.Instance.onCarStop += CarStop;
        GiantWheelManager.Instance.EnterwheelCar += CallEnterWheelRPC;
        GiantWheelManager.Instance.StopWheel(this);
    }

    public void CallEnterWheelRPC()
    {
        if (!isInsideWheel && view.IsMine)
        {
            var car = GiantWheelManager.Instance.car;
            var props = PhotonNetwork.CurrentRoom.CustomProperties;

            int seatToAssign = 0;
            for (int i = 1; i <= 4; i++)
            {
                string seatKey = $"GiantWheelSeat{i}";
                if (!props.ContainsKey(seatKey) || string.IsNullOrEmpty(props[seatKey] as string))
                {
                    seatToAssign = i;
                    break;
                }
            }

            if (seatToAssign == 0)
            {
                Debug.LogWarning("No available seats!");
                return;
            }

            // Reserve the seat
            ExitGames.Client.Photon.Hashtable seatUpdate = new ExitGames.Client.Photon.Hashtable();
            seatUpdate[$"GiantWheelSeat{seatToAssign}"] = PhotonNetwork.LocalPlayer.UserId;
            PhotonNetwork.CurrentRoom.SetCustomProperties(seatUpdate);

            MyPlayerPos = seatToAssign;
            LoadingHandler.Instance.DomeLoadingProgess(100);
            view.RPC(nameof(EnterWheelCar), RpcTarget.All, MyPlayerPos);
        }
    }

    private void CarStop()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            view.RPC(nameof(WheelStoped), RpcTarget.Others);
            GameplayEntityLoader.instance.IsJoinSummitWorld = false;
        }
    }

    public void Exit()
    {
        showExit = true;
        if (isInsideWheel)
        {
            view.RPC(nameof(ExitWheelCar), RpcTarget.All, MyPlayerPos);
            LoadingHandler.Instance.enter = false;
            LoadingHandler.Instance.iswheel = false;

            CarNavigationManager.CarNavigationInstance.DisableExitCanvas();
        }
        if (isInsideCAr)
        {
            ExitCar();
            CarNavigationManager.CarNavigationInstance.DisableExitCanvas();
            GameplayEntityLoader.instance.PlayerCamera.GetComponent<CinemachineCollider>().m_MinimumDistanceFromTarget = 0.02f; //Set to Default on Car Exit
        }
    }
    public void CancelExit()
    {
        showExit = false;
    }

    public void checkforExit()
    {
        if (view.IsMine)
        {
            if (showExit)
            {

            }
        }
    }

    public void CheckForExitWheel()
    {
        if (view.IsMine)
        {
            if (showExit)
            {


            }
        }

    }

    public void checkforDisableExit()
    {
        if (view.IsMine)
            CarNavigationManager.CarNavigationInstance.DisableExitCanvas();

    }
    private void SetClothEnabled(bool enabled)
    {
        if (avatarController == null) return;


        if (avatarController.wornShirt != null)
        {
            var shirtCloth = avatarController.wornShirt?.GetComponent<MagicaCloth>();
            if (shirtCloth != null)
                shirtCloth.enabled = enabled;
        }
        if (avatarController.wornHair != null)
        {
            var hairCloth = avatarController.wornHair?.GetComponent<MagicaCloth>();
            if (hairCloth != null)
                hairCloth.enabled = enabled;
        }
    }
    public void OnPlayerLeftRoom(Player otherPlayer)
    {

    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {

    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {

    }

    public void SpwanCanidateManager(int viewID, string addressableKey)
    {
        // Only the Master Client should send the RPC
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"[SpwanCanidateManager] Spawning CandidatesManager with ViewID: {viewID} and Addressable Key: {addressableKey}");
            gameObject.GetComponent<PhotonView>().RPC(nameof(SpawnCanidatesManagerRPC), RpcTarget.All, viewID, addressableKey);
            //SpawnCanidatesManagerRPC(viewID, addressableKey); // Spawn locally for the Master Client
        }
    }

    [PunRPC]
    void SpawnCanidatesManagerRPC(int viewID, string addressableKey)
    {
        if (CandidateSpwanner.candidatesManager == null)
        {

            Debug.Log($"[SpawnCanidatesManagerRPC] Starting to load prefab with Addressable key: {addressableKey}");
            Debug.Log($"[SpawnCanidatesManagerRPC] Successfully loaded prefab with Addressable key: {addressableKey}");


            // Instantiate the object locally
            GameObject instance = PhotonNetwork.InstantiateRoomObject(addressableKey, Vector3.zero, Quaternion.identity);
            Debug.Log($"[SpawnCanidatesManagerRPC] Prefab instantiated successfully.");

            CandidateSpwanner.candidatesManager = instance.GetComponent<CandidatesManager>();
            if (CandidateSpwanner.candidatesManager != null)
            {
                Debug.Log($"[SpawnCanidatesManagerRPC] CandidatesManager component assigned successfully.");
            }
            else
            {
                Debug.LogError($"[SpawnCanidatesManagerRPC] Failed to find CandidatesManager component on the instantiated prefab.");
            }

            // Assign the PhotonView ID
            //PhotonView photonView = instance.GetComponent<PhotonView>();
            //if (photonView != null)
            //{
            //    Debug.Log($"[SpawnCanidatesManagerRPC] PhotonView ID {viewID} assigned successfully.");
            //    photonView.ViewID = viewID;
            //}
            //else
            //{
            //    Debug.LogError($"[SpawnCanidatesManagerRPC] The prefab does not have a PhotonView component.");
            //}

            // Move the instance to the target scene
            Debug.Log($"[SpawnCanidatesManagerRPC] Moving instance to scene: {WorldItemView.m_EnvName}");
            StartCoroutine(WaitToMoveScene(instance, WorldItemView.m_EnvName));
            //// Download the prefab locally
            //Addressables.LoadAssetAsync<GameObject>(addressableKey).Completed += handle =>
            //{
            //    if (handle.Status == AsyncOperationStatus.Succeeded)
            //    {
            //        Debug.Log($"[SpawnCanidatesManagerRPC] Successfully loaded prefab with Addressable key: {addressableKey}");

            //        GameObject prefab = handle.Result;

            //        // Instantiate the object locally
            //        GameObject instance = PhotonNetwork.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            //        Debug.Log($"[SpawnCanidatesManagerRPC] Prefab instantiated successfully.");

            //        CandidateSpwanner.candidatesManager = instance.GetComponent<CandidatesManager>();
            //        if (CandidateSpwanner.candidatesManager != null)
            //        {
            //            Debug.Log($"[SpawnCanidatesManagerRPC] CandidatesManager component assigned successfully.");
            //        }
            //        else
            //        {
            //            Debug.LogError($"[SpawnCanidatesManagerRPC] Failed to find CandidatesManager component on the instantiated prefab.");
            //        }

            //        // Assign the PhotonView ID
            //        PhotonView photonView = instance.GetComponent<PhotonView>();
            //        if (photonView != null)
            //        {
            //            Debug.Log($"[SpawnCanidatesManagerRPC] PhotonView ID {viewID} assigned successfully.");
            //            photonView.ViewID = viewID;
            //        }
            //        else
            //        {
            //            Debug.LogError($"[SpawnCanidatesManagerRPC] The prefab does not have a PhotonView component.");
            //        }

            //        // Move the instance to the target scene
            //        Debug.Log($"[SpawnCanidatesManagerRPC] Moving instance to scene: {WorldItemView.m_EnvName}");
            //        StartCoroutine(WaitToMoveScene(instance, WorldItemView.m_EnvName));
            //    }
            //    else
            //    {
            //        Debug.LogError($"[SpawnCanidatesManagerRPC] Failed to load prefab with Addressable key: {addressableKey}. Status: {handle.Status}");
            //    }
            //};
        }
    }

    IEnumerator WaitToMoveScene( GameObject temp, string name)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.MoveGameObjectToScene(temp, SceneManager.GetSceneByName(WorldItemView.m_EnvName));

    }

}
