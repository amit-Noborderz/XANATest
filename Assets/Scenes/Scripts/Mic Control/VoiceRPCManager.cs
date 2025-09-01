using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class VoiceRPCManager : MonoBehaviour, IOnEventCallback
{
    // Event codes for different voice events
    private const byte MUTE_ALL_PLAYERS_EVENT = 2;
    private const byte SET_USER_MIC_STATUS_EVENT = 3;

    PhotonView PhotonView;

    private void OnEnable()
    {
        PhotonView = GetComponent<PhotonView>();
        PhotonNetwork.AddCallbackTarget(this);

        if (PhotonView.IsMine)
        {
            VoicePlayerManager.MuteAllPlayersMic += MuteAllPlayers;
            VoicePlayerManager.SetUserMicStatus += SetUserMicStatus;
        }
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);

        if (PhotonView.IsMine)
        {
            VoicePlayerManager.MuteAllPlayersMic -= MuteAllPlayers;
            VoicePlayerManager.SetUserMicStatus -= SetUserMicStatus;
        }
    }

    void MuteAllPlayers(bool mute)
    {
        object[] content = new object[] { mute };
        RaiseEventOptions raiseEventOptions = RaiseEventOptions.Default;
        raiseEventOptions.Receivers = ReceiverGroup.Others;

        PhotonNetwork.RaiseEvent(MUTE_ALL_PLAYERS_EVENT, content, raiseEventOptions, SendOptions.SendReliable);
    }

    void SetUserMicStatus(int viewId, bool micStatus)
    {
        object[] content = new object[] { viewId, micStatus };
        RaiseEventOptions raiseEventOptions = RaiseEventOptions.Default;
        raiseEventOptions.Receivers = ReceiverGroup.Others;

        PhotonNetwork.RaiseEvent(SET_USER_MIC_STATUS_EVENT, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        switch (eventCode)
        {
            case MUTE_ALL_PLAYERS_EVENT:
                OnMuteAllPlayersEvent(photonEvent.CustomData);
                break;

            case SET_USER_MIC_STATUS_EVENT:
                OnSetUserMicStatusEvent(photonEvent.CustomData);
                break;
        }
    }

    private void OnMuteAllPlayersEvent(object customData)
    {
        object[] data = (object[])customData;
        bool mute = (bool)data[0];

        Debug.LogError("Mute all Event");
        if (mute)
        {
            ConstantsHolder.xanaConstants.UserCanUnmute = false;
            XanaVoiceChat.instance.TurnOffMic();
            Debug.LogError("user mic is turned off");
        }
        else
        {
            ConstantsHolder.xanaConstants.UserCanUnmute = true;
            Debug.Log("Now User can Unmute mic");
        }
    }

    private void OnSetUserMicStatusEvent(object customData)
    {
        object[] data = (object[])customData;
        int viewId = (int)data[0];
        bool muted = (bool)data[1];

        Debug.LogError($"this users mic is set {viewId}" + muted);
        if (PhotonView.ViewID == viewId)
        {
            if (muted)
            {
                ConstantsHolder.xanaConstants.UserCanUnmute = false;
                XanaVoiceChat.instance.TurnOffMic();
            }
            else
            {
                ConstantsHolder.xanaConstants.UserCanUnmute = true;
            }
        }
    }
}




//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Photon.Pun;

//[RequireComponent(typeof(Photon.Pun.PhotonView))]
//public class VoiceRPCManager : MonoBehaviour
//{
//    PhotonView PhotonView;
//    private void OnEnable()
//    {
//        PhotonView = GetComponent<PhotonView>();
//        if (PhotonView.IsMine)
//        {
//            VoicePlayerManager.MuteAllPlayersMic += MuteAllPlayers;
//            VoicePlayerManager.SetUserMicStatus += SetUserMicStatus;
//        }
//    }

//    private void OnDisable()
//    {
//        if (PhotonView.IsMine)
//        {
//            VoicePlayerManager.MuteAllPlayersMic -= MuteAllPlayers;
//            VoicePlayerManager.SetUserMicStatus -= SetUserMicStatus;
//        }
//    }

//    void MuteAllPlayers(bool Mute)
//    {
//        PhotonView.RPC(nameof(MuteAllPlayersRPC), RpcTarget.Others, Mute);
//    }
//    [PunRPC]
//    public void MuteAllPlayersRPC(bool mute)
//    {
//        Debug.LogError("Mute all RPC");
//        if (mute)
//        {
//            ConstantsHolder.xanaConstants.UserCanUnmute = false;
//            XanaVoiceChat.instance.TurnOffMic();
//            Debug.LogError("user mic is turned off");
//        }
//        else
//        {
//            ConstantsHolder.xanaConstants.UserCanUnmute = true;
//            Debug.Log("Now User can Unmute mic");
//        }

//    }

//    void SetUserMicStatus(int viewId, bool micStatus)
//    {
//        PhotonView.RPC(nameof(SetUserMicStatusRPC), RpcTarget.Others, viewId, micStatus);
//    }

//    [PunRPC]
//    public void SetUserMicStatusRPC(int viewId, bool muted)
//    {
//        Debug.LogError($"this users mic is set {viewId}" + muted);
//        if (PhotonView.ViewID== viewId)
//        {
//            if(muted)
//            {
//                ConstantsHolder.xanaConstants.UserCanUnmute = false;
//                XanaVoiceChat.instance.TurnOffMic();
//            }
//            else
//            {
//                ConstantsHolder.xanaConstants.UserCanUnmute = true;
//            }
//        }
//    }
//}
