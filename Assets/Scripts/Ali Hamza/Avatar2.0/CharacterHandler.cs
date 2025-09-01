using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//using static InventoryManager;

public class CharacterHandler : MonoBehaviour
{
    public static CharacterHandler instance;
    public AvatarGender activePlayerGender;
    public AvatarData maleAvatarData;
    public AvatarData femaleAvatarData;
    public AvatarData vt_Female;
    public AvatarData vt_Male;

    public GameObject playerNameCanvas;
    public GameObject playerPostCanvas;

    public GameObject vtuberLight;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        if(SaveCharacterProperties.instance && !string.IsNullOrEmpty(SaveCharacterProperties.instance.SaveItemList.gender))
            ActivateAvatarByGender(SaveCharacterProperties.instance.SaveItemList.gender);
    }

    public void ActivateAvatarBeforeInitialization(SavingCharacterDataClass _data = null)
    {
        string folderPath = GameManager.Instance.GetStringFolderPath();
        if (File.Exists(folderPath) && File.ReadAllText(folderPath) != "") //Check if data exist
        {
            SavingCharacterDataClass _CharacterData = new SavingCharacterDataClass();
            if (ConstantsHolder.isFixedHumanoid)
            {
                _CharacterData = _CharacterData.CreateFromJSON(XANASummitDataContainer.FixedAvatarJson);
            }
            else if (_data != null)
                _CharacterData = _data;
            else
            {
                _CharacterData = _CharacterData.CreateFromJSON(File.ReadAllText(folderPath));
            }
            ActivateAvatarByGender(_CharacterData.gender);
        }

        
    }

    public void ActivateAvatarByGender(string gender)
    {
        SetPlayerPositionAndRotation(gender);
        switch (gender)
        {
            case "Male":
                if (femaleAvatarData.avatar_parent)
                    femaleAvatarData.avatar_parent.gameObject.SetActive(false);

                if (vt_Female.avatar_parent)
                    vt_Female.avatar_parent.gameObject.SetActive(false);

                if (vt_Male.avatar_parent)
                    vt_Male.avatar_parent.gameObject.SetActive(false);

                if (maleAvatarData.avatar_parent)
                {
                    maleAvatarData.avatar_parent.gameObject.SetActive(true);
                    UpdateAvatarRefrences(maleAvatarData);
                }

                CharacterLightStatus(true);
                break;

            case "Female":
                if(maleAvatarData.avatar_parent)
                    maleAvatarData.avatar_parent.gameObject.SetActive(false);

                if (vt_Female.avatar_parent)
                    vt_Female.avatar_parent.gameObject.SetActive(false);

                if (vt_Male.avatar_parent)
                    vt_Male.avatar_parent.gameObject.SetActive(false);

                if (femaleAvatarData.avatar_parent)
                {
                    femaleAvatarData.avatar_parent.gameObject.SetActive(true);
                    UpdateAvatarRefrences(femaleAvatarData);
                }

                CharacterLightStatus(true);
                break;

            case "VTuber_Female":
                if (maleAvatarData.avatar_parent)
                    maleAvatarData.avatar_parent.gameObject.SetActive(false);

                if (femaleAvatarData.avatar_parent)
                    femaleAvatarData.avatar_parent.gameObject.SetActive(false);

                if (vt_Male.avatar_parent)
                    vt_Male.avatar_parent.gameObject.SetActive(false);

                if (vt_Female.avatar_parent)
                    vt_Female.avatar_parent.gameObject.SetActive(true);

                CharacterLightStatus(false);
                UpdateAvatarRefrences(vt_Female);
                break;

            case "VTuber_Male":
                if (maleAvatarData.avatar_parent)
                    maleAvatarData.avatar_parent.gameObject.SetActive(false);

                if (femaleAvatarData.avatar_parent)
                    femaleAvatarData.avatar_parent.gameObject.SetActive(false);

                if (vt_Female.avatar_parent)
                    vt_Female.avatar_parent.gameObject.SetActive(false);

                if (vt_Male.avatar_parent)
                    vt_Male.avatar_parent.gameObject.SetActive(true);

                CharacterLightStatus(false);
                UpdateAvatarRefrences(vt_Male);
                break;

            default:
                if (femaleAvatarData.avatar_parent)
                    femaleAvatarData.avatar_parent.gameObject.SetActive(false);

                if (vt_Female.avatar_parent)
                    vt_Female.avatar_parent.gameObject.SetActive(false);

                if (vt_Male.avatar_parent)
                    vt_Male.avatar_parent.gameObject.SetActive(false);

                if (maleAvatarData.avatar_parent)
                {
                    maleAvatarData.avatar_parent.gameObject.SetActive(true);
                    UpdateAvatarRefrences(maleAvatarData);
                }

                CharacterLightStatus(true);
                Debug.LogError("Gender Not Matched : " + gender);
            break;
        }

        //InventoryManager.upateAssetOnGenderChanged?.Invoke();
        //if(ConstantsHolder.xanaConstants.isStoreActive)

    }

    void SetPlayerPositionAndRotation(string gender)
    {
        string oldSelectedGender = activePlayerGender.ToString();//== AvatarGender.Female ? "1" : "0";

        // Check Old and new Selected are not same
        if (oldSelectedGender != gender) // 
        {
            // Copy old avatar pos, rotation and implement to new avatar 
            if (oldSelectedGender == "Female")
            {
                // Old is Female
                maleAvatarData.avatar_parent.transform.localPosition = femaleAvatarData.avatar_parent.transform.localPosition;
                maleAvatarData.avatar_parent.transform.localRotation = femaleAvatarData.avatar_parent.transform.localRotation;

                vt_Female.avatar_parent.transform.localPosition = femaleAvatarData.avatar_parent.transform.localPosition;
                vt_Female.avatar_parent.transform.localRotation = femaleAvatarData.avatar_parent.transform.localRotation;

                vt_Male.avatar_parent.transform.localPosition = femaleAvatarData.avatar_parent.transform.localPosition;
                vt_Male.avatar_parent.transform.localRotation = femaleAvatarData.avatar_parent.transform.localRotation;
            }
            else if (oldSelectedGender == "Male")
            {
                femaleAvatarData.avatar_parent.transform.localPosition = maleAvatarData.avatar_parent.transform.localPosition;
                femaleAvatarData.avatar_parent.transform.localRotation = maleAvatarData.avatar_parent.transform.localRotation;

                vt_Female.avatar_parent.transform.localPosition = maleAvatarData.avatar_parent.transform.localPosition;
                vt_Female.avatar_parent.transform.localRotation = maleAvatarData.avatar_parent.transform.localRotation;

                vt_Male.avatar_parent.transform.localPosition = maleAvatarData.avatar_parent.transform.localPosition;
                vt_Male.avatar_parent.transform.localRotation = maleAvatarData.avatar_parent.transform.localRotation;
            }
            else if (oldSelectedGender == "VTuber_Female")
            {
                maleAvatarData.avatar_parent.transform.localPosition = vt_Female.avatar_parent.transform.localPosition;
                maleAvatarData.avatar_parent.transform.localRotation = vt_Female.avatar_parent.transform.localRotation;

                femaleAvatarData.avatar_parent.transform.localPosition = vt_Female.avatar_parent.transform.localPosition;
                femaleAvatarData.avatar_parent.transform.localRotation = vt_Female.avatar_parent.transform.localRotation;

                vt_Male.avatar_parent.transform.localPosition = vt_Female.avatar_parent.transform.localPosition;
                vt_Male.avatar_parent.transform.localRotation = vt_Female.avatar_parent.transform.localRotation;
            }
            else if (oldSelectedGender == "VTuber_Male")
            {
                maleAvatarData.avatar_parent.transform.localPosition = vt_Male.avatar_parent.transform.localPosition;
                maleAvatarData.avatar_parent.transform.localRotation = vt_Male.avatar_parent.transform.localRotation;

                femaleAvatarData.avatar_parent.transform.localPosition = vt_Male.avatar_parent.transform.localPosition;
                femaleAvatarData.avatar_parent.transform.localRotation = vt_Male.avatar_parent.transform.localRotation;

                vt_Female.avatar_parent.transform.localPosition = vt_Male.avatar_parent.transform.localPosition;
                vt_Female.avatar_parent.transform.localRotation = vt_Male.avatar_parent.transform.localRotation;
            }

            if (ConstantsHolder.xanaConstants.isStoreActive)
                InventoryManager.instance.DeletePreviousItems();
        }
    }



    void CharacterLightStatus(bool status)
    {
       // return; // Not Using Lights for Vtuber handling from Shaders
        //Debug.Log("CharacterLightStatus: " + status);
        vtuberLight.SetActive(status);
    }
    private void UpdateAvatarRefrences(AvatarData _avatarData)
    {
        if (_avatarData.avatar_parent.GetComponent<EyesBlinking>() != null)
        {
            _avatarData.avatar_parent.GetComponent<EyesBlinking>().StoreBlendShapeValues();
            if (activePlayerGender != _avatarData.avatar_Gender)
            {
                StartCoroutine(_avatarData.avatar_parent.GetComponent<EyesBlinking>().BlinkingStartRoutine());
            }
        }

        activePlayerGender = _avatarData.avatar_Gender;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.mainCharacter = _avatarData.avatar_parent;
            GameManager.Instance.m_ChHead = _avatarData.avatar_face.gameObject;
            GameManager.Instance.m_CharacterAnimator = _avatarData.avatar_animator;
            GameManager.Instance.avatarController = _avatarData.avatar_parent.GetComponent<AvatarController>();
            GameManager.Instance.characterBodyParts = _avatarData.avatar_parent.GetComponent<CharacterBodyParts>();
            GameManager.Instance.eyesBlinking = _avatarData.avatar_parent.GetComponent<EyesBlinking>();


            if (!ConstantsHolder.xanaConstants.isStoreActive)
            {
                GameManager.Instance.m_CharacterAnimator.SetBool("Action", true);
                GameManager.Instance.ActorManager.Init();
            }
            else
            {
                GameManager.Instance.m_CharacterAnimator.SetBool("IdleMenu", true);
            }

        }
        if (SaveCharacterProperties.instance != null && GameManager.Instance != null)
        {
            SaveCharacterProperties.instance.charcterBodyParts = GameManager.Instance.characterBodyParts;
            SaveCharacterProperties.instance.characterController = GameManager.Instance.avatarController;

            string AvatarName = SaveCharacterProperties.instance.characterController.name;

            if (AvatarName.Contains("Vtuber"))
            {
                if(AvatarName.Contains("Female"))
                    SaveCharacterProperties.instance.SaveItemList.gender = "VTuber_Female";
                else 
                    SaveCharacterProperties.instance.SaveItemList.gender = "VTuber_Male";
            }
            else if (AvatarName.Contains("Female"))
                SaveCharacterProperties.instance.SaveItemList.gender = "Female";
            else
                SaveCharacterProperties.instance.SaveItemList.gender = "Male";
        }
        
        if (playerNameCanvas && playerPostCanvas && !ConstantsHolder.xanaConstants.isStoreActive)
        {
            UpdateNameAndPostTarget(_avatarData.avatar_parent);   // Update the target of the name and post canvas to the active player
        }
    }


    private void UpdateNameAndPostTarget(GameObject activePlayer)
    {
        playerNameCanvas.GetComponent<FollowUser>().targ = activePlayer.transform;
        playerNameCanvas.GetComponent<FollowUser>().newplayerTransform = activePlayer.transform;

        playerPostCanvas.GetComponent<LookAtCamera>()._playerTransform = activePlayer.transform;
        playerPostCanvas.GetComponent<LookAtCamera>().newplayerTransform = activePlayer.transform;

        if (!playerNameCanvas.activeInHierarchy)
            playerNameCanvas.SetActive(true);
        if (!playerPostCanvas.activeInHierarchy)
            playerPostCanvas.SetActive(true);
    }

    public AvatarData GetActiveAvatarData()
    {
        if (activePlayerGender == AvatarGender.Male)
        {
            return maleAvatarData;
        }
        else
        {
            return femaleAvatarData;
        }
    }


    [Serializable]
    public class AvatarData
    {
        public AvatarGender avatar_Gender;
        public GameObject avatar_parent;
        public SkinnedMeshRenderer avatar_body;
        public SkinnedMeshRenderer avatar_face;
        public Animator avatar_animator;
        public Texture /*DShirt_Texture, DPent_Texture, DShoe_Texture,*/ DEye_texture, DFace_Texture, DSkin_Texture;
    }
}
