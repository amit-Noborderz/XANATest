using MD_Plugin;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MyBeachSelfieCam : MonoBehaviour
{
    public GameObject Selfie;
    public Camera SelfieCapture;
    public Camera SelfieCapturepotrait;
    public Camera SelfieCapturepotrait1;
    public GameObject SelfieCapture_CamRender;
    public GameObject SelfieCapture_CamRenderPotraiat;


    public List<string> m_SceneNames;
    // Start is called before the first frame update
    public Cinemachine.CinemachineVirtualCamera CinemachineVirtualCamera;
    public GameObject FrontSelfieCamTarget;
    public GameObject RearSelfieCamTarget;
    private void OnEnable()
    {
        BuilderEventManager.SwitchCamera += ToggleSelfieCamera;
        if (GamePlayButtonEvents.inst != null) GamePlayButtonEvents.inst.OnSelfieButton += OpenSelfieCam;
    }

    private void OnDisable()
    {
        BuilderEventManager.SwitchCamera -= ToggleSelfieCamera;
        if (GamePlayButtonEvents.inst != null) GamePlayButtonEvents.inst.OnSelfieButton -= OpenSelfieCam;
    }

    private void Start()
    {
        if (ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<PlayerController>().animator)
        {
            RuntimeAnimatorController animator = ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<PlayerController>().animator.runtimeAnimatorController;
            EmoteAnimationHandler.Instance.controller = animator;
        }
        SelfieCapturePP();
        CheckForOcclusionCulling();
    }
    public void SelfieCapturePP() {
        if (CheckPostProcessEnable())
        {
            Selfie.GetComponent<Camera>().GetUniversalAdditionalCameraData().renderPostProcessing = true;
            SelfieCapture.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            SelfieCapturepotrait.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            SelfieCapturepotrait1.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        }
    }

    public void CheckForOcclusionCulling()
    {
       if(ConstantsHolder.xanaConstants.EnviornmentName.Equals("D_Infinity_Labo"))
            Selfie.GetComponent<Camera>().useOcclusionCulling = false;
    }

    bool CheckPostProcessEnable()
    {
        if (m_SceneNames.Contains(WorldItemView.m_EnvName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void OpenSelfieCam()
    {
        SwitchCam = false;
        CinemachineVirtualCamera.LookAt = FrontSelfieCamTarget.transform;
    }

    bool SwitchCam;
    void ToggleSelfieCamera()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            SwitchCam = !SwitchCam;
            if (SwitchCam)
            {
                CinemachineVirtualCamera.LookAt = RearSelfieCamTarget.transform;
            }
            else
            {
                CinemachineVirtualCamera.LookAt = FrontSelfieCamTarget.transform;
            }
        }
    }
    public void AddJumpFroce() // Increase Y value after animation event call 
    {
        //Debug.Log("JumpForce : ");
        GameplayEntityLoader.instance.mainController.GetComponent<PlayerController>().UpdateVelocity();
    }
}
