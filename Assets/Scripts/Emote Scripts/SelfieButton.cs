using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelfieButton : MonoBehaviour
{
    Button btn;

    public void Awake()
    {
        btn = GetComponent<Button>();
    }

    public void OnEnable()
    {
        // if (GamePlayButtonEvents.inst != null) GamePlayButtonEvents.inst.SelfieBtnUpdate += SelfieBtnUpdated; // no need for this function enable and disable class doing this !
        btn.onClick.AddListener(OnSelfieClick);
    }


    public void OnDisable()
    {
        //  if (GamePlayButtonEvents.inst != null) GamePlayButtonEvents.inst.SelfieBtnUpdate -= SelfieBtnUpdated; // no need for this function enable and disable class doing this !
        btn.onClick.RemoveListener(OnSelfieClick);
    }

    private void OnSelfieClick()
    {
        GameObject player = ReferencesForGamePlay.instance.m_34player;
        if (player.GetComponent<PhotonView>().IsMine && player.GetComponent<PlayerSitting>().isSitting)
        {
            //btn.interactable = false;
            return;
        }
        else
        {
            btn.interactable = true;
        }
        //added condition to prevent selfie open issue while jumping and falling from environment
        //added condition to prevent selfie open issue while player is respawning or being shifted from one summit area to other summit area. 
        if (!ReferencesForGamePlay.instance.playerControllerNew._IsGrounded || MutiplayerController.instance.isShifting)
            return;
        //if (ActionManager.IsAnimRunning) //for stop dance animation
        //{
        //    ActionManager.StopActionAnimation?.Invoke();
        //}
        ActionManager.StopActionAnimation?.Invoke();
        EmoteReactionUIHandler.lastEmotePlayed = null;

        Invoke("CallToSelfie",0.5f);
        BuilderEventManager.UIToggle?.Invoke(true);
        PlayerController.PlayerIsWalking?.Invoke();
        ReferencesForGamePlay.instance.playerControllerNew.StopBuilderComponent();
        PlayerSelfieController.Instance.WheelExitButton.SetActive(false);
    }

    private void SelfieBtnUpdated(bool canClick)
    {
        btn.interactable = canClick;
    }
    private void CallToSelfie() {
        GamePlayButtonEvents.inst.OnSelfieClick();
    }
    
}