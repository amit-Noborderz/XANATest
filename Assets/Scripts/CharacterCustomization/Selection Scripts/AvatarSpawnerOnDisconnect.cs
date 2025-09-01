using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary> this script is Handling Build in avatar models data and spawning  avatar buttons on  Ui.</summary>/// 
public class AvatarSpawnerOnDisconnect : MonoBehaviourPunCallbacks
{
    [HideInInspector]
    public GameObject currentDummyPlayer;
    public AskForJoining askForJoining;

    // Start is called before the first frame update
    public GameObject spawnPoint;
    public GameObject JoinCurrentRoomPanel;
    public RectTransform ConnectionErrorToast;
    public Sprite FavouriteAnimationSprite;
    public Sprite NormalAnimationSprite;

    public static event Action OninternetDisconnect;
    public static event Action OninternetConnected;

    public static AvatarSpawnerOnDisconnect Instance;

    public GameObject InternetLost;
    public TextMeshProUGUI toastMessage;
    public List<GameObject> DisableUIElementsOnDisconnect = new List<GameObject>();
    public GameObject EmotesFavouriteCircle;
    public GameObject EmotesFavouritePanel;
    public bool IsRjoining = false;
    float lastPausedTime;
    private void Awake()
    {
        Instance = this;
        if(ConnectionErrorToast.gameObject.GetComponent<AskForJoining>() != null)
            askForJoining = ConnectionErrorToast.gameObject.GetComponent<AskForJoining>();
    }


    private void OnApplicationQuit()
    {
        PhotonNetwork.Destroy(currentDummyPlayer);
        PhotonNetwork.LeaveRoom(false);
        PhotonNetwork.LeaveLobby();
        UserAnalyticsHandler.onUpdateWorldRelatedStats?.Invoke(false, false, false, true);
        if(XanaPrivateChat.IsPrivateChatActive)
        {
            XanaChatSystem.instance.xanaPrivateChat.LeavePrivateChatBtn();
        }
    }

    public void ShowJoinRoomPanel()
    {
        //InternetLost = null;
        if (InternetLost == null)
        {
            ConstantsHolder.xanaConstants.needToClearMemory = false;
            //if (LoadingHandler.Instance)
                //LoadingHandler.Instance.HideLoading();
            //GameObject go = Instantiate(JoinCurrentRoomPanel) as GameObject;
            ConnectionErrorToast.gameObject.SetActive(true);
            toastMessage.text= TextLocalization.GetLocaliseTextByKey("Connection Error: waiting for network...");
            ConnectionErrorToast.DOAnchorPos(new Vector2(ConnectionErrorToast.anchoredPosition.x, -245f), 0.5f).SetEase(Ease.InOutSine);
            DisableUnwantedScreens();
            EnableDisableUIElements(false);
            if(GamePlayUIHandler.inst.sitButton.gameObject.activeInHierarchy) // sitting button
                GamePlayUIHandler.inst.sitButton.gameObject.SetActive(false);
            if (!ConstantsHolder.isPenguin && GameplayEntityLoader.instance.mainController.GetComponent<SwimmingController>().isInPool)
            {
                //DashButton.SetActive(false);
                GameplayEntityLoader.instance.mainController.GetComponent<SwimmingController>().SetSwimJumpState(false);
                GameplayEntityLoader.instance.mainController.GetComponent<SwimmingController>().StartSwimming();
            }
            InternetLost = ConnectionErrorToast.gameObject;
        }

        if (LoadingHandler.Instance != null && !LoadingHandler.Instance.gameObject.transform.GetChild(0).gameObject.activeInHierarchy)
        {
            //PlayerCameraController.instance.DisAllowControl();
            //OffSelfie();
            //TurnCameras(false);
            //Instantiate(JoinCurrentRoomPanel);
        }

    }
    public void InstantiatePlayerAgain()
    {
        StartCoroutine(MainReconnect());
    }

    private void OffSelfie()
    {
        if (ConstantsHolder.isPenguin || ConstantsHolder.xanaConstants.isXanaPartyWorld) return;
        //PlayerSelfieController.Instance.isReconnecting = true;
        PlayerSelfieController.Instance.SwitchFromSelfieControl();
        if (ReferencesForGamePlay.instance.m_34player && ReferencesForGamePlay.instance.m_34player.GetComponent<IKMuseum>())
        ReferencesForGamePlay.instance.m_34player.GetComponent<IKMuseum>().m_SelfieStick.SetActive(false);
    }

    private void TurnCameras(bool active)
    {
        if (active)
        {
            PlayerCameraController.instance.AllowControl();
        }
        else
        {
            PlayerCameraController.instance.DisAllowControl();
        }
    }

    RoomOptions roomOptions;


    private IEnumerator MainReconnect()
    {
        while (PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState != ExitGames.Client.Photon.PeerStateValue.Disconnected)
        {
            //Debug.Log("Waiting for client to be fully disconnected..", this);
            yield return new WaitForSeconds(0.2f);
        }

        string lastRoomName = Photon.Pun.Demo.PunBasics.MutiplayerController.CurrRoomName;
        if (!PhotonNetwork.ReconnectAndRejoin())
        {
            if (PhotonNetwork.RejoinRoom(lastRoomName))
            {
                Debug.Log(" Successful reconnected!", this);
            }
        }
        else
        {
           /* PhotonNetwork.AutomaticallySyncScene = true;                      //Zeel Removed this cusing sector issue
            roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 20;
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;
            roomOptions.PublishUserId = true;
            roomOptions.CleanupCacheOnLeave = true;*/
            PhotonNetwork.JoinOrCreateRoom(lastRoomName,MutiplayerController.instance.roomOptions, new TypedLobby(Photon.Pun.Demo.PunBasics.MutiplayerController.CurrLobbyName, LobbyType.Default), null);
        }

    }
    void OnApplicationPause(bool pause)
    {
        if (pause)
            lastPausedTime = Time.realtimeSinceStartup;
        else
        {
            float pausedDuration = Time.realtimeSinceStartup - lastPausedTime;
            if (pausedDuration > 10f)
            {
                MutiplayerController.instance.connectionState = ServerConnectionStates.NotConnectedToServer;
                PhotonNetwork.Disconnect();
            }
        }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
#if UNITY_EDITOR
        if (cause == DisconnectCause.ApplicationQuit)
            return;
#endif
        base.OnDisconnected(cause);
        if (ConstantsHolder.xanaConstants.EnviornmentName != "BreakingDownGame")
        {
            if (ReferencesForGamePlay.instance.m_34player != null && !MutiplayerController.instance.isShifting)
            {
                GameplayEntityLoader.instance.SpawnLocalPlayer();
            }
            else
            {
                MutiplayerController.instance.isShifting = false;
            }
            if (OninternetDisconnect != null)
                OninternetDisconnect.Invoke();
            ShowJoinRoomPanel();
        }
    }


    public override void OnConnected()
    {
        if (OninternetConnected != null)
            OninternetConnected.Invoke();
    }

    void OnApplicationFocus(bool isGameFocus)
    {
        // User Analatics 
        if (!SceneManager.GetActiveScene().name.Contains("Home"))
        {
            //UserAnalyticsHandler.onUserJoinedLeaved?.Invoke(isGameFocus);
            if (isGameFocus)
            {
                UserAnalyticsHandler.onUpdateWorldStatCustom?.Invoke(true, false);
            }
            else
            {
                UserAnalyticsHandler.onUpdateWorldRelatedStats?.Invoke(false, false, false, true);
            }
        }

    }
    public void EnableDisableUIElements(bool enable)
    {
        foreach (GameObject go in DisableUIElementsOnDisconnect)
        {
            if (go != null)
                go.SetActive(enable);
        }
    }
    public void DisableUnwantedScreens()
    {
        XanaChatSystem.instance.OpenCloseChatDialog(false);
        if (ReferencesForGamePlay.instance.playerControllerNew.isFirstPerson)
        {
            GamePlayUIHandler.inst.OnSwitchCameraClick();
        }
        //if(PlayerSelfieController.Instance.m_IsSelfieFeatureActive)
        //{
        //    PlayerSelfieController.Instance.DisableSelfieFeature();
        //}
        OffSelfie();
        GamePlayUIHandler.inst.CloseAnimationButtonClick();
        GamePlayUIHandler.inst.CloseReactionButtonClick();
        if(!LoadingHandler.Instance.DomeLoading.activeInHierarchy)
            GamePlayUIHandler.inst.OnHelpButtonClick(false);
        TopMenuButtonController.Instance.CloseAllScreens();
        EmotesFavouriteCircle.SetActive(false);
        EmotesFavouritePanel.SetActive(false);

        ReferencesForGamePlay.instance.FullScreenMapStatus(false);
        if (GamePlayUIHandler.inst.isFreeCam)
        {
            ReferencesForGamePlay.instance.playerControllerNew.FreeFloatToggleButton(false);
            ReferencesForGamePlay.instance.hiddenButtonEnable();
            ReferencesForGamePlay.instance.m_34player.GetComponent<IKMuseum>().ConsoleObj.SetActive(false);
            ReferencesForGamePlay.instance.m_34player.GetComponent<IKMuseum>().m_ConsoleObjOther.SetActive(false);
        }
        if (LoadingHandler.Instance.ApprovalUI.activeInHierarchy)
            LoadingHandler.Instance.ReturnDome();
    }
    public void DisableConnectionToast()
    {
        ConnectionErrorToast.DOAnchorPos(new Vector2(ConnectionErrorToast.anchoredPosition.x, 0f), 0.5f).SetEase(Ease.InOutSine)
        .OnComplete(() =>
        {
            ConnectionErrorToast.gameObject.SetActive(false);
            IsRjoining = false;
        });
    }
}