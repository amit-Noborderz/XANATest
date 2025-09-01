using System.Collections;
using UnityEngine;
using Models;
using Photon.Pun;

public class WarpFunctionComponent : ItemComponent
{
    internal WarpFunctionComponentData warpFunctionComponentData;
    public static bool isPortalUsed;
    CharacterController characterControllerNew;

    public void Init(WarpFunctionComponentData warpFunctionComponentData)
    {
        this.warpFunctionComponentData = warpFunctionComponentData;

        GamificationComponentData.instance.warpComponentList.Add(this);
    }

    private void Start()
    {
        isPortalUsed = false;
    }

    private void OnCollisionEnter(Collision _other)
    {
        if (_other.gameObject.CompareTag("PhotonLocalPlayer") && _other.gameObject.GetComponent<PhotonView>().IsMine)
        {
            characterControllerNew = ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<CharacterController>();

            if (warpFunctionComponentData.isWarpPortalStart && !isPortalUsed)
                HandleWarp(warpFunctionComponentData.warpPortalStartKeyValue, false);
            else if (warpFunctionComponentData.isWarpPortalEnd && warpFunctionComponentData.isReversible && !isPortalUsed)
                HandleWarp(warpFunctionComponentData.warpPortalEndKeyValue, true);
        }
    }

    private void HandleWarp(string warpKey, bool isReversed)
    {
        StartCoroutine(PositionUpdating());
        ReferencesForGamePlay.instance.m_34player.GetComponent<SoundEffects>().PlaySoundEffects(SoundEffects.Sounds.PortalSound);
        isPortalUsed = true;

        characterControllerNew.enabled = false;
        GamificationComponentData.instance.buildingDetect.CameraEffect();

        GamificationComponentData.instance.playerControllerNew.transform.localPosition = GamificationComponentData.instance.WarpKeyDictHandler.GetPosition(warpKey, isReversed);
        characterControllerNew.enabled = true;
    }

    IEnumerator PositionUpdating()
    {

        yield return new WaitForSeconds(2f);
        isPortalUsed = false;
    }

    #region BehaviourControl
    private void StartComponent()
    {

    }
    private void StopComponent()
    {


    }

    public override void StopBehaviour()
    {
        if (isPlaying)
        {
            isPlaying = false;
            StopComponent();
        }
    }

    public override void PlayBehaviour()
    {
        isPlaying = true;
        StartComponent();
    }

    public override void ToggleBehaviour()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
            PlayBehaviour();
        else
            StopBehaviour();
    }
    public override void ResumeBehaviour()
    {
        PlayBehaviour();
    }

    public override void AssignItemComponentType()
    {
        _componentType = Constants.ItemComponentType.WarpFunctionComponent;
    }

    public override void CollisionExitBehaviour()
    {
        //throw new System.NotImplementedException();
    }

    public override void CollisionEnterBehaviour()
    {
        //CollisionEnter();
    }

    #endregion
}