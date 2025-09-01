using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BD
{
    public class XanaFightingPlayer : MonoBehaviour
    {
        //public Renderer[] rds;
        //public string cloth;
        //public bool isLoadingScreenPlayer;
        //public int loadingPlayerIndex;
        //public bool isWinningAnimationPlayer;
        //void Start()
        //{

        //}
        //private void OnEnable()
        //{
        //    if (isLoadingScreenPlayer)
        //    {
        //        if (UFE.gameMode == UFE3D.GameMode.TrainingRoom)
        //        {
        //            if (loadingPlayerIndex == 1)
        //            {
        //                SetTexture(1);
        //            }
        //            else
        //            {
        //                SetTexture(2);
        //            }
        //        }
        //        else if (UFE.gameMode == UFE3D.GameMode.VersusMode)
        //        {
        //            if (loadingPlayerIndex == 1)
        //            {
        //                SetTexture(1);
        //            }
        //            else
        //            {
        //                SetTexture(2);
        //            }
        //        }
        //        else
        //        {
        //            if (loadingPlayerIndex == 1)
        //            {
        //                if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        //                {
        //                    SetTexture(1);
        //                }
        //                else
        //                {
        //                    SetTexture(1);
        //                }
        //            }
        //            else
        //            {
        //                if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        //                {
        //                    SetTexture(2);
        //                }
        //                else
        //                {
        //                    SetTexture(2);
        //                }
        //            }
        //        }
        //        return;
        //    }
        //    if (FightingGameManager.instance.p1WonRound > FightingGameManager.instance.p2WonRound)
        //    {
        //        SetTexture(1);
        //    }
        //    else
        //    {
        //        SetTexture(2);
        //    }
        //}
        //public void SetTexture(int pIndex)
        //{
        //    Texture2D[] texture = pIndex == 1 ? FightingGameManager.instance.p1Textures : FightingGameManager.instance.p2Textures;
        //    Mesh ms = pIndex == 1 ? FightingGameManager.instance.p1Mesh: FightingGameManager.instance.p2Mesh;
        //    rds[0].GetComponent<SkinnedMeshRenderer>().sharedMesh = ms;
        //    for (int i = 0; i < rds.Length; i++)
        //    {
        //        Material mat = new Material(rds[i].material);
        //        mat.mainTexture = texture[i];
        //        rds[i].material = mat;
        //    }
        //}
        //private void OnTransformParentChanged()
        //{
        //    Debug.LogError("OnTransformParentChanged");
        //    if (transform.parent.GetComponent<ControlsScript>())
        //    {
        //        ControlsScript controlsScript = transform.parent.GetComponent<ControlsScript>();
        //        Debug.LogError("playernum: " + controlsScript.playerNum + " actor number: " + PhotonNetwork.LocalPlayer.ActorNumber);
        //        if (controlsScript.playerNum == 1)
        //        {
        //            if (UFE.gameMode == UFE3D.GameMode.TrainingRoom)
        //            {
        //                SetTexture(1);
        //                /*avatarController.isLoadStaticClothFromJson = false;
        //                avatarController.staticPlayer = true;
        //                controlsScript.myInfo.characterName = PlayerPrefs.GetString("PlayerName").ToUpper();*/
        //            }
        //            else if (UFE.gameMode == UFE3D.GameMode.VersusMode)
        //            {
        //                SetTexture(1);
        //                /*avatarController.isLoadStaticClothFromJson = false;
        //                avatarController.staticPlayer = true;
        //                controlsScript.myInfo.characterName = PlayerPrefs.GetString("PlayerName").ToUpper();*/
        //            }
        //            else
        //            {
        //                if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        //                {
        //                    SetTexture(1);
        //                    /*cloth = avatarController.staticClothJson = FightingGameManager.instance.player1Data.clothJson;
        //                    controlsScript.myInfo.characterName = FightingGameManager.instance.player1Data.name.ToUpper();*/
        //                }
        //                else
        //                {
        //                    SetTexture(1);
        //                    /*cloth = avatarController.staticClothJson = FightingGameManager.instance.player2Data.clothJson;
        //                    controlsScript.myInfo.characterName = FightingGameManager.instance.player2Data.name.ToUpper();*/
        //                }
        //            }
        //            /*SavingCharacterDataClass _CharacterData = new SavingCharacterDataClass();
        //            _CharacterData = _CharacterData.CreateFromJSON(FightingGameManager.instance.PlayerClothJson);
        //            controlsScript.fightingPlayer.profile = _CharacterData.profile;
        //            controlsScript.fightingPlayer.speed = _CharacterData.speed;
        //            controlsScript.fightingPlayer.stamina = _CharacterData.stamina;
        //            controlsScript.fightingPlayer.punch = _CharacterData.punch;
        //            controlsScript.fightingPlayer.kick = _CharacterData.kick;
        //            controlsScript.fightingPlayer.defence = _CharacterData.defence;
        //            controlsScript.fightingPlayer.special_move = _CharacterData.special_move;
        //            FightingDataManager.Instance.player2 = controlsScript.fightingPlayer;*/
        //        }
        //        else
        //        {
        //            if (UFE.gameMode == UFE3D.GameMode.TrainingRoom)
        //            {
        //                SetTexture(2);
        //                /*avatarController.isLoadStaticClothFromJson = true;
        //                avatarController.staticPlayer = false;
        //                controlsScript.myInfo.characterName = ConstantsHolder.xanaConstants.defaultFightingName.ToUpper();*/
        //            }
        //            else if (UFE.gameMode == UFE3D.GameMode.VersusMode)
        //            {
        //                SetTexture(2);
        //                /*avatarController.isLoadStaticClothFromJson = true;
        //                avatarController.staticPlayer = false;
        //                controlsScript.myInfo.characterName = ConstantsHolder.xanaConstants.defaultFightingName.ToUpper();*/
        //            }
        //            else
        //            {
        //                if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        //                {
        //                    SetTexture(2);
        //                    /*cloth = avatarController.staticClothJson = FightingGameManager.instance.player1Data.clothJson;
        //                    controlsScript.myInfo.characterName = FightingGameManager.instance.player1Data.name.ToUpper();*/
        //                }
        //                else
        //                {
        //                    SetTexture(2);
        //                    /*cloth = avatarController.staticClothJson = FightingGameManager.instance.player2Data.clothJson;
        //                    controlsScript.myInfo.characterName = FightingGameManager.instance.player2Data.name.ToUpper();*/
        //                }
        //            }
        //        }
        //    }
        //}
    }
}