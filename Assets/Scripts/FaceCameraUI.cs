using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCameraUI : MonoBehaviour
{
    //public static Transform selficamOtherAssign;
    private Transform mainCam;
    private Transform thirdPersonCam;
    private Transform firstPersonCam;
    //public GameObject selfieStick;
    //public Transform selfieCam;
    public Transform selfieCamera;

    public Transform FreelookCam;

    private Transform localTrans;
    //public bool oneTimeCall = false;

    void OnEnable()
    {
        if (PlayerSelfieController.Instance != null)
            PlayerSelfieController.Instance.SetNameOnSelfieOpen += SetSelfieCameraForName;
    }
    void OnDisable()
    {
        if (PlayerSelfieController.Instance != null)
            PlayerSelfieController.Instance.SetNameOnSelfieOpen -= SetSelfieCameraForName;
    }
    // Start is called before the first frame update
    void Start()
    {
        localTrans = GetComponent<Transform>();
        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            mainCam = GameplayEntityLoader.instance.PlayerCamera.transform;
            firstPersonCam = GameplayEntityLoader.instance.firstPersonCamera.transform;
        }
        else
        {
            if (GameplayEntityLoader.instance != null)
                mainCam = GameplayEntityLoader.instance.XanaPartyCamera.GetComponentInChildren<Camera>().transform;
        }
        thirdPersonCam = mainCam;
        if (PlayerSelfieController.Instance != null && PlayerSelfieController.Instance.m_IsSelfieFeatureActive)
        {
            selfieCamera = PlayerSelfieController.Instance.screenShotCameraCapture.transform.parent;
        }
        if (ReferencesForGamePlay.instance)
            FreelookCam = ReferencesForGamePlay.instance.FreeCam.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            if (firstPersonCam == null)
                return;
            if (firstPersonCam.gameObject.activeInHierarchy && mainCam != firstPersonCam)
                mainCam = firstPersonCam;
            else if (mainCam != thirdPersonCam && !firstPersonCam.gameObject.activeInHierarchy)
                mainCam = thirdPersonCam;
        }

        if (mainCam == null && GameplayEntityLoader.instance != null)
        {
            mainCam = GameplayEntityLoader.instance.XanaPartyCamera.GetComponentInChildren<Camera>().transform;
        }

        if (mainCam == null) return;

        if(GamePlayUIHandler.inst.isFreeCam)
        {
            localTrans.LookAt(2 * localTrans.position - FreelookCam.position);
        }
        else if (mainCam.gameObject.activeInHierarchy)
        {
            localTrans.LookAt(2 * localTrans.position - mainCam.position);
            //oneTimeCall = true;
        }
        else if (PlayerSelfieController.Instance != null && PlayerSelfieController.Instance.m_IsSelfieFeatureActive)
        {
            localTrans.LookAt(2 * localTrans.position - selfieCamera.position);
            //if (selfieStick != null && selfieStick.activeInHierarchy)
            //{
            //    if (oneTimeCall)
            //    {
            //        selficamOtherAssign = selfieCamOther;
            //        oneTimeCall = false;
            //        //GameObject[] objects = GameObject.FindGameObjectsWithTag("PhotonLocalPlayer");
            //        GameObject[] objects = Photon.Pun.Demo.PunBasics.MutiplayerController.instance.playerobjects.ToArray();

            //        for (int i = 0; i < objects.Length; i++)
            //        {
            //            if (objects[i].GetComponent<PhotonView>() && !objects[i].GetComponent<PhotonView>().IsMine)
            //            {
            //                objects[i].gameObject.GetComponent<ArrowManager>().PhotonUserName.gameObject.GetComponent<FaceCameraUI>().selfieCam = selfieCam;
            //                objects[i].gameObject.GetComponent<ArrowManager>().PhotonUserName.gameObject.GetComponent<FaceCameraUI>().selfieCamOther = selfieCamOther;
            //            }
            //        }
            //    }
            //    //localTrans.LookAt(2 * localTrans.position - selfieCam.position);
            //    localTrans.LookAt(2 * localTrans.position - selfieCamOther.position);
            //    print(localTrans.GetComponent<FaceCameraUI>().GetInstanceID() + "local trans");
            //}
        }
    }
    void SetSelfieCameraForName(Transform otherSelfieCam)
    {
        selfieCamera = otherSelfieCam;
    }
}
