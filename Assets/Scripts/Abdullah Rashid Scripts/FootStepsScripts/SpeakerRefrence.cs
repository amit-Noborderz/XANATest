using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;
using Photon.Voice.PUN;

public class SpeakerRefrence : MonoBehaviour
{
   
    public AudioSource RangeVolSpeaker;

    void Start()
    {
        var voiceView = GetComponent<PhotonVoiceView>();
        if (voiceView != null)
        {
            if (voiceView.RecorderInUse != null)
            {
                voiceView.RecorderInUse.DebugEchoMode = false;
            }

            if (voiceView.SpeakerInUse != null)
            {
                voiceView.SpeakerInUse.enabled = !GetComponent<PhotonView>().IsMine;
            }
        }
    }

    private void OnEnable()
    {
        if(ConstantsHolder.xanaConstants.EnviornmentName=="RooftopParty")
        {
            RangeVolSpeaker.maxDistance = 3;
        }

        if(!GetComponent<PhotonView>().IsMine)
            Destroy(GetComponent<AudioListener>());

    }

    //public void MyspeakerSync2D()                  //Added by Ali Hamza
    //{
    //    GetComponent<PhotonView>().RPC(nameof(SyncSpeaker2D), RpcTarget.AllBuffered, GetComponent<PhotonView>().ViewID);
    //}

    //[PunRPC]
    //void SyncSpeaker2D(int viewId)                       //Added by Ali Hamza
    //{
    //    for (int i = 0; i < MutiplayerController.instance.playerobjects.Count; i++)
    //    {
    //        if (MutiplayerController.instance.playerobjects[i] != null)
    //        {
    //            if (MutiplayerController.instance.playerobjects[i].GetComponent<PhotonView>().ViewID == viewId)
    //            {
    //                MutiplayerController.instance.playerobjects[i].GetComponent<SpeakerRefrence>().RangeVolSpeaker.spatialBlend = 0;
    //            }
    //        }
    //    }
    //}
    //public void MyspeakerSync3D()                  //Added by Ali Hamza
    //{
    //    GetComponent<PhotonView>().RPC(nameof(SyncSpeaker3D), RpcTarget.AllBuffered, GetComponent<PhotonView>().ViewID);
    //}

    //[PunRPC]
    //void SyncSpeaker3D(int viewId)                       //Added by Ali Hamza
    //{
    //    for (int i = 0; i < MutiplayerController.instance.playerobjects.Count; i++)
    //    {
    //        if (MutiplayerController.instance.playerobjects[i] != null)
    //        {
    //            if (MutiplayerController.instance.playerobjects[i].GetComponent<PhotonView>().ViewID == viewId)
    //            {
    //                MutiplayerController.instance.playerobjects[i].GetComponent<SpeakerRefrence>().RangeVolSpeaker.spatialBlend = 1;
    //            }
    //        }
    //    }
    //}
}
