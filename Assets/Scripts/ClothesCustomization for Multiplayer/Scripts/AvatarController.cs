using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;
using System;
using Random = UnityEngine.Random;
using UnityEditor;
using static InventoryManager;
using Photon.Pun.Demo.PunBasics;
using MagicaCloth2;

public class AvatarController : MonoBehaviour
{
    #region public Delegates
    public delegate void CharacterLoaded();
    public static event CharacterLoaded characterLoaded;
    #endregion

    #region public Var
    public GameObject AvatarAndClothParent;
    public bool isPlayerAvatar = false;
    public bool IsInit = false;
    public bool staticPlayer;
    public bool isWearOrNot = false;
    public bool isClothLoaded = false;
    public bool isLoadStaticClothFromJson;
    public string staticClothJson;
    public string clothJson;
    public string presetValue;
    public GameObject wornHair, wornPant, wornShirt, wornShoes, wornEyeWearable, wornGloves, wornChain;
    public GameObject[] wornEyebrow;
    public NFTColorCodes _nftAvatarColorCodes;
    public CharacterBodyParts characterBodyParts;
    public SavingCharacterDataClass _PCharacterData = new SavingCharacterDataClass();
    [SerializeField] RuntimeAnimatorController ArAnimator;

    public List<Item> ClothsToBeLoaded = new List<Item>();
    #endregion

    #region var Hide Inspector
    [HideInInspector]
    public int wornHairId, hairColorPaletteId, wornPantId, wornShirtId, wornShoesId, wornEyewearableId, skinId,
        faceId, eyeBrowId, eyeBrowColorPaletteId, eyesId, eyesColorId, eyesColorPaletteId, noseId, lipsId,
        lipsColorId, lipsColorPaletteId, bodyFat, makeupId, eyeLashesId, wornGlovesId, wornChainId;
    //[HideInInspector]
    public List<Texture> masks = new List<Texture>();
    #endregion

    #region private var
    public Stitcher stitcher;
    private DefaultClothDatabase itemDatabase;
    private string sceneName;
    private Color presetHairColor;
    AddressableDownloader addressableDownloader;
    ConstantsHolder xanaConstants;
    //FriendAvatarController friendController;
    #endregion

    #region Commented Code
    //public SkinnedMeshRenderer Body;
    //public SkinnedMeshRenderer head;
    //NFT avatar color codes
    //public bool isVisibleOnCam = false;
    #endregion

    #region Unity Default Functions
    private void Awake()
    {
        stitcher = new Stitcher();
        sceneName = SceneManager.GetActiveScene().name;
        characterBodyParts = this.GetComponent<CharacterBodyParts>();
        itemDatabase = DefaultClothDatabase.instance;
        if (sceneName.Equals("ARModuleActionScene") || sceneName.Equals("ARModuleRoomScene") || sceneName.Equals("ARModuleRealityScene")) // Manually scalling object to match old XANA character 1.0
        {
            transform.localScale *= 2;
        }

        //if(this.GetComponent<FriendAvatarController>())
        //    friendController = this.GetComponent<FriendAvatarController>();
    }
    private void Start()
    {
        addressableDownloader = AddressableDownloader.Instance;
        xanaConstants = ConstantsHolder.xanaConstants;

        //added this code to resolve naked avatar 
        AvatarAndClothParent.SetActive(false);

    }

    public void OnLogout()
    {
        //added this code to resolve naked avatar 
        AvatarAndClothParent.SetActive(false);
    }

    public async void OnEnable()
    {
        //BoxerNFTEventManager.OnNFTequip += EquipNFT;
        //BoxerNFTEventManager.OnNFTUnequip += UnequipNFT;

        //Debug.Log("enabled  " + IsInit);
        //if (IsInit) // init avatar according to the Avatar Type (Friend/Self player). 
        //{

        //    if (!isPlayerAvatar) // to check is friend or player avatar in Home Scene.
        //    {
        //        SetAvatarClothDefault(this.gameObject, "Male");
        //    }
        //    else
        //    {
        //        StartCoroutine(SetAvatarDefaultClothDelay(this.gameObject, "Male"));
        //    }
        //}
        if (xanaConstants == null)
            xanaConstants = ConstantsHolder.xanaConstants;
        if (addressableDownloader == null)
            addressableDownloader = AddressableDownloader.Instance;

        MainSceneEventHandler.OnSucessFullLogout += OnLogout;

        if (xanaConstants != null)
        {
            if (xanaConstants.isStoreActive)
            {
                return;
            }

            if (sceneName.Contains("Home") || sceneName.Contains("UGC") && ConstantsHolder.ClothLoadingCheckWhenReturning) // For Home
            {
                if (isPlayerAvatar)
                {
                    await Custom_InitializeAvatar();
                    await Task.Delay(1000);
                    AvatarAndClothParent.SetActive(true);

                    // In case Of VT we have disable the Meshes
                    EnableClothMesh();
                }
            }

            //if (sceneName.Contains("Home") || sceneName.Contains("UGC")) // For Home
            //{
            //    //Fighter Module Removed By Ahsan
            //    //if (xanaConstants.isNFTEquiped)
            //    //{
            //    //    EquipNFT();
            //    //}
            //    //else
            //    //{
            //    if (isPlayerAvatar)
            //        Custom_InitializeAvatar();
            //    //}
            //    //if (xanaConstants.isNFTEquiped)
            //    //{
            //    //    this.GetComponent<SwitchToBoxerAvatar>().OnNFTEquipShaderUpdate();
            //    //}

            //}
            //else
            //{
            //    //commented below lines as cloths are downling 2 times now cloths are downloading from RPCCallForBufferPlayers.cs so it is not required.
            //   // Invoke(nameof(Deley_Custom_InitializeAvatar), 0.0f);

            //    //if (xanaConstants.isNFTEquiped) //Fighter Module Removed By Ahsan
            //    //{
            //    //    this.GetComponent<SwitchToBoxerAvatar>().OnNFTEquipShaderUpdate();
            //    //}
            //}

        }
    }

    //void Deley_Custom_InitializeAvatar()
    //{
    //    Custom_InitializeAvatar();
    //}
    //Fighter Module Removed By Ahsan
    private void OnDisable()
    {
        MainSceneEventHandler.OnSucessFullLogout-= OnLogout;

        //BoxerNFTEventManager.OnNFTequip -= EquipNFT;
        //BoxerNFTEventManager.OnNFTUnequip -= UnequipNFT;
    }
    #endregion

    /// <summary>
    /// Setting Avatar default clothes with delay if it is not friend avatar
    /// </summary>
    /// <param name="_obj">GameObject of the avatar</param>
    /// <param name="_gender"> Gender of the avatar</param>
    /// <returns></returns>
    //IEnumerator SetAvatarDefaultClothDelay(GameObject _obj, string _gender)
    //{
    //    yield return new WaitForEndOfFrame();  //WaitForSeconds(0.1f);
    //    if (SceneManager.GetActiveScene().name != "Home" && !isWearOrNot)
    //    {
    //        //Debug.LogError(_obj.GetComponent<PhotonView>().ViewID + ":Wear Default cloth manually");
    //        SetAvatarClothDefault(_obj, _gender);
    //    }
    //}

    /// <summary>
    /// To Get Ready Avatar with NFT
    /// </summary>
    /// <param name="canModifyFile"></param>
    //Fighter Module Removed By Ahsan
    //public void EquipNFT(bool canModifyFile = false)
    //{
    //    // .Step to do After Equipt NFT
    //    // .Add Data into NFTBoxerJson 
    //    // .Disable Store BTN
    //    // .Make All Items Default [Eyebrow, EyeBrowPoints, Bones, BlendShapes, Items Color[hair,eyebrow,eyes,lip,skin]]
    //    // .Reset BodyFat
    //    // .Read Data From Json & load its Properties

    //    xanaConstants.isNFTEquiped = true;
    //    xanaConstants.isHoldCharacterNFT = true;

    //    if (characterBodyParts != null)
    //    {
    //        ResetBonesDefault(characterBodyParts);
    //        characterBodyParts.DefaultBlendShapes(this.gameObject);

    //        characterBodyParts.DefaultTexture(false);
    //        ResizeClothToBodyFat(this.gameObject, 0);

    //        characterBodyParts.head.materials[2].SetInt("_Active", 0);
    //        characterBodyParts.body.materials[0].SetInt("_Active", 0);

    //        //extra blendshape added to character to build muscles on Character
    //        characterBodyParts.head.SetBlendShapeWeight(54, 100);
    //        characterBodyParts.body.SetBlendShapeWeight(0, 100);

    //        BoxerNFTEventManager.OnNFTequipShaderUpdate?.Invoke();

    //        InitializeAvatar(canModifyFile);
    //    }
    //}

    /// <summary>
    /// To Unequip NFT of the avatar
    /// </summary>
    //Fighter Module Removed By Ahsan
    //public void UnequipNFT()
    //{
    //    xanaConstants.isNFTEquiped = false;
    //    ResetBonesDefault(characterBodyParts);
    //    characterBodyParts.DefaultBlendShapes(this.gameObject);

    //    characterBodyParts.DefaultTexture(false);
    //    ResizeClothToBodyFat(this.gameObject, 0);

    //    if (wornEyeWearable)
    //        UnStichItem("EyeWearable");

    //    if (wornChain)
    //        UnStichItem("Chain");

    //    if (wornGloves)
    //    {
    //        characterBodyParts.TextureForGlove(null);
    //        UnStichItem("Glove");
    //    }

    //    characterBodyParts.head.materials[2].SetInt("_Active", 1);
    //    characterBodyParts.body.materials[0].SetInt("_Active", 1);

    //    BoxerNFTEventManager.OnNFTUnequipShaderUpdate?.Invoke();
    //    BoxerNFTEventManager.NFTLightUpdate?.Invoke(LightPresetNFT.DefaultSkin);
    //    InitializeAvatar();
    //}

    /// <summary>
    /// Set Avatar with deault clothes
    /// </summary>
    /// <param name="applyOn"> Avatar to be applied</param>
    /// <param name="_gender"> Gender on which type is to be</param>
    public void SetAvatarClothDefault(GameObject applyOn, string _gender)
    {
        if (_gender.IsNullOrEmpty()) // In Case of VTuber Avatar -- Sending Null String
            return;
        IsInit = false;
        WearDefaultItem("Legs", applyOn.gameObject, _gender);
        WearDefaultItem("Chest", applyOn.gameObject, _gender);
        WearDefaultItem("Feet", applyOn.gameObject, _gender);
        WearDefaultItem("Hair", applyOn.gameObject, _gender);
        applyOn.GetComponent<CharacterBodyParts>().DefaultTexture(true, _gender); // Setting Default Texture according to Gender
    }

    /// <summary>
    /// To Inialize Character.
    ///  - Intilaze Store item 
    ///  - Intilaze Character customization (bones, morphes)
    /// </summary>
    public async void InitializeAvatar(bool canWriteFile = false, SavingCharacterDataClass _tempdata = null)
    {
        while (!ConstantsHolder.isAddressableCatalogDownload)
        {
            await Task.Yield();
        }
        //Fighter Module Removed By Ahsan
        //if (canWriteFile && /*xanaConstants.isHoldCharacterNFT &&*/ xanaConstants.isNFTEquiped) // Checking is avatar is Breaking Down
        //{
        //    BoxerNFTDataClass nftAttributes = new BoxerNFTDataClass();
        //    string _Path = Application.persistentDataPath + xanaConstants.NFTBoxerJson;
        //    nftAttributes = nftAttributes.CreateFromJSON(File.ReadAllText(_Path));
        //    CreateOrUpdateBoxerFile(nftAttributes);
        //}
       await Custom_InitializeAvatar(_tempdata);

        //added this code to resolve naked avatar 
        await Task.Delay(1000);

        // In Case of Saudi Event we are redirecting player to the Event Dome when Post Loading completed and enabled Meshes From There
        if (ConstantsHolder.xanaConstants.saudiEventDomeId < 0 )
        { 
            AvatarAndClothParent.SetActive(true);

            // In case Of VT we have disable the Meshes
            EnableClothMesh();
        }
    }

    public void EnableClothMesh()
    {
        if (wornHair && wornHair.GetComponent<SkinnedMeshRenderer>())
            wornHair.GetComponent<SkinnedMeshRenderer>().enabled = true;

        if (wornShirt && wornShirt.GetComponent<SkinnedMeshRenderer>())
            wornShirt.GetComponent<SkinnedMeshRenderer>().enabled = true;

        if (wornPant && wornPant.GetComponent<SkinnedMeshRenderer>())
            wornPant.GetComponent<SkinnedMeshRenderer>().enabled = true;
    }

    public bool isStitchedSuccessfully;
    public SavingCharacterDataClass clothDataClass = new SavingCharacterDataClass();
    public List<GameObject> clothsList = new List<GameObject>();
    public Coroutine clothsStichedOrNotCoroutine;


    public void HandleCharacterParts(bool active)
    {
        /*if (characterBodyParts.eyeShadow != null)
        {
            characterBodyParts.eyeShadow.enabled = active;
        }
        characterBodyParts.head.enabled = characterBodyParts.body.enabled = active;*/
    }

    public void HandleCloths(bool active)
    {
        /*if (wornHair != null)
        {
            wornHair.SetActive(active);
        }
        if (wornShirt != null)
        {
            wornShirt.SetActive(active);
        }
        if (wornPant != null)
        {
            wornPant.SetActive(active);
        }
        if (wornShoes != null)
        {
            wornShoes.SetActive(active);
        }*/
    }
    public bool isClothStichedOrNot(List<string> clothsList, AvatarController avatar)
    {
        if (clothsList.Count == 0)
        {
            return true;
        }
        string pant = "";
        string shirt = "";
        string hair = "";
        string shoes = "";
        if (avatar.wornPant != null)
        {
            pant = avatar.wornPant.name;
        }
        if (avatar.wornShirt != null)
        {
            shirt = avatar.wornShirt.name;
        }
        if (avatar.wornHair != null)
        {
            hair = avatar.wornHair.name;
        }
        if (avatar.wornShoes != null)
        {
            shoes = avatar.wornShoes.name;
        }
        return clothsList[0] == pant && clothsList[1] == shirt && clothsList[2] == hair;
    }

    /// <summary>
    /// Downloading Random Preset
    /// </summary>
    /// <param name="_CharacterData"> GameObject on which data to be applied</param>
    /// <param name="_rand"> Random numbe of the preset</param>
    /*async*/ void DownloadRandomPresets(SavingCharacterDataClass _CharacterData, int _rand)
    {
        CharacterHandler.instance.ActivateAvatarByGender(characterBodyParts.randomPresetData[_rand].GenderType);
        SetAvatarClothDefault(GameManager.Instance.avatarController.gameObject, characterBodyParts.randomPresetData[_rand].GenderType); //Set Default Cloth and Set texture according to it.
        if (_CharacterData.myItemObj == null || _CharacterData.myItemObj.Count == 0)
        {
            for (int i = 0; i < 4; i++)
                _CharacterData.myItemObj.Add(new Item(0, "", "", "", ""));
        }
        var randomPresetData = characterBodyParts.randomPresetData[_rand];
        _CharacterData.myItemObj[0].ItemName = randomPresetData.PantPresetData.ObjectName;
        _CharacterData.myItemObj[0].ItemType = randomPresetData.PantPresetData.ObjectType;

        _CharacterData.myItemObj[1].ItemName = randomPresetData.ShirtPresetData.ObjectName;
        _CharacterData.myItemObj[1].ItemType = randomPresetData.ShirtPresetData.ObjectType;

        _CharacterData.myItemObj[2].ItemName = randomPresetData.HairPresetData.ObjectName;
        _CharacterData.myItemObj[2].ItemType = randomPresetData.HairPresetData.ObjectType;

        _CharacterData.myItemObj[3].ItemName = randomPresetData.ShoesPresetData.ObjectName;
        _CharacterData.myItemObj[3].ItemType = randomPresetData.ShoesPresetData.ObjectType;

        //CharacterHandler.instance.ActivateAvatarByGender(randomPresetData.GenderType);

        if (_CharacterData.myItemObj.Count > 0)
        {
            for (int i = 0; i < _CharacterData.myItemObj.Count; i++)
            {
                if (!string.IsNullOrEmpty(_CharacterData.myItemObj[i].ItemName))
                {
                    HashSet<string> itemTypes = new HashSet<string> { "Legs", "Chest", "Feet", "Hair", "EyeWearable", "Glove", "Chain" };

                    if (itemTypes.Any(item => _CharacterData.myItemObj[i].ItemType.Contains(item)))
                    {
                        if (!_CharacterData.myItemObj[i].ItemName.ToLowerInvariant().Contains("md"))
                        {
                            var item = _CharacterData.myItemObj[i];
                            var gender = _CharacterData.gender ?? "Male";
                            var avatarController = this.gameObject.GetComponent<AvatarController>();
                            //await addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, item.ItemType, gender, avatarController, Color.clear);
                            addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, item.ItemType, gender, avatarController, Color.clear);
                        }
                        else
                        {
                            if (PlayerPrefs.HasKey("Equiped") || ConstantsHolder.isNFTEquiped)
                            {
                                if (_CharacterData.myItemObj[i].ItemType.IndexOf("Chest", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornShirt)
                                        UnStichItem("Chest");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Hair", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornHair)
                                        UnStichItem("Hair");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Legs", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornPant)
                                        UnStichItem("Legs");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Feet", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornShoes)
                                        UnStichItem("Feet");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("EyeWearable", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornEyeWearable)
                                        UnStichItem("EyeWearable");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Glove", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornGloves)
                                        UnStichItem("Glove");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Chain", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornChain)
                                        UnStichItem("Chain");
                                }
                            }
                            else
                            {
                                var item = _CharacterData.myItemObj[i];
                                var gender = _CharacterData.gender ?? "Male";
                                WearDefaultItem(item.ItemType, this.gameObject, gender);
                            }
                        }
                    }
                    else
                    {
                        var item = _CharacterData.myItemObj[i];
                        var gender = _CharacterData.gender ?? "Male";
                        WearDefaultItem(item.ItemType, this.gameObject, gender);
                    }
                }
            }
        }

        _CharacterData.gender = randomPresetData.GenderType;
        _CharacterData.avatarType = "NewAvatar";
        var filePath = Path.Combine(Application.persistentDataPath, "loginAsGuestClass.json");
        File.WriteAllText(filePath, JsonUtility.ToJson(_CharacterData));

        GameManager.Instance.selectedPresetData = JsonUtility.ToJson(_CharacterData);

        if (_CharacterData.HairColor != null)
            xanaConstants.isPresetHairColor = true;

        SavePresetOnServer(_CharacterData);
    }

    /// <summary>
    /// To Save player asset file on server
    /// </summary>
    /// <param name="savingCharacterDataClass">Player assets data class</param>
    void SavePresetOnServer(SavingCharacterDataClass savingCharacterDataClass)
    {
        if (PlayerPrefs.GetInt("IsLoggedIn") == 1)
        {
            var filePath = Path.Combine(Application.persistentDataPath, "logIn.json");
            File.WriteAllText(filePath, JsonUtility.ToJson(savingCharacterDataClass));
            ServerSideUserDataHandler.Instance.CreateUserOccupiedAsset(() => { });
        }
    }

    /// <summary>
    /// Initializing Avatar with json file from the server.
    /// </summary>
    async Task Custom_InitializeAvatar(SavingCharacterDataClass _data = null)
    {
        
        //  await Task.Delay(100);
        if (isLoadStaticClothFromJson)
        {
            //  Debug.Log("Buildding character from local json... ");
            await BuildCharacterFromLocalJson();
            return;
        }
        //Debug.Log("Buildding character from Saved Path ... ");
        string folderPath = GameManager.Instance.GetStringFolderPath();
        if (File.Exists(folderPath) && File.ReadAllText(folderPath) != "") //Check if data exist
        {
            SavingCharacterDataClass _CharacterData = new SavingCharacterDataClass();


            if (ConstantsHolder.isFixedHumanoid)
            {
                _CharacterData = _CharacterData.CreateFromJSON(XANASummitDataContainer.FixedAvatarJson);
                clothJson = XANASummitDataContainer.FixedAvatarJson;
            }
            else if (_data != null)
                _CharacterData = _data;
            else
            {
                _CharacterData = _CharacterData.CreateFromJSON(File.ReadAllText(folderPath));
                clothJson = File.ReadAllText(folderPath);
            }

            _PCharacterData = _CharacterData;

            var gender = _CharacterData.gender ?? "Male";
            var avatarController = this.gameObject.GetComponent<AvatarController>();
            sceneName = SceneManager.GetActiveScene().name; // updating scene name if scene changed.
            if (sceneName.Equals("Home") || sceneName.Equals("UGC")) // for store/ main menu
            {
                if (string.IsNullOrEmpty(_CharacterData.avatarType) || _CharacterData.avatarType == "OldAvatar")
                {
                    int _rand = Random.Range(0, characterBodyParts.randomPresetData.Length);
                    DownloadRandomPresets(_CharacterData, _rand);
                }
                else
                {
                    CharacterHandler.instance.ActivateAvatarByGender(_CharacterData.gender);

                    //SetAvatarClothDefault(gameObject, _CharacterData.gender);   //Zeel Commented why you have to set default cloths every time

                    if (_CharacterData.myItemObj.Count > 0)
                    {
                        for (int i = 0; i < _CharacterData.myItemObj.Count; i++)
                        {
                            var item = _CharacterData.myItemObj[i];
                            string type = _CharacterData.myItemObj[i].ItemType;

                            if (WearSameCloths(item.ItemName, type))
                                continue;


                            if (!string.IsNullOrEmpty(item.ItemName) &&
                                !item.ItemName.Contains("default", System.StringComparison.CurrentCultureIgnoreCase))
                            {

                                HashSet<string> itemTypes = new HashSet<string> { "Legs", "Chest", "Feet", "Hair", "EyeWearable", "Glove", "Chain" };
                                if (itemTypes.Any(item => type.Contains(item)))
                                {
                                    //getHairColorFormFile = true;
                                    if (!item.ItemName.Contains("md", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (type.Contains("Hair"))
                                        {
                                            if (!string.IsNullOrEmpty(_CharacterData.hairItemData) && _CharacterData.hairItemData.Contains("No hair") && wornHair)
                                                UnStichItem("Hair");
                                            else if (gameObject.activeInHierarchy)
                                                await addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, type, gender, avatarController, _CharacterData.HairColor);
                                        }
                                        else if (gameObject.activeInHierarchy)
                                           await addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, type, gender, avatarController, Color.clear);
                                    }
                                    else
                                    {
                                        if (PlayerPrefs.HasKey("Equiped") || ConstantsHolder.isNFTEquiped)
                                        {
                                            if (item.ItemType.Contains("Chest") && wornShirt)
                                                UnStichItem("Chest");

                                            else if (item.ItemType.Contains("Hair") && wornHair)
                                                UnStichItem("Hair");

                                            else if (item.ItemType.Contains("Legs") && wornPant)
                                                UnStichItem("Legs");
                                            
                                            else if (item.ItemType.Contains("Feet") && wornShoes)
                                                UnStichItem("Feet");
                                            
                                            else if (item.ItemType.Contains("EyeWearable") && wornEyeWearable)
                                                UnStichItem("EyeWearable");
                                          
                                            else if (item.ItemType.Contains("Glove") && wornGloves)
                                                UnStichItem("Glove");
                                            
                                            else if (item.ItemType.Contains("Chain") && wornChain)
                                                UnStichItem("Chain");
                                        }
                                        else
                                        {
                                            WearDefaultItem(type, this.gameObject, gender);
                                        }
                                    }
                                }
                                else
                                {
                                    WearDefaultItem(type, this.gameObject, gender);
                                }
                            }
                            else // wear the default item of that specific part.
                            {
                                if (ConstantsHolder.isNFTEquiped && type.Contains("Chest"))
                                {
                                    if (wornShirt)
                                        UnStichItem("Chest");
                                    characterBodyParts.TextureForShirt(null);
                                }
                                else
                                {
                                    //As one preset not have footwear, so unstitch the previous pair
                                    if (_CharacterData.myItemObj[0].ItemName == "Boy_Pant_V009" && _CharacterData.myItemObj[1].ItemName == "Boy_Shirt_V009")
                                    {
                                        if (item.ItemType.Contains("Feet"))
                                            UnStichItem("Feet");
                                    }
                                    else
                                    {
                                        WearDefaultItem(type, this.gameObject, gender);
                                    }
                                }
                            }
                        }
                    }
                    // Added By WaqasAhmad
                    // When User Reset From Store 
                    // _CharacterData file clear & no Data is available
                    // Implemented Default Cloths
                    else
                    {
                        WearDefaultItem("Legs", this.gameObject, gender);
                        WearDefaultItem("Chest", this.gameObject, gender);
                        WearDefaultItem("Feet", this.gameObject, gender);
                        WearDefaultItem("Hair", this.gameObject, gender);

                        if (wornEyeWearable)
                            UnStichItem("EyeWearable");
                        if (wornChain)
                            UnStichItem("Chain");
                        if (wornGloves)
                        {
                            UnStichItem("Glove");
                            characterBodyParts.TextureForGlove(null);
                        }
                    }
                    if (_CharacterData.charactertypeAi == true /*&& !UGCManager.isSelfieTaken*/)
                    {
                        ApplyAIData(_CharacterData, this.gameObject);
                    }

                    characterBodyParts.LoadBlendShapes(_CharacterData, this.gameObject);

                    if (!String.IsNullOrEmpty(_CharacterData.eyeTextureName) && gameObject.activeInHierarchy)
                    {
                        addressableDownloader.DownloadAddressableTexture(_CharacterData.eyeTextureName, this.gameObject, CurrentTextureType.EyeLense);
                    }
                }


                #region Xana Avatar 1.0  //--> remove for xana avatar2.0


                //if (_CharacterData.eyeLashesName != "" && _CharacterData.eyeLashesName != null)
                //{
                //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.eyeLashesName, this.gameObject, CurrentTextureType.EyeBrowPoints));
                //}
                if (!string.IsNullOrEmpty(_CharacterData.eyebrrowTexture) && !_CharacterData.eyebrrowTexture.Contains("default"))
                {
                    addressableDownloader.DownloadAddressableTexture(_CharacterData.eyebrrowTexture, this.gameObject, CurrentTextureType.EyeBrows);
                }
                else
                {
                    //characterBodyParts.SetTextureDefault(CurrentTextureType.EyeBrows, this.gameObject);
                    characterBodyParts.SetTextureDefault(CurrentTextureType.EyeBrows, this.gameObject);
                    characterBodyParts.ChangeEyebrowColor(characterBodyParts.DefaultEyebrowColor);
                }

                //if (_CharacterData.makeupName != "" && _CharacterData.makeupName != null)
                //{
                //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.makeupName, this.gameObject, CurrentTextureType.Makeup));
                //}

                //New texture are downloading for Boxer NFT 
                //if (!string.IsNullOrEmpty(_CharacterData.faceTattooTextureName) && _CharacterData.faceTattooTextureName != null)
                //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.faceTattooTextureName, this.gameObject, CurrentTextureType.FaceTattoo));
                //else
                //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.FaceTattoo);

                //if (!string.IsNullOrEmpty(_CharacterData.chestTattooTextureName) && _CharacterData.chestTattooTextureName != null)
                //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.chestTattooTextureName, this.gameObject, CurrentTextureType.ChestTattoo));
                //else
                //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.ChestTattoo);

                //if (!string.IsNullOrEmpty(_CharacterData.legsTattooTextureName) && _CharacterData.legsTattooTextureName != null)
                //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.legsTattooTextureName, this.gameObject, CurrentTextureType.LegsTattoo));
                //else
                //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.LegsTattoo);

                //if (!string.IsNullOrEmpty(_CharacterData.armTattooTextureName) && _CharacterData.armTattooTextureName != null)
                //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.armTattooTextureName, this.gameObject, CurrentTextureType.ArmTattoo));
                //else
                //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.ArmTattoo);

                //if (!string.IsNullOrEmpty(_CharacterData.mustacheTextureName) && _CharacterData.mustacheTextureName != null)
                //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.mustacheTextureName, this.gameObject, CurrentTextureType.Mustache));
                //else
                //    this.GetComponent<CharcterBodyParts>().RemoveMustacheTexture(null, this.gameObject);

                //LoadBonesData(_CharacterData, this.gameObject);

                // Seperate 
                //if (_CharacterData.Skin != null)
                //{
                //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.Skin, SliderType.Skin, this.gameObject));
                //    if (xanaConstants.isNFTEquiped)
                //    {
                //        BoxerNFTEventManager._lightPresetNFT = GetLightPresetValue(_CharacterData.Skin);
                //        BoxerNFTEventManager.NFTLightUpdate?.Invoke(BoxerNFTEventManager._lightPresetNFT);
                //    }
                //}
                //if (_CharacterData.EyeColor != null)
                //{
                //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.EyeColor, SliderType.EyesColor, this.gameObject));
                //}
                //if (_CharacterData.LipColor != null)
                //{
                //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.LipColor, SliderType.LipsColor, this.gameObject));
                //}

                //if (_CharacterData.EyebrowColor != null)
                //{
                //    Color tempColor = _CharacterData.EyebrowColor;
                //    tempColor.a = 1;
                //    _CharacterData.EyebrowColor = tempColor;
                //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.EyebrowColor, SliderType.EyeBrowColor, this.gameObject));
                //}

                //if (_CharacterData.SkinGerdientColor != null)
                //{
                //    characterBodyParts.ApplyGredientColor(_CharacterData.SkinGerdientColor, this.gameObject);
                //}
                //else
                //{
                //    characterBodyParts.ApplyGredientDefault(this.gameObject);
                //}

                //if (_CharacterData.SssIntensity != null)
                //{
                //    characterBodyParts.SetSssIntensity(_CharacterData.SssIntensity, this.gameObject);
                //}
                //else
                //{
                //    characterBodyParts.SetSssIntensity(characterBodyParts.defaultSssValue, this.gameObject);
                //}
                //SetItemIdsFromFile(_CharacterData);
                //characterBodyParts.LoadBlendShapes(_CharacterData, this.gameObject);
                #endregion
            }
            else // worlds scene 
            {
                if (this.GetComponent<PhotonView>() && this.GetComponent<PhotonView>().IsMine || staticPlayer) // self
                {
                    //   SetAvatarClothDefault(gameObject, gender);             //  Zeel Commented why you have to set default cloths every time

                    if (_CharacterData.myItemObj.Count > 0)
                    {
                        ClothsToBeLoaded.Clear();
                        for (int i = 0; i < _CharacterData.myItemObj.Count; i++)
                        {
                            var item = _CharacterData.myItemObj[i];
                            string type = item.ItemType;
                            if (!string.IsNullOrEmpty(item.ItemName) &&
                                !item.ItemName.Contains("default", System.StringComparison.CurrentCultureIgnoreCase))
                            {
                                HashSet<string> itemTypes = new HashSet<string> { "Legs", "Chest", "Feet", "Hair", "EyeWearable", "Glove", "Chain" };
                                if (itemTypes.Any(item => type.Contains(item)))
                                {
                                    if (!item.ItemName.Contains("md", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (type.Contains("Hair"))
                                        {
                                            if (!string.IsNullOrEmpty(_CharacterData.hairItemData) && _CharacterData.hairItemData.Contains("No hair") && wornHair)
                                                UnStichItem("Hair");
                                            else
                                            {
                                                ClothsToBeLoaded.Add(item);
                                                await addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, type, gender, avatarController, _CharacterData.HairColor);
                                            }
                                        }
                                        else
                                        {
                                            ClothsToBeLoaded.Add(item);
                                            await addressableDownloader.DownloadAddressableObj(_CharacterData.myItemObj[i].ItemID, _CharacterData.myItemObj[i].ItemName, type, _CharacterData.gender != null ? _CharacterData.gender : "Male", this.gameObject.GetComponent<AvatarController>(), Color.clear);

                                        }
                                    }
                                    else
                                    {
                                        if (ConstantsHolder.isNFTEquiped)
                                        {
                                            if (type.Contains("Chest"))
                                            {
                                                if (wornShirt)
                                                    UnStichItem("Chest");
                                            }
                                            else if (type.Contains("Hair"))
                                            {
                                                if (wornHair)
                                                    UnStichItem("Hair");
                                            }
                                            else if (type.Contains("Legs"))
                                            {
                                                if (wornPant)
                                                    UnStichItem("Legs");
                                            }
                                            else if (type.Contains("Feet"))
                                            {
                                                if (wornShoes)
                                                    UnStichItem("Feet");
                                            }
                                            else if (type.Contains("EyeWearable"))
                                            {
                                                if (wornEyeWearable)
                                                    UnStichItem("EyeWearable");
                                            }
                                            else if (type.Contains("Glove"))
                                            {
                                                if (wornGloves)
                                                    UnStichItem("Glove");
                                            }
                                            else if (type.Contains("Chain"))
                                            {
                                                if (wornChain)
                                                    UnStichItem("Chain");
                                            }
                                        }
                                        else
                                        {
                                            WearDefaultItem(type, this.gameObject, gender);
                                        }
                                    }
                                }
                                else
                                {
                                    WearDefaultItem(type, this.gameObject, gender);
                                }
                            }
                            else // wear the default item of that specific part.
                            {
                                if (ConstantsHolder.isNFTEquiped && type.Contains("Chest"))
                                {
                                    if (wornShirt)
                                        UnStichItem("Chest");
                                }
                                else
                                {
                                    WearDefaultItem(type, this.gameObject, gender);
                                }
                            }
                        }

                        WaitForCallback();
                    }
                    else
                    {
                        WearDefaultItem("Legs", this.gameObject, gender);
                        WearDefaultItem("Chest", this.gameObject, gender);
                        WearDefaultItem("Feet", this.gameObject, gender);
                        WearDefaultItem("Hair", this.gameObject, gender);
                    }
                    if (_CharacterData.charactertypeAi == true /*&& !UGCManager.isSelfieTaken*/)
                    {
                        ApplyAIData(_CharacterData, this.gameObject);
                    }
                    characterBodyParts.LoadBlendShapes(_CharacterData, this.gameObject);

                    if (!string.IsNullOrEmpty(_CharacterData.eyeTextureName))
                    {
                        addressableDownloader.DownloadAddressableTexture(_CharacterData.eyeTextureName, this.gameObject, CurrentTextureType.EyeLense);
                    }

                    #region Xana Avatar 1.0 //--> remove for xana avatar2.0

                    //if (_CharacterData.eyebrrowTexture != "" && _CharacterData.eyebrrowTexture != null)
                    //{
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.eyebrrowTexture, this.gameObject, CurrentTextureType.EyeBrows));
                    //}
                    //if (_CharacterData.eyeLashesName != "" && _CharacterData.eyeLashesName != null)
                    //{
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.eyeLashesName, this.gameObject, CurrentTextureType.EyeBrowPoints));
                    //}

                    //if (_CharacterData.makeupName != "" && _CharacterData.makeupName != null)
                    //{
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.makeupName, this.gameObject, CurrentTextureType.Makeup));
                    //}

                    ////New texture are downloading for Boxer NFT 
                    //if (!string.IsNullOrEmpty(_CharacterData.faceTattooTextureName) && _CharacterData.faceTattooTextureName != null)
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.faceTattooTextureName, this.gameObject, CurrentTextureType.FaceTattoo));
                    //else
                    //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.FaceTattoo);

                    //if (!string.IsNullOrEmpty(_CharacterData.chestTattooTextureName) && _CharacterData.chestTattooTextureName != null)
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.chestTattooTextureName, this.gameObject, CurrentTextureType.ChestTattoo));
                    //else
                    //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.ChestTattoo);

                    //if (!string.IsNullOrEmpty(_CharacterData.legsTattooTextureName) && _CharacterData.legsTattooTextureName != null)
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.legsTattooTextureName, this.gameObject, CurrentTextureType.LegsTattoo));
                    //else
                    //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.LegsTattoo);

                    //if (!string.IsNullOrEmpty(_CharacterData.armTattooTextureName) && _CharacterData.armTattooTextureName != null)
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.armTattooTextureName, this.gameObject, CurrentTextureType.ArmTattoo));
                    //else
                    //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.ArmTattoo);

                    //if (!string.IsNullOrEmpty(_CharacterData.mustacheTextureName) && _CharacterData.mustacheTextureName != null)
                    //    StartCoroutine(addressableDownloader.DownloadAddressableTexture(_CharacterData.mustacheTextureName, this.gameObject, CurrentTextureType.Mustache));
                    //else
                    //    this.GetComponent<CharcterBodyParts>().RemoveMustacheTexture(null, this.gameObject);

                    //// Seperate 
                    //if (_CharacterData.Skin != null)
                    //{
                    //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.Skin, SliderType.Skin, this.gameObject));
                    //}
                    //if (_CharacterData.EyeColor != null)
                    //{
                    //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.EyeColor, SliderType.EyesColor, this.gameObject));
                    //}
                    //if (_CharacterData.LipColor != null)
                    //{
                    //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.LipColor, SliderType.LipsColor, this.gameObject));
                    //}

                    //if (_CharacterData.EyebrowColor != null)
                    //{
                    //    StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.EyebrowColor, SliderType.EyeBrowColor, this.gameObject));
                    //}

                    //if (_CharacterData.SkinGerdientColor != null)
                    //{
                    //    characterBodyParts.ApplyGredientColor(_CharacterData.SkinGerdientColor, this.gameObject);
                    //}
                    //else
                    //{
                    //    characterBodyParts.ApplyGredientDefault(this.gameObject);
                    //}

                    //characterBodyParts.SetSssIntensity(0, this.gameObject);
                    //characterBodyParts.LoadBlendShapes(_CharacterData, this.gameObject);
                    //LoadBonesData(_CharacterData, this.gameObject);
                    #endregion
                }
            }
        }
        if (ConstantsHolder.isNFTEquiped)
            LoadingHandler.Instance.nftLoadingScreen.SetActive(false);
        if (characterBodyParts.head != null && characterBodyParts.body != null)
        {
            characterBodyParts.head.enabled = characterBodyParts.body.enabled = true;
        }
        isClothLoaded = true;
        
    }
    
    bool WearSameCloths(string itemName, string itemType)
    {
        GameObject wornItem = itemType switch
        {
            "Legs" => wornPant,
            "Chest" => wornShirt,
            "Feet" => wornShoes,
            "Hair" => wornHair,
            _ => null
        };

        return wornItem != null && wornItem.name == itemName;
    }
    async void WaitForCallback()
    {
        while (true)
        {
            int clothsloaded = 0;
            foreach (var item in ClothsToBeLoaded)
            {

                switch (item.ItemType)
                {

                    case "Chest":

                        if (wornShirtId == item.ItemID)
                            clothsloaded++;

                        break;

                    case "Legs":
                        if (wornPantId == item.ItemID)
                            clothsloaded++;


                        break;

                    case "Hair":

                        if (wornHairId == item.ItemID)
                            clothsloaded++;


                        break;

                    case "Feet":

                        if (wornShoesId == item.ItemID)
                            clothsloaded++;


                        break;

                    case "EyeWearable":

                        if (wornEyewearableId == item.ItemID)
                            clothsloaded++;

                        break;

                    case "Chain":

                        if (wornChainId == item.ItemID)
                            clothsloaded++;

                        break;

                    case "Glove":

                        if (wornGlovesId == item.ItemID)
                            clothsloaded++;

                        break;
                }
            }

            if (clothsloaded == ClothsToBeLoaded.Count)
            {
                GameplayEntityLoader.instance.ClothsLoaded = true;
                break;
            }
            await Task.Delay(1000);
        }

    }

    /// <summary>
    /// Setting Character from localJson neither than server
    /// </summary>
    public async Task<bool> BuildCharacterFromLocalJson()
    {
        SavingCharacterDataClass _CharacterData = new SavingCharacterDataClass();
        _CharacterData = new SavingCharacterDataClass();
        _CharacterData = _CharacterData.CreateFromJSON(staticClothJson);
        _PCharacterData = _CharacterData;
        clothJson = staticClothJson;
        var gender = _CharacterData.gender ?? "Male";
        if (_CharacterData.myItemObj.Count > 0)
        {
            ClothsToBeLoaded.Clear();
            for (int i = 0; i < _CharacterData.myItemObj.Count; i++)
            {
                var item = _CharacterData.myItemObj[i];
                string type = item.ItemType;
                if (!string.IsNullOrEmpty(item.ItemName) &&
                    !item.ItemName.Contains("default", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    HashSet<string> itemTypes = new HashSet<string> { "Legs", "Chest", "Feet", "Hair", "EyeWearable", "Glove", "Chain" };
                    if (itemTypes.Any(item => type.Contains(item)))
                    {
                        if (!item.ItemName.Contains("md", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (type.Contains("Hair"))
                            {
                                if (!string.IsNullOrEmpty(_CharacterData.hairItemData) && _CharacterData.hairItemData.Contains("No hair") && wornHair)
                                    UnStichItem("Hair");
                                else
                                {
                                    ClothsToBeLoaded.Add(item);
                                    if (this == null)
                                    {
                                        Debug.Log("applyOn object is null"+ this.gameObject.name);
                                    }
                                    else
                                    {
                                        await addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, type, gender, this, _CharacterData.HairColor);
                                    }
                                }
                            }
                            else
                            {
                                ClothsToBeLoaded.Add(item);
                                var applyOn = this.gameObject.GetComponent<AvatarController>();
                                if (applyOn == null)
                                {
                                    Debug.Log("applyOn object is null" + this.gameObject.name);
                                }
                                else
                                {
                                    await addressableDownloader.DownloadAddressableObj(_CharacterData.myItemObj[i].ItemID, _CharacterData.myItemObj[i].ItemName, type, _CharacterData.gender != null ? _CharacterData.gender : "Male", applyOn, Color.clear);
                                }
                            }
                        }
                        else
                        {
                            if (ConstantsHolder.isNFTEquiped)
                            {
                                if (type.Contains("Chest"))
                                {
                                    if (wornShirt)
                                        UnStichItem("Chest");
                                }
                                else if (type.Contains("Hair"))
                                {
                                    if (wornHair)
                                        UnStichItem("Hair");
                                }
                                else if (type.Contains("Legs"))
                                {
                                    if (wornPant)
                                        UnStichItem("Legs");
                                }
                                else if (type.Contains("Feet"))
                                {
                                    if (wornShoes)
                                        UnStichItem("Feet");
                                }
                                else if (type.Contains("EyeWearable"))
                                {
                                    if (wornEyeWearable)
                                        UnStichItem("EyeWearable");
                                }
                                else if (type.Contains("Glove"))
                                {
                                    if (wornGloves)
                                        UnStichItem("Glove");
                                }
                                else if (type.Contains("Chain"))
                                {
                                    if (wornChain)
                                        UnStichItem("Chain");
                                }
                            }
                            else
                            {
                                WearDefaultItem(type, this.gameObject, gender);
                            }
                        }
                    }
                    else
                    {
                        WearDefaultItem(type, this.gameObject, gender);
                    }
                }
                else // wear the default item of that specific part.
                {
                    if (ConstantsHolder.isNFTEquiped && type.Contains("Chest"))
                    {
                        if (wornShirt)
                            UnStichItem("Chest");
                    }
                    else
                    {
                        WearDefaultItem(type, this.gameObject, gender);
                    }
                }
            }

            WaitForCallback();
        }
        else
        {
            WearDefaultItem("Legs", this.gameObject, gender);
            WearDefaultItem("Chest", this.gameObject, gender);
            WearDefaultItem("Feet", this.gameObject, gender);
            WearDefaultItem("Hair", this.gameObject, gender);
        }
        characterBodyParts.LoadBlendShapes(_CharacterData, this.gameObject);
        isClothLoaded = true;
        if (!String.IsNullOrEmpty(_CharacterData.eyeTextureName))
        {
            addressableDownloader.DownloadAddressableTexture(_CharacterData.eyeTextureName, this.gameObject, CurrentTextureType.EyeLense);
        }
        return true;
    }

    /// <summary>
    /// For Boxer NFT there is no modification in data
    /// We only need to save file each time
    /// No Detection Available for NFT changed so Each time update & Save
    /// </summary>
    //void CreateOrUpdateBoxerFile(BoxerNFTDataClass _NFTData)
    //{
    //    CharacterBodyParts charcterBodyParts = GameManager.Instance.mainCharacter.GetComponent<CharacterBodyParts>();
    //    SavingCharacterDataClass _CharacterData1 = new SavingCharacterDataClass();

    //    // Create Class New Object 
    //    // Setting Default Values into this Object
    //    {
    //        _CharacterData1.eyeTextureName = "";
    //        _CharacterData1.Skin = characterBodyParts.DefaultSkinColor;
    //        _CharacterData1.LipColor = characterBodyParts.DefaultLipColor;
    //        _CharacterData1.EyebrowColor = characterBodyParts.DefaultEyebrowColor;
    //        _CharacterData1.HairColor = characterBodyParts.DefaultHairColor;

    //        _CharacterData1.makeupName = characterBodyParts.defaultMakeup.name;
    //        _CharacterData1.eyeLashesName = characterBodyParts.defaultEyelashes.name;
    //        _CharacterData1.eyebrrowTexture = characterBodyParts.defaultEyebrow.name;

    //        _CharacterData1.id = LoadPlayerAvatar.avatarId;
    //        _CharacterData1.name = LoadPlayerAvatar.avatarName;
    //        _CharacterData1.thumbnail = LoadPlayerAvatar.avatarThumbnailUrl;
    //        _CharacterData1.SkinId = this.skinId;
    //        _CharacterData1.PresetValue = this.presetValue;
    //        _CharacterData1.FaceValue = this.faceId;
    //        _CharacterData1.EyeBrowValue = this.eyeBrowId;
    //        _CharacterData1.EyeLashesValue = this.eyeLashesId;
    //        _CharacterData1.EyeValue = this.eyesId;
    //        _CharacterData1.EyesColorValue = this.eyesColorId;
    //        _CharacterData1.NoseValue = this.noseId;
    //        _CharacterData1.LipsValue = this.lipsId;
    //        _CharacterData1.LipsColorValue = this.lipsColorId;
    //        _CharacterData1.BodyFat = this.bodyFat;

    //        // These Are using for save implemted obj index
    //        _CharacterData1.MakeupValue = this.makeupId;
    //        _CharacterData1.faceMorphed = xanaConstants.isFaceMorphed;
    //        _CharacterData1.eyeBrowMorphed = xanaConstants.isEyebrowMorphed;
    //        _CharacterData1.eyeMorphed = xanaConstants.isEyeMorphed;
    //        _CharacterData1.noseMorphed = xanaConstants.isNoseMorphed;
    //        _CharacterData1.lipMorphed = xanaConstants.isLipMorphed;
    //        _CharacterData1.punch = _NFTData.punch;
    //        _CharacterData1.special_move = _NFTData.special_move;
    //        _CharacterData1.kick = _NFTData.kick;
    //        _CharacterData1.profile = _NFTData.profile;
    //        _CharacterData1.speed = _NFTData.speed;
    //        _CharacterData1.stamina = _NFTData.stamina;
    //        _CharacterData1.defence = _NFTData.defence;


    //        _CharacterData1.SavedBones = new List<BoneDataContainer>();
    //        for (int i = 0; i < charcterBodyParts.BonesData.Count; i++)
    //        {
    //            Transform bone = charcterBodyParts.BonesData[i].Obj.transform;
    //            _CharacterData1.SavedBones.Add(new BoneDataContainer(charcterBodyParts.BonesData[i].Name, bone.localPosition, bone.localEulerAngles, bone.localScale));
    //        }

    //        int totaBlendShapes = GameManager.Instance.m_ChHead.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount;
    //        _CharacterData1.FaceBlendsShapes = new float[totaBlendShapes];

    //        _CharacterData1.faceTattooTextureName = string.Empty;
    //        _CharacterData1.chestTattooTextureName = string.Empty;
    //        _CharacterData1.legsTattooTextureName = string.Empty;
    //        _CharacterData1.armTattooTextureName = string.Empty;
    //        _CharacterData1.mustacheTextureName = string.Empty;
    //        _CharacterData1.eyeLidTextureName = string.Empty;
    //    }

    //    int listCurrentIndex = 0;
    //    _CharacterData1.myItemObj = new List<Item>();

    //    // Cloths , Gloves , Chains , glasses
    //    {
    //        if (!string.IsNullOrEmpty(_NFTData.Pants))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Pants, "Pants_");

    //            _CharacterData1.myItemObj.Add(new Item(-1, tempKey, "Legs"));
    //            listCurrentIndex++;
    //        }
    //        else
    //        {
    //            _CharacterData1.myItemObj.Add(new Item(-1, "md", "Legs"));
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Full_Costumes))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Full_Costumes, "Full_Costume_");

    //            _CharacterData1.myItemObj.Add(new Item(-1, tempKey, "Chest"));
    //            listCurrentIndex++;
    //        }
    //        else
    //        {
    //            _CharacterData1.myItemObj.Add(new Item(-1, "md", "Chest"));
    //            if (wornShirt)
    //            {
    //                UnStichItem("Chest");
    //                characterBodyParts.TextureForShirt(null);
    //            }
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Hairs))
    //        {
    //            string tempKey = _NFTData.Hairs.Split('-')[0];
    //            tempKey = GetUpdatedKey(tempKey, "Hairs_");
    //            // need to split // its also has color code

    //            _CharacterData1.myItemObj.Add(new Item(-1, tempKey, "Hair"));
    //            listCurrentIndex++;
    //        }
    //        else
    //        {
    //            _CharacterData1.myItemObj.Add(new Item(-1, "md", "Hair"));
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Shoes))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Shoes, "Shoes_");

    //            _CharacterData1.myItemObj.Add(new Item(-1, tempKey, "Feet"));
    //            listCurrentIndex++;
    //        }
    //        else
    //        {
    //            _CharacterData1.myItemObj.Add(new Item(-1, "md", "Feet"));
    //            if (wornShoes)
    //            {
    //                UnStichItem("Feet");
    //                characterBodyParts.TextureForShoes(null);
    //            }
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Glasses))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Glasses, "Glasses_");

    //            _CharacterData1.myItemObj.Add(new Item(-1, tempKey, "EyeWearable"));
    //            listCurrentIndex++;
    //        }
    //        else
    //        {
    //            _CharacterData1.myItemObj.Add(new Item(-1, "md", "EyeWearable"));
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Gloves))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Gloves, "Gloves_");

    //            _CharacterData1.myItemObj.Add(new Item(-1, tempKey, "Glove"));
    //            listCurrentIndex++;
    //        }
    //        else
    //        {
    //            _CharacterData1.myItemObj.Add(new Item(-1, "md", "Glove"));
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Chains))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Chains, "Chains_");

    //            _CharacterData1.myItemObj.Add(new Item(-1, tempKey, "Chain"));
    //            listCurrentIndex++;
    //        }
    //        else
    //        {
    //            _CharacterData1.myItemObj.Add(new Item(-1, "md", "Chain"));
    //        }
    //    }
    //    // Tattoo , Mustache , EyeLid
    //    {
    //        if (!string.IsNullOrEmpty(_NFTData.Face_Tattoo))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Face_Tattoo, "Face_Tattoo_");
    //            characterBodyParts.faceTattooColor = characterBodyParts.faceTattooColorDefault;
    //            _CharacterData1.faceTattooTextureName = tempKey;
    //        }
    //        else if (!string.IsNullOrEmpty(_NFTData.Forehead_Tattoo))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Forehead_Tattoo, "Forehead_Tattoo_");
    //            characterBodyParts.faceTattooColor = characterBodyParts.foreheafTattooColor;
    //            _CharacterData1.faceTattooTextureName = tempKey;
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Chest_Tattoo))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Chest_Tattoo, "Chest_Tattoo_");
    //            _CharacterData1.chestTattooTextureName = tempKey;
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Legs_Tattoo))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Legs_Tattoo, "Legs_Tattoo_");
    //            _CharacterData1.legsTattooTextureName = tempKey;
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Arm_Tattoo))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Arm_Tattoo, "Arm_Tattoo_");
    //            _CharacterData1.armTattooTextureName = tempKey;
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Mustache))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Mustache, "Mustache_");
    //            _CharacterData1.mustacheTextureName = tempKey;
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Eyelid))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Eyelid, "Eyelid_");
    //            _CharacterData1.eyeLidTextureName = tempKey;
    //        }
    //    }

    //    // EyeShape, Lense, Skin, lips , Hairs , Eyebrows
    //    {
    //        if (!string.IsNullOrEmpty(_NFTData.Eye_Shapes))
    //        {
    //            string tempKey = _NFTData.Eye_Shapes.Replace(" - ", "_");
    //            tempKey = tempKey.Split('_')[0].Replace(" ", "");
    //            BoxerEyeData.instance.SetNFTData(tempKey);

    //            // Add Values in current Object
    //            for (int i = 0; i < GameManager.Instance.m_ChHead.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; i++)
    //            {
    //                _CharacterData1.FaceBlendsShapes[i] = GameManager.Instance.m_ChHead.GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(i);
    //            }

    //            _CharacterData1.SavedBones.Clear();
    //            for (int i = 0; i < charcterBodyParts.BonesData.Count; i++)
    //            {
    //                Transform bone = charcterBodyParts.BonesData[i].Obj.transform;
    //                _CharacterData1.SavedBones.Add(new BoneDataContainer(charcterBodyParts.BonesData[i].Name, bone.localPosition, bone.localEulerAngles, bone.localScale));
    //            }
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Eye_Lense))
    //        {
    //            string tempKey = GetUpdatedKey(_NFTData.Eye_Lense, "Eye_Lense_");
    //            _CharacterData1.eyeTextureName = tempKey;
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Eyebrows))
    //        {
    //            string temp = _NFTData.Eyebrows.Replace(" - ", "_").Split('_')[0];

    //            string tempKey = GetUpdatedKey(temp, "Eyebrows_");
    //            Debug.Log(tempKey);
    //            _CharacterData1.eyebrrowTexture = tempKey;
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Skin))
    //        {
    //            string tempKey = GetColorCodeFromNFTKey(_NFTData.Skin);
    //            _CharacterData1.Skin = GetColorCode(tempKey);
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Lips))
    //        {
    //            string tempKey = GetColorCodeFromNFTKey(_NFTData.Lips);
    //            _CharacterData1.LipColor = GetColorCode(tempKey);
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Eyebrows))
    //        {
    //            string tempKey = GetColorCodeFromNFTKey(_NFTData.Eyebrows);
    //            _CharacterData1.EyebrowColor = GetColorCode(tempKey);
    //        }

    //        if (!string.IsNullOrEmpty(_NFTData.Hairs))
    //        {
    //            string tempKey = GetColorCodeFromNFTKey(_NFTData.Hairs);
    //            Debug.Log(tempKey);
    //            _CharacterData1.HairColor = GetColorCode(tempKey);
    //        }
    //    }

    //    string updatedBoxerData = JsonUtility.ToJson(_CharacterData1);
    //    var filePath = Path.Combine(Application.persistentDataPath, xanaConstants.NFTBoxerJson);
    //    File.WriteAllText(filePath, updatedBoxerData);
    //}

    /// <summary>
    /// To Wear DefaultItem according to the gender
    /// </summary>
    /// <param name="type"> Type of object like hair, shirt,pent etc </param>
    /// <param name="applyOn"></param>
    /// <param name="gender"></param>
    public void WearDefaultItem(string type, GameObject applyOn, string gender)
    {
        CharacterBodyParts bodyParts = characterBodyParts;

        //Debug.Log("Item: " + type + " --- Gender: " + gender);

        if (gender.IsNullOrEmpty())
        {
            if (applyOn.name.Contains("VTuber_", System.StringComparison.CurrentCultureIgnoreCase))
            {
                if (applyOn.name.Contains("Female", System.StringComparison.CurrentCultureIgnoreCase))
                    gender = "VTuber_Female";
                else
                    gender = "VTuber_Male";
            }
            else if (applyOn.name.Contains("Female", System.StringComparison.CurrentCultureIgnoreCase))
                gender = "Female";
            else
                gender = "Male";
        }


        if (itemDatabase == null)
        {
            itemDatabase = DefaultClothDatabase.instance;
        }
        if (gender == "Male") // if avatar is Male
        {
            switch (type)
            {
                case "Legs":
                    if (itemDatabase.maleAvatarDefaultCostume.DefaultPent != null)
                        StichItem(-1, itemDatabase.maleAvatarDefaultCostume.DefaultPent, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Chest":
                    if (itemDatabase.maleAvatarDefaultCostume.DefaultShirt != null)
                        StichItem(-1, itemDatabase.maleAvatarDefaultCostume.DefaultShirt, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Feet":
                    if (itemDatabase.maleAvatarDefaultCostume.DefaultShoes != null)
                        StichItem(-1, itemDatabase.maleAvatarDefaultCostume.DefaultShoes, type, applyOn);
                    break;

                case "Hair":
                    if (itemDatabase.maleAvatarDefaultCostume.DefaultHair != null)
                        StichItem(-1, itemDatabase.maleAvatarDefaultCostume.DefaultHair, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                default:
                    break;
            }
        }
        else if (gender == "Female") // if avatar is female
        {
            switch (type)
            {
                case "Legs":
                    if (itemDatabase.femaleAvatarDefaultCostume.DefaultPent != null)
                        StichItem(-1, itemDatabase.femaleAvatarDefaultCostume.DefaultPent, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Chest":
                    if (itemDatabase.femaleAvatarDefaultCostume.DefaultShirt != null)
                        StichItem(-1, itemDatabase.femaleAvatarDefaultCostume.DefaultShirt, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Feet":
                    if (itemDatabase.femaleAvatarDefaultCostume.DefaultShoes != null)
                        StichItem(-1, itemDatabase.femaleAvatarDefaultCostume.DefaultShoes, type, applyOn);
                    break;

                case "Hair":
                    if (itemDatabase.femaleAvatarDefaultCostume.DefaultHair != null)
                        StichItem(-1, itemDatabase.femaleAvatarDefaultCostume.DefaultHair, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                default:
                    break;
            }
        }
        else if (gender == "VTuber_Female") // if avatar is female
        {
            switch (type)
            {
                case "Legs":
                    if (itemDatabase.vtFemaleAvatarDefaultCostume.DefaultPent != null)
                        StichItem(-1, itemDatabase.vtFemaleAvatarDefaultCostume.DefaultPent, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Chest":
                    if (itemDatabase.vtFemaleAvatarDefaultCostume.DefaultShirt != null)
                        StichItem(-1, itemDatabase.vtFemaleAvatarDefaultCostume.DefaultShirt, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Feet":
                    if (itemDatabase.vtFemaleAvatarDefaultCostume.DefaultShoes != null)
                        StichItem(-1, itemDatabase.vtFemaleAvatarDefaultCostume.DefaultShoes, type, applyOn);
                    break;

                case "Hair":
                    if (itemDatabase.vtFemaleAvatarDefaultCostume.DefaultHair != null)
                        StichItem(-1, itemDatabase.vtFemaleAvatarDefaultCostume.DefaultHair, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                default:
                    break;
            }
        }
        else if (gender == "VTuber_Male") // if avatar is female
        {
            switch (type)
            {
                case "Legs":
                    if (itemDatabase.vtMaleAvatarDefaultCostume.DefaultPent != null)
                        StichItem(-1, itemDatabase.vtMaleAvatarDefaultCostume.DefaultPent, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Chest":
                    if (itemDatabase.vtMaleAvatarDefaultCostume.DefaultShirt != null)
                        StichItem(-1, itemDatabase.vtMaleAvatarDefaultCostume.DefaultShirt, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                case "Feet":
                    if (itemDatabase.vtMaleAvatarDefaultCostume.DefaultShoes != null)
                        StichItem(-1, itemDatabase.vtMaleAvatarDefaultCostume.DefaultShoes, type, applyOn);
                    break;

                case "Hair":
                    if (itemDatabase.vtMaleAvatarDefaultCostume.DefaultHair != null)
                        StichItem(-1, itemDatabase.vtMaleAvatarDefaultCostume.DefaultHair, type, applyOn);
                    else
                        UnStichItem(type);
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// To Wear default hairs for multiplier
    /// </summary>
    /// <param name="applyOn"></param>
    /// <param name="hairColor"></param>
    public void WearDefaultHair(GameObject applyOn, Color hairColor)
    {
        //StichItem(-1, itemDatabase.DefaultHair, "Hair", applyOn, hairColor);
        StichHairWithColor(-1, itemDatabase.DefaultHair, "Hair", applyOn, hairColor, true);
    }

    /// <summary>
    /// 1 - Update body according to fat
    /// 2 -Fit cloth according to the selected body type
    /// </summary>
    public void ResizeClothToBodyFat(GameObject applyOn, int bodyFat)
    {
        //Equipment equipment = applyOn.GetComponent<Equipment>();
        CharacterBodyParts bodyparts = applyOn.GetComponent<CharacterBodyParts>();

        float _size3 = 1f + ((float)bodyFat / 100f);

        Debug.Log("Resizing Body Parts & Cloths : " + bodyFat + "  :  " + _size3);

        if (bodyparts._scaleBodyParts.Count > 0)
        {
            for (int i = 0; i < bodyparts._scaleBodyParts.Count; i++)
            {
                if (bodyparts._scaleBodyParts[i])
                    bodyparts._scaleBodyParts[i].transform.localScale = new Vector3(_size3, 1, _size3);
            }
        }
    }

    /// <summary>
    /// To Load data from file
    /// </summary>
    /// <param name="_CharacterData"> player data save in file</param>
    /// <param name="applyOn">Object on which data is going to apply</param>
    public void LoadBonesData(SavingCharacterDataClass _CharacterData, GameObject applyOn)
    {
        CharacterBodyParts parts = applyOn.GetComponent<CharacterBodyParts>();
        if (applyOn != null)
        {
            if (_CharacterData != null)
            {
                List<BoneDataContainer> boneData = _CharacterData.SavedBones;
                if (boneData.Count > 0)
                {
                    for (int i = 0; i < boneData.Count; i++)
                    {
                        if (i < parts.BonesData.Count && boneData[i] != null)
                        {
                            var bone = parts.BonesData[i].Obj.transform;
                            bone.localPosition = boneData[i].Pos;
                            bone.localScale = boneData[i].Scale;
                        }
                    }
                }
                else
                {
                    ResetBonesDefault(parts);
                }
            }
            else
            {
                if (parts.BonesData.Count > 0)
                {
                    ResetBonesDefault(parts);
                }
            }
        }
    }

    /// <summary>
    /// To reset bones to default pos and scale
    /// </summary>
    /// <param name="parts"> CharcterBodyParts </param>
    public void ResetBonesDefault(CharacterBodyParts parts)
    {
        if (parts != null && parts.BonesData.Count > 0)
        {
            for (int i = 0; i < parts.BonesData.Count; i++)
            {
                var bone = parts.BonesData[i].Obj.transform;
                if (bone != null)
                {
                    bone.localPosition = parts.BonesData[i].Pos;
                    bone.localScale = parts.BonesData[i].Scale;
                    bone.localEulerAngles = parts.BonesData[i].Rotation;
                }
            }
        }
    }

    /// <summary>
    /// To stich item on player rig
    /// </summary>
    /// <param name="item">Cloth to wear</param>
    /// <param name="applyOn">Player that are going to wear the dress</param>
    public void StichItem(int itemId, GameObject item, string type, GameObject applyOn, bool applyHairColor = true)
    {
        CharacterBodyParts tempBodyParts = applyOn.gameObject.GetComponent<CharacterBodyParts>();
        //FriendAvatarController friendAvatarController = applyOn.GetComponent<FriendAvatarController>();
        EffectedParts effectedParts = item.GetComponent<EffectedParts>();


        
        if (applyOn.name.Contains("Vtuber") || applyOn.name.Contains("VT_"))
        {
            MagicaCloth magicaCloth = item.GetComponentInChildren<MagicaCloth>();
            if (magicaCloth != null && magicaCloth.enabled)
                item = stitcher.Stitch_Vtuber_Megica(item, applyOn);
            else
                item = this.stitcher.Stitch(item, applyOn);
        }
        else
            item = stitcher.Stitch(item, applyOn);
        //Debug.LogError($"Hair stitch: {type} {applyHairColor} {ConstantsHolder.xanaConstants.isStoreActive}");
        if (type == "Hair" && !item.name.StartsWith("VT_"))
        {
            if (applyHairColor /*&& _CharData.HairColor != null && getHairColorFormFile */)
            {
                SavingCharacterDataClass _CharacterData = new SavingCharacterDataClass();
                if (isLoadStaticClothFromJson)
                {
                    _CharacterData = _CharacterData.CreateFromJSON(staticClothJson);
                    if (!ConstantsHolder.xanaConstants.isStoreActive) // Changing Hair no need to apply color from file
                        tempBodyParts.ImplementColors(_CharacterData.HairColor, SliderType.HairColor, applyOn);
                }
                else
                {
                    var filePath = GameManager.Instance.GetStringFolderPath();
                    if (File.Exists(filePath))
                    {
                        var fileContent = File.ReadAllText(filePath);
                        if (!string.IsNullOrEmpty(fileContent))
                        {
                            _CharacterData = _CharacterData.CreateFromJSON(fileContent);
                            if (!ConstantsHolder.xanaConstants.isStoreActive && gameObject.activeInHierarchy) // Changing Hair no need to apply color from file
                                tempBodyParts.ImplementColors(_CharacterData.HairColor, SliderType.HairColor, applyOn);
                        }
                    }
                    else
                    {
                        tempBodyParts.ImplementColors(Color.black, SliderType.HairColor, applyOn);
                        // Hairs Default Color
                        //StartCoroutine(tempBodyParts.ImplementColors(new Color(0.9058824f, 0.5137255f, 0.4039216f,1f), SliderType.HairColor, applyOn));
                    }
                }
                if (_CharacterData?.charactertypeAi == true)
                {
                    tempBodyParts.ImplementColors(_CharacterData.hair_color, SliderType.HairColor, applyOn);
                }
                //else
                //{
                //    //StartCoroutine(tempBodyParts.ImplementColors(Color.black, SliderType.HairColor, applyOn));
                //    StartCoroutine(tempBodyParts.ImplementColors(new Color(0.9058824f, 0.5137255f, 0.4039216f, 1f), SliderType.HairColor, applyOn));
                //}
            }
            else if (type == "Hair" && xanaConstants.isPresetHairColor && presetHairColor != null)
            {
                //getHairColorFormFile = false;
                tempBodyParts.ImplementColors(presetHairColor, SliderType.HairColor, applyOn);
                presetHairColor = Color.clear;
            }
        }

        if (SceneManager.GetActiveScene().name != "Home")
        {
            if (item.name.StartsWith("VT_"))
                item.layer = 3; // Change the Shoes Layer for light effect
            else
                item.layer = 22;
        }
        else
        {
            if (xanaConstants == null)
            {
                xanaConstants = ConstantsHolder.xanaConstants;
            }

            if (ConstantsHolder.isNFTEquiped)
            {
                if (PlayerPrefs.GetInt("IsNFTCollectionBreakingDown") == 1)
                {

                    if (type == "Hair")
                        item.layer = 25;
                    else
                        item.layer = 26;

                    SwitchToShoesHirokoKoshino.Instance.DisableAllLighting();
                }
                if (PlayerPrefs.GetInt("IsNFTCollectionBreakingDown") == 2)
                {
                    // HIROKO KOSHINO NFT 
                    SwitchToShoesHirokoKoshino.Instance.SwitchLightFor_HirokoKoshino(PlayerPrefs.GetString("HirokoLight"));
                    item.layer = 11;
                }
            }
            else
            {
                item.layer = 11;
            }
        }
        
        UnStichItem(type);

        if (effectedParts && effectedParts.texture != null)
        {
            Texture tempTex = effectedParts.texture;
            masks.Add(tempTex);
            tempBodyParts.ApplyMaskTexture(type, tempTex, this.gameObject);
        }
        if (effectedParts && effectedParts.variation_Texture != null)
        {
            string baseMap = "_BaseMap";

            if (item.name.StartsWith("VT_"))
            {
                baseMap = "_MainTex"; //_MainTex -- BaseMap
            }

            item.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.SetTexture(baseMap, effectedParts.variation_Texture);
        }

        switch (type)
        {
            case "Chest":
                wornShirt = item;
                wornShirtId = itemId;
                wornShirt.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                break;

            case "Legs":
                wornPant = item;
                wornPantId = itemId;
                wornPant.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                break;

            case "Hair":
                wornHair = item;
                wornHairId = itemId;
                wornHair.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                break;

            case "Feet":
                wornShoes = item;
                wornShoesId = itemId;
                wornShoes.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                break;

            case "EyeWearable":
                wornEyeWearable = item;
                wornEyewearableId = itemId;
                break;

            case "Chain":
                wornChain = item;
                wornChainId = itemId;
                break;

            case "Glove":
                wornGloves = item;
                Material m = new Material(wornGloves.GetComponent<SkinnedMeshRenderer>().materials[0]);
                wornGloves.GetComponent<SkinnedMeshRenderer>().materials[0] = m;
                wornGlovesId = itemId;
                break;
        }


        if (CheckShirtCostume(item.name))
        {
            // Disable Pant
            if (wornPant)
            {
                UnStichItem("Legs");
                // wornPant.GetComponent<SkinnedMeshRenderer>().enabled = false; 
            }

            // Also Remove Pant Mask
            tempBodyParts.ApplyMaskTexture("Legs", null, this.gameObject);
        }
        else if (type == "Legs" && (wornShirt && CheckShirtCostume(item.name)))
        {
            if (SceneManager.GetActiveScene().name != "Home")
                // User Has wear Full Costume 
                // Change Full costume to Default shirt 

                WearDefaultItem("Chest", applyOn.gameObject, CharacterHandler.instance.activePlayerGender.ToString());

            // Apply Mask For Default Shirt
            //tempBodyParts.DefaultTextureForNewCharacter_Single("Shirt");


            if (CharacterHandler.instance.activePlayerGender == AvatarGender.VTuber_Female)
                StichItem(-1, itemDatabase.vtFemaleAvatarDefaultCostume.DefaultShirt, "Chest", applyOn);

            else if (CharacterHandler.instance.activePlayerGender == AvatarGender.VTuber_Male)
                StichItem(-1, itemDatabase.vtMaleAvatarDefaultCostume.DefaultShirt, "Chest", applyOn);

            else if (CharacterHandler.instance.activePlayerGender == AvatarGender.Male)
                StichItem(-1, itemDatabase.maleAvatarDefaultCostume.DefaultShirt, "Chest", applyOn);

            else
                StichItem(-1, itemDatabase.femaleAvatarDefaultCostume.DefaultShirt, "Chest", applyOn);

            #region Xana1.0 Boxer currently Not using In Xana2.0
            //if (xanaConstants.isNFTEquiped && gameObject.GetComponent<SwitchToBoxerAvatar>())
            //{
            //    if (wornPant)
            //        wornPant.SetActive(false);

            //    // Also Remove Pant Mask
            //    tempBodyParts.ApplyMaskTexture("Legs", null, this.gameObject);

            //    if (wornChain)
            //        wornChain.SetActive(false);

            //    gameObject.GetComponent<SwitchToBoxerAvatar>().OnFullCostumeWear();
            //}
            #endregion
        }
        else if (type == "Chest" && (wornShirt && !CheckShirtCostume(item.name)))
        {
            if (wornPant == null)
            {

                if (CharacterHandler.instance.activePlayerGender == AvatarGender.VTuber_Male)
                    StichItem(-1, itemDatabase.vtMaleAvatarDefaultCostume.DefaultPent, "Legs", applyOn);

                else if (CharacterHandler.instance.activePlayerGender == AvatarGender.VTuber_Female)
                    StichItem(-1, itemDatabase.vtFemaleAvatarDefaultCostume.DefaultPent, "Legs", applyOn);

                else if (CharacterHandler.instance.activePlayerGender == AvatarGender.Male)
                    StichItem(-1, itemDatabase.maleAvatarDefaultCostume.DefaultPent, "Legs", applyOn);
                else
                    StichItem(-1, itemDatabase.femaleAvatarDefaultCostume.DefaultPent, "Legs", applyOn);
            }

            // User Has wear Full Costume And its Change to Shirt Than enable Pant
            if (wornPant != null)
            {
                wornPant.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }
        //else
        //{
        //    if (wornPant)
        //        wornPant.SetActive(true);
        //    if (wornHair)
        //        wornHair.SetActive(true);
        //    if (wornChain)
        //        wornChain.SetActive(true);
        //}
        if (PlayerPrefs.GetInt("presetPanel") != 1)
        {
            if (InventoryManager.instance != null && InventoryManager.instance.loaderForItems)
            {
                //Debug.Log("Asset Loader False");
                InventoryManager.instance.loaderForItems.SetActive(false);
            }
        }
    }


    bool CheckShirtCostume(string itemName)
    {
        if (itemName.Contains("Full_Costume", System.StringComparison.CurrentCultureIgnoreCase) ||
          (itemName.Contains("VT_Girl_Costume_", System.StringComparison.CurrentCultureIgnoreCase)) ||
          (itemName.Contains("VT_Boy_Costume_", System.StringComparison.CurrentCultureIgnoreCase)))
        {
            return true;
        }
        else return false;
    }


    /// <summary>
    /// As Home Scene has now friends so we need to change color of hairs as require
    /// </summary>
    public void StichHairWithColor(int itemId, GameObject item, string type, GameObject applyOn, Color hairColor, bool isMultiPlayer)
    {
        CharacterBodyParts tempBodyParts = applyOn.gameObject.GetComponent<CharacterBodyParts>();
        EffectedParts effectedParts = item.GetComponent<EffectedParts>();
        UnStichItem(type);

        if (applyOn.name.Contains("Vtuber") || applyOn.name.Contains("VT_"))
        {
            MagicaCloth magicaCloth = item.GetComponentInChildren<MagicaCloth>();
            if (magicaCloth != null && magicaCloth.enabled)
                item = this.stitcher.Stitch_Vtuber_Megica(item, applyOn);
            else
                item = this.stitcher.Stitch(item, applyOn);

            if (effectedParts && effectedParts.variation_Texture != null)
                item.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.SetTexture("_MainTex", effectedParts.variation_Texture);
        }
        else
        {
            item = this.stitcher.Stitch(item, applyOn);
            applyOn.GetComponent<AvatarController>().wornHair = item;
            tempBodyParts.ImplementColors(hairColor, SliderType.HairColor, applyOn);
        }

        if (isMultiPlayer)
        {
            item.layer = 22;
        }
        else
        {
            item.layer = 11;
        }
        wornHair = item;
        wornHairId = itemId;
        wornHair.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;

        if (PlayerPrefs.GetInt("presetPanel") != 1)
        {
            if (InventoryManager.instance != null && InventoryManager.instance.loaderForItems)
            {
                Debug.Log("Asset Loader False");
                InventoryManager.instance.loaderForItems.SetActive(false);
            }
        }
    }


    /// <summary>
    /// For Multiplayer Hairs Only
    /// </summary>
    /// <param name="itemId"> asset id </param>
    /// <param name="item"> item to wear</param>
    /// <param name="type">item type</param>
    /// <param name="applyOn">object on which item is to apply</param>
    /// <param name="hairColor"> hair color code</param>
    public void StichItem(int itemId, GameObject item, string type, GameObject applyOn, Color hairColor)
    {
        CharacterBodyParts tempBodyParts = applyOn.gameObject.GetComponent<CharacterBodyParts>();
        EffectedParts effectedParts = item.GetComponent<EffectedParts>();
        UnStichItem(type);
        if (effectedParts && effectedParts.texture != null)
        {
            Texture tempTex = effectedParts.texture;
            masks.Add(tempTex);
            tempBodyParts.ApplyMaskTexture(type, tempTex, this.gameObject);
        }

        if (effectedParts && effectedParts.variation_Texture != null)
        {
            item.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.SetTexture("_BaseMap", effectedParts.variation_Texture);
        }

        if (applyOn.name.Contains("Vtuber") || applyOn.name.Contains("VT_"))
        {
            MagicaCloth magicaCloth = item.GetComponentInChildren<MagicaCloth>();
            if (magicaCloth != null && magicaCloth.enabled)
                item = this.stitcher.Stitch_Vtuber_Megica(item, applyOn);
            else
                item = this.stitcher.Stitch(item, applyOn);
        }
        else
            item = this.stitcher.Stitch(item, applyOn);

        if (type == "Hair")
        {
            tempBodyParts.ImplementColors(hairColor, SliderType.HairColor, applyOn);
        }

        item.layer = 22;
        wornHair = item;
        wornHairId = itemId;
        wornHair.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
    }

    /// <summary>
    /// To Ubstich item from the object
    /// </summary>
    /// <param name="type"></param>
    public void UnStichItem(string type)
    {
        switch (type)
        {
            case "Chest":
                characterBodyParts.TextureForShirt(null);
                Destroy(wornShirt);
                break;

            case "Legs":
                characterBodyParts.TextureForPant(null);
                Destroy(wornPant);
                break;

            case "Hair":
                Destroy(wornHair);
                break;

            case "Feet":
                characterBodyParts.TextureForShoes(null);
                Destroy(wornShoes);
                break;

            case "EyeWearable":
                Destroy(wornEyeWearable);
                break;

            case "Chain":
                Destroy(wornChain);
                break;

            case "Glove":
                characterBodyParts.TextureForGlove(null);
                Destroy(wornGloves);
                break;
        }
    }

    /// <summary>
    /// Applying assets mask
    /// </summary>
    /// <param name="applyOn">Multiplayer Avatar on which asset is too be bind</param>
    /// <returns></returns>
    public IEnumerator RPCMaskApply(GameObject applyOn)
    {
        yield return new WaitForSeconds(1);
        if (masks.Count > 0)
        {
            foreach (var mask in masks)
            {
                applyOn.GetComponent<CharacterBodyParts>().ApplyMaskTexture(mask.name, mask, this.gameObject);
            }
        }
    }

    /// <summary>
    /// Update Keys for Boxer NFT's
    /// </summary>
    /// <param name="Key"> item key </param>
    /// <param name="prefixAdded"> prefec to be added</param>
    /// <returns></returns>
    string GetUpdatedKey(string Key, string prefixAdded)
    {
        string tempKey;
        if (Key.Contains(" - "))
            tempKey = Key.Replace(" - ", "_");
        else
            tempKey = Key;
        tempKey = tempKey.Replace(" ", "");
        tempKey = prefixAdded + tempKey;
        return tempKey;
    }

    /// <summary>
    /// Get Color Code for NFT
    /// </summary>
    /// <param name="key"> item key </param>
    /// <returns></returns>
    string GetColorCodeFromNFTKey(string key)
    {
        string tempKey;
        if (key.Contains(" - "))
        {
            tempKey = key.Replace(" - ", "_");
            tempKey = tempKey.Split('_').Last();
        }
        else
            tempKey = key;

        tempKey = tempKey.Replace(" ", "");
        return tempKey;
    }

    /// <summary>
    /// Get Color Code for NFT
    /// </summary>
    /// <param name="colorCode"></param>
    /// <returns></returns>
    public Color GetColorCode(string colorCode)
    {
        for (int i = 0; i < _nftAvatarColorCodes.colorCodes.Count; i++)
        {
            if (colorCode.ToLower() == _nftAvatarColorCodes.colorCodes[i].colorName.ToLower())
            {
                return _nftAvatarColorCodes.colorCodes[i].updatedColor;
            }
        }
        return Color.black;
    }

    /// <summary>
    /// To get NFT light preset
    /// </summary>
    /// <param name="colorCode"></param>
    /// <returns></returns>
    public LightPresetNFT GetLightPresetValue(Color colorCode)
    {
        for (int i = 0; i < _nftAvatarColorCodes.colorCodes.Count; i++)
        {
            if (colorCode == _nftAvatarColorCodes.colorCodes[i].updatedColor)
            {
                return _nftAvatarColorCodes.colorCodes[i].LightPresetNFT;
            }
        }
        return LightPresetNFT.DefaultSkin;
    }

    /// <summary>
    /// Set avatar according to the AI 
    /// </summary>
    /// <param name="_CharacterData"> AI Character data</param>
    async void ApplyAIData(SavingCharacterDataClass _CharacterData, GameObject applyOn)
    {
        string pathPath = "Assets/Store Items Addressables/";
        characterBodyParts.head.SetBlendShapeWeight(_CharacterData.faceItemData, 100);
        characterBodyParts.head.SetBlendShapeWeight(_CharacterData.lipItemData, 100);
        characterBodyParts.head.SetBlendShapeWeight(_CharacterData.noseItemData, 100);
        characterBodyParts.head.SetBlendShapeWeight(_CharacterData.eyeShapeItemData, 100);
        string gender = _CharacterData.gender != null ? _CharacterData.gender : "Male";
        if (_CharacterData.lip_color != null)
        {
            characterBodyParts.head.materials[2].SetColor("_Lips_Color", _CharacterData.lip_color);
        }
        if (_CharacterData.eyeItemData != "" && _CharacterData.eyeItemData != null)
        {
            if (addressableDownloader == null)
            {
                addressableDownloader = AddressableDownloader.Instance;
            }
            addressableDownloader.DownloadAddressableTexture(_CharacterData.eyeItemData, this.gameObject, CurrentTextureType.EyeLense);
        }
        if (_CharacterData.skin_color != "" && _CharacterData.Skin != null)
        {
            if (_CharacterData.ai_gender == "male")
            {
                StartCoroutine(addressableDownloader.DownloadAddressableTextureByName(pathPath + "1k_Boy_Face_Texture", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Face));
                StartCoroutine(addressableDownloader.DownloadAddressableTextureByName(pathPath + "1k_Boy_Body_Texture", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Skin));
            }
            else
            {
                StartCoroutine(addressableDownloader.DownloadAddressableTextureByName(pathPath + "1k_Girl_Face_Textures", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Face));
                StartCoroutine(addressableDownloader.DownloadAddressableTextureByName(pathPath + "1k_Girl_Body_Texture", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Skin));
            }
        }
        if (_CharacterData.hairItemData != null)
        {
            if (_CharacterData.hairItemData.Equals("No hair"))
            {
                if (wornHair)
                    UnStichItem("Hair");
            }
            else
                await addressableDownloader.DownloadAddressableObj(-1, _CharacterData.hairItemData, "Hair", gender, applyOn.GetComponent<AvatarController>(), _CharacterData.hair_color, true);
        }
    }

    /// <summary>
    /// To initialize Friend Avatar according with its data
    /// </summary>
    /// <param name="_CharacterData"></param>
    /// <param name="applyOn"></param>
    public async void InitializeFrndAvatar(SavingCharacterDataClass _CharacterData, GameObject applyOn)
    {
        //added this code to resolve naked avatar 
        AvatarAndClothParent.SetActive(false);

        CharacterBodyParts bodyParts = applyOn.GetComponent<CharacterBodyParts>();
        if (_CharacterData.avatarType == null || _CharacterData.avatarType == "OldAvatar")
        {
            int _rand = Random.Range(0, 7);
            await DownloadRandomFrndPresets(_rand);
        }
        else
        {
            SetAvatarClothDefault(applyOn.gameObject, _CharacterData.gender);

            //ApplyRandomPreset:
            if (_CharacterData.myItemObj.Count > 0)
            {
                for (int i = 0; i < _CharacterData.myItemObj.Count; i++)
                {
                    string type = _CharacterData.myItemObj[i].ItemType;
                    Item item = _CharacterData.myItemObj[i];
                    string gender = _CharacterData.gender != null ? _CharacterData.gender : "Male";
                    if (!string.IsNullOrEmpty(_CharacterData.myItemObj[i].ItemName) && !_CharacterData.myItemObj[i].ItemName.Contains("default", System.StringComparison.CurrentCultureIgnoreCase) && !_CharacterData.myItemObj[i].ItemName.Contains("boy_h_066", System.StringComparison.CurrentCultureIgnoreCase)) // last condition is for the deault hairs
                    {
                        HashSet<string> itemTypes = new HashSet<string> { "Legs", "Chest", "Feet", "Hair", "EyeWearable", "Glove", "Chain" };
                        if (itemTypes.Any(item => type.Contains(item)))
                        {
                            //getHairColorFormFile = true;
                            if (!item.ItemName.Contains("md", System.StringComparison.CurrentCultureIgnoreCase) &&
                                !item.ItemName.Contains("default", System.StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (type.Contains("Hair") && (_CharacterData.hairItemData != null && _CharacterData.hairItemData.Contains("No hair")))
                                {
                                    if (wornHair)
                                        UnStichItem("Hair");
                                }
                                else
                                {
                                    if (this.gameObject.GetComponent<AvatarController>())
                                    {
                                        //StartCoroutine(AddressableDownloader.Instance.DownloadAddressableObj(item.ItemID, item.ItemName, type, gender, this.gameObject.GetComponent<AvatarController>(), Color.clear));
                                        await addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, type, gender, this.gameObject.GetComponent<AvatarController>(), _CharacterData.HairColor);
                                    }
                                }
                            }
                            else
                            {
                                if (PlayerPrefs.HasKey("Equiped") || ConstantsHolder.isNFTEquiped)
                                {
                                    if (type.Contains("Chest"))
                                    {
                                        if (wornShirt)
                                        {
                                            UnStichItem("Chest");
                                        }
                                    }
                                    else if (type.Contains("Hair"))
                                    {
                                        if (wornHair)
                                            UnStichItem("Hair");
                                    }
                                    else if (type.Contains("Legs"))
                                    {
                                        // IF fullcostume[3 piece suit] than remove bottom
                                        if (wornPant)
                                        {
                                            UnStichItem("Legs");
                                        }
                                    }
                                    else if (type.Contains("Feet"))
                                    {
                                        if (wornShoes)
                                        {
                                            UnStichItem("Feet");
                                        }
                                    }
                                    else if (type.Contains("EyeWearable"))
                                    {
                                        if (wornEyeWearable)
                                            UnStichItem("EyeWearable");
                                    }
                                    else if (type.Contains("Glove"))
                                    {
                                        if (wornGloves)
                                        {
                                            UnStichItem("Glove");
                                        }
                                    }
                                    else if (type.Contains("Chain"))
                                    {
                                        if (wornChain)
                                            UnStichItem("Chain");
                                    }
                                }
                                else
                                {
                                    WearDefaultItem(type, this.gameObject, gender);
                                }
                            }
                        }
                        else
                        {
                            WearDefaultItem(type, this.gameObject, gender);
                        }
                    }
                    else // wear the default item of that specific part.
                    {
                        if (ConstantsHolder.isNFTEquiped && type.Contains("Chest"))
                        {
                            if (wornShirt)
                                UnStichItem("Chest");
                        }
                        else
                        {
                            WearDefaultItem(type, this.gameObject, gender);
                        }
                    }
                }
            }
            // Added By WaqasAhmad
            // When User Reset From Store 
            // _CharacterData file clear & no Data is available
            // Implemented Default Cloths
            else
            {
                //if(_CharacterData.gender== "VTuber_Female")
                //{
                //    int _rand = Random.Range(0, DefaultClothDatabase.instance.VtubeFeMaleAvatarRandomPresetList.Count);
                //    string randomJson = DefaultClothDatabase.instance.VtubeMaleAvatarRandomPresetList[_rand];
                //    _CharacterData = JsonUtility.FromJson<SavingCharacterDataClass>(randomJson);
                //}
                //else
                //{
                //    int _rand = Random.Range(0, DefaultClothDatabase.instance.VtubeMaleAvatarRandomPresetList.Count);
                //    string randomJson = DefaultClothDatabase.instance.VtubeMaleAvatarRandomPresetList[_rand];
                //    _CharacterData = JsonUtility.FromJson<SavingCharacterDataClass>(randomJson);
                //}
                //goto ApplyRandomPreset;

                string gender = _CharacterData.gender != null ? _CharacterData.gender : "Male";
                WearDefaultItem("Legs", this.gameObject, gender);
                WearDefaultItem("Chest", this.gameObject, gender);
                WearDefaultItem("Feet", this.gameObject, gender);
                WearDefaultItem("Hair", this.gameObject, gender);

                //if (wornEyeWearable)
                //    UnStichItem("EyeWearable");
                //if (wornChain)
                //    UnStichItem("Chain");
                //if (wornGloves)
                //{
                //    UnStichItem("Glove");
                //    bodyParts.TextureForGlove(null);
                //}
            }
            if (_CharacterData.charactertypeAi == true)
            {
                ApplyAIData(_CharacterData, applyOn);
            }
            else
            {
                ApplyDefaultFrndData(_CharacterData, applyOn);
            }
            characterBodyParts.LoadBlendShapes(_CharacterData, applyOn);
            #region Xana Avatar 1.0   //--> remove for xana avatar2.0
            //if (_CharacterData.eyeTextureName != "" && _CharacterData.eyeTextureName != null)
            //{
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.eyeTextureName, this.gameObject, CurrentTextureType.EyeLense));
            //}

            //if (_CharacterData.eyeLashesName != "" && _CharacterData.eyeLashesName != null)
            //{
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.eyeLashesName, this.gameObject, CurrentTextureType.EyeBrowPoints));
            //}
            //if (_CharacterData.eyebrrowTexture != "" && _CharacterData.eyebrrowTexture != null)
            //{
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.eyebrrowTexture, this.gameObject, CurrentTextureType.EyeBrows));
            //}

            //if (_CharacterData.makeupName != "" && _CharacterData.makeupName != null)
            //{
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.makeupName, this.gameObject, CurrentTextureType.Makeup));
            //}

            //New texture are downloading for Boxer NFT 
            //if (!string.IsNullOrEmpty(_CharacterData.faceTattooTextureName) && _CharacterData.faceTattooTextureName != null)
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.faceTattooTextureName, this.gameObject, CurrentTextureType.FaceTattoo));
            //else
            //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.FaceTattoo);

            //if (!string.IsNullOrEmpty(_CharacterData.chestTattooTextureName) && _CharacterData.chestTattooTextureName != null)
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.chestTattooTextureName, this.gameObject, CurrentTextureType.ChestTattoo));
            //else
            //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.ChestTattoo);

            //if (!string.IsNullOrEmpty(_CharacterData.legsTattooTextureName) && _CharacterData.legsTattooTextureName != null)
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.legsTattooTextureName, this.gameObject, CurrentTextureType.LegsTattoo));
            //else
            //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.LegsTattoo);

            //if (!string.IsNullOrEmpty(_CharacterData.armTattooTextureName) && _CharacterData.armTattooTextureName != null)
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.armTattooTextureName, this.gameObject, CurrentTextureType.ArmTattoo));
            //else
            //    this.GetComponent<CharcterBodyParts>().RemoveTattoo(null, this.gameObject, CurrentTextureType.ArmTattoo);

            //if (!string.IsNullOrEmpty(_CharacterData.mustacheTextureName) && _CharacterData.mustacheTextureName != null)
            //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.mustacheTextureName, this.gameObject, CurrentTextureType.Mustache));
            //else
            //    this.GetComponent<CharcterBodyParts>().RemoveMustacheTexture(null, this.gameObject);


            //LoadBonesData(_CharacterData, this.gameObject);

            // Seperate 
            //if (_CharacterData.Skin != null)
            //{
            //    StartCoroutine(bodyParts.ImplementColors(_CharacterData.Skin, SliderType.Skin, this.gameObject));
            //    if (ConstantsHolder.xanaConstants.isNFTEquiped)
            //    {
            //        BoxerNFTEventManager._lightPresetNFT = GetLightPresetValue(_CharacterData.Skin);
            //        BoxerNFTEventManager.NFTLightUpdate?.Invoke(BoxerNFTEventManager._lightPresetNFT);
            //    }
            //}
            //if (_CharacterData.EyeColor != null)
            //{
            //    StartCoroutine(bodyParts.ImplementColors(_CharacterData.EyeColor, SliderType.EyesColor, this.gameObject));
            //}
            //if (_CharacterData.LipColor != null)
            //{
            //    StartCoroutine(bodyParts.ImplementColors(_CharacterData.LipColor, SliderType.LipsColor, this.gameObject));
            //}

            //if (_CharacterData.EyebrowColor != null)
            //{
            //    Color tempColor = _CharacterData.EyebrowColor;
            //    tempColor.a = 1;
            //    _CharacterData.EyebrowColor = tempColor;
            //    StartCoroutine(bodyParts.ImplementColors(_CharacterData.EyebrowColor, SliderType.EyeBrowColor, this.gameObject));
            //}

            //if (_CharacterData.SkinGerdientColor != null)
            //{
            //    bodyParts.ApplyGredientColor(_CharacterData.SkinGerdientColor, this.gameObject);
            //}
            //else
            //{
            //    bodyParts.ApplyGredientDefault(this.gameObject);
            //}

            //if (_CharacterData.SssIntensity != null)
            //{
            //    bodyParts.SetSssIntensity(_CharacterData.SssIntensity, this.gameObject);
            //}
            //else
            //{
            //    bodyParts.SetSssIntensity(bodyParts.defaultSssValue, this.gameObject);
            //}

            //SetItemIdsFromFile(_CharacterData);
            //bodyParts.LoadBlendShapes(_CharacterData, this.gameObject);
            #endregion  
        }
        //added this code to resolve naked avatar 
        await Task.Delay(1000);
        if (ConstantsHolder.xanaConstants.saudiEventDomeId < 0)
        {
            AvatarAndClothParent.SetActive(true);
            // In case Of VT we have disable the Meshes
            EnableClothMesh();
        }
            
    }

    /// <summary>
    /// To Download Random Presets
    /// </summary>
    /// <param name="_rand"> Rand number</param>
    public async Task DownloadRandomFrndPresets(int _rand)
    {
        string avatarGender = characterBodyParts.randomPresetData[_rand].GenderType;
        if (ProfileUIHandler.instance != null)
        {
            ProfileUIHandler.instance.ActivateProfileAvatarByGender(avatarGender);
            SetAvatarClothDefault(ProfileUIHandler.instance.avatarRef, avatarGender);
        }
        else
        {
            SetAvatarClothDefault(gameObject, avatarGender);

        }

        SavingCharacterDataClass _CharacterData = new SavingCharacterDataClass();
        if (_CharacterData.myItemObj == null || _CharacterData.myItemObj.Count == 0)
        {
            _CharacterData.myItemObj = new List<Item>();
            for (int i = 0; i < 4; i++)
                _CharacterData.myItemObj.Add(new Item(0, "", "", "", ""));
        }

        _CharacterData.myItemObj[0].ItemName = characterBodyParts.randomPresetData[_rand].PantPresetData.ObjectName;
        _CharacterData.myItemObj[0].ItemType = characterBodyParts.randomPresetData[_rand].PantPresetData.ObjectType;

        _CharacterData.myItemObj[1].ItemName = characterBodyParts.randomPresetData[_rand].ShirtPresetData.ObjectName;
        _CharacterData.myItemObj[1].ItemType = characterBodyParts.randomPresetData[_rand].ShirtPresetData.ObjectType;

        _CharacterData.myItemObj[2].ItemName = characterBodyParts.randomPresetData[_rand].HairPresetData.ObjectName;
        _CharacterData.myItemObj[2].ItemType = characterBodyParts.randomPresetData[_rand].HairPresetData.ObjectType;

        _CharacterData.myItemObj[3].ItemName = characterBodyParts.randomPresetData[_rand].ShoesPresetData.ObjectName;
        _CharacterData.myItemObj[3].ItemType = characterBodyParts.randomPresetData[_rand].ShoesPresetData.ObjectType;

        if (_CharacterData.myItemObj.Count > 0)
        {
            for (int i = 0; i < _CharacterData.myItemObj.Count; i++)
            {
                if (!string.IsNullOrEmpty(_CharacterData.myItemObj[i].ItemName))
                {
                    HashSet<string> itemTypes = new HashSet<string> { "Legs", "Chest", "Feet", "Hair", "EyeWearable", "Glove", "Chain" };

                    if (itemTypes.Any(item => _CharacterData.myItemObj[i].ItemType.Contains(item)))
                    {
                        if (!_CharacterData.myItemObj[i].ItemName.ToLowerInvariant().Contains("md"))
                        {
                            var item = _CharacterData.myItemObj[i];
                            var gender = _CharacterData.gender ?? characterBodyParts.randomPresetData[_rand].GenderType;
                            var avatarController = this.gameObject.GetComponent<AvatarController>();
                            if (addressableDownloader == null)
                            {
                                addressableDownloader = AddressableDownloader.Instance;
                            }
                            if (gameObject.activeInHierarchy)
                               await addressableDownloader.DownloadAddressableObj(item.ItemID, item.ItemName, item.ItemType, gender, avatarController, Color.clear);
                        }
                        else
                        {
                            if (PlayerPrefs.HasKey("Equiped") || ConstantsHolder.isNFTEquiped)
                            {
                                if (_CharacterData.myItemObj[i].ItemType.IndexOf("Chest", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornShirt)
                                    {
                                        UnStichItem("Chest");
                                    }
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Hair", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornHair)
                                        UnStichItem("Hair");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Legs", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornPant)
                                    {
                                        UnStichItem("Legs");
                                    }
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Feet", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornShoes)
                                    {
                                        UnStichItem("Feet");
                                    }
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("EyeWearable", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornEyeWearable)
                                        UnStichItem("EyeWearable");
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Glove", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornGloves)
                                    {
                                        UnStichItem("Glove");
                                    }
                                }
                                else if (_CharacterData.myItemObj[i].ItemType.IndexOf("Chain", StringComparison.Ordinal) >= 0)
                                {
                                    if (wornChain)
                                        UnStichItem("Chain");
                                }
                            }
                            else
                            {
                                var item = _CharacterData.myItemObj[i];
                                var gender = _CharacterData.gender ?? "Male";
                                WearDefaultItem(item.ItemType, this.gameObject, gender);
                            }
                        }
                    }
                    else
                    {
                        var item = _CharacterData.myItemObj[i];
                        var gender = _CharacterData.gender ?? "Male";
                        WearDefaultItem(item.ItemType, this.gameObject, gender);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Apply Default Data to Frnd 
    /// </summary>
    /// <param name="_CharacterData"></param>
    /// <param name="applyOn"></param>
    void ApplyDefaultFrndData(SavingCharacterDataClass _CharacterData, GameObject applyOn)
    {
        if (_CharacterData.gender == AvatarGender.VTuber_Female.ToString() || _CharacterData.gender == AvatarGender.VTuber_Male.ToString())
        {
            if (!String.IsNullOrEmpty(_CharacterData.eyeTextureName))
            {
                addressableDownloader.DownloadAddressableTexture(_CharacterData.eyeTextureName, characterBodyParts.gameObject, CurrentTextureType.EyeLense);
            }
            else
            {
                applyOn.GetComponent<CharacterBodyParts>().ApplyEyeLenTexture(applyOn.GetComponent<CharacterBodyParts>().Eye_Texture, applyOn);
            }
            return;
        }


        if (_CharacterData.gender == AvatarGender.Male.ToString())
        {
            characterBodyParts.head.materials[2].SetColor("_BaseColor", new Color(1, 1, 1, 1));
            characterBodyParts.head.materials[2].SetColor("_Lips_Color", new Color(0.9137255f, 0.4431373f, 0.4352941f, 1));
            characterBodyParts.body.materials[0].SetColor("_BaseColor", new Color(1, 1, 1, 1));
            characterBodyParts.ApplyEyeLenTexture(CharacterHandler.instance.maleAvatarData.DEye_texture, characterBodyParts.gameObject);
        }
        else if (_CharacterData.gender == AvatarGender.Female.ToString())
        {
            characterBodyParts.head.materials[2].SetColor("_BaseColor", new Color(1, 1, 1, 1));
            characterBodyParts.head.materials[2].SetColor("_Lips_Color", new Color(0.9137255f, 0.4431373f, 0.4352941f, 1));
            characterBodyParts.body.materials[0].SetColor("_BaseColor", new Color(1, 1, 1, 1));
            characterBodyParts.ApplyEyeLenTexture(CharacterHandler.instance.femaleAvatarData.DEye_texture, characterBodyParts.gameObject);
        }

        if (_CharacterData.skin_color != "" && _CharacterData.Skin != null)
        {
            if (_CharacterData.ai_gender == "male")
            {
                StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTextureByName("Assets/Store Items Addressables/1k_Boy_Face_Texture", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Face));
                StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTextureByName("Assets/Store Items Addressables/1k_Boy_Body_Texture", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Skin));
            }
            else
            {
                StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTextureByName("Assets/Store Items Addressables/1k_Girl_Face_Textures", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Face));
                StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTextureByName("Assets/Store Items Addressables/1k_Girl_Body_Texture", _CharacterData.skin_color, this.gameObject, CurrentTextureType.Skin));
            }
        }
        if (_CharacterData.hairItemData != null)
        {
            characterBodyParts.head.materials[2].SetColor("_BaseColor", new Color(1, 1, 1, 1));
            characterBodyParts.head.materials[2].SetColor("_Lips_Color", new Color(0.9137255f, 0.4431373f, 0.4352941f, 1));
            characterBodyParts.body.materials[0].SetColor("_BaseColor", new Color(1, 1, 1, 1));
            characterBodyParts.ApplyEyeLenTexture(CharacterHandler.instance.femaleAvatarData.DEye_texture, characterBodyParts.gameObject);
        }
        for (int i = 0; i < characterBodyParts.head.sharedMesh.blendShapeCount - 1; i++)
        {
            characterBodyParts.head.SetBlendShapeWeight(i, 0);
        }
    }

    /// <summary>
    /// Set player for AR scene
    /// </summary>
    public void SetAvatarForAR()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetFloat("Blend", 0.0f);
        anim.SetBool("Action", false);
        anim.SetBool("isMoving", false);
        anim.SetBool("idel", true);
        gameObject.GetComponent<CharacterOnScreenNameHandler>().enabled = false;
        anim.runtimeAnimatorController = ArAnimator;
    }


    #region Commented Function
    // void OnBecameInvisible()
    //{
    //    isVisibleOnCam = false;
    //}

    //void OnBecameVisible()
    //{
    //    isVisibleOnCam = true;
    //}
    //public void ApplyPreset(SavingCharacterDataClass _CharacterData)
    //{
    //    presetHairColor = _CharacterData.HairColor;
    //    if (_CharacterData.myItemObj.Count > 0)
    //    {
    //        for (int i = 0; i < _CharacterData.myItemObj.Count; i++)
    //        {
    //            if (!string.IsNullOrEmpty(_CharacterData.myItemObj[i].ItemName))
    //            {
    //                string type = _CharacterData.myItemObj[i].ItemType;
    //                if (type.Contains("Legs") || type.Contains("Chest") || type.Contains("Feet") || type.Contains("Hair") || type.Contains("EyeWearable"))
    //                {
    //                    if (!_CharacterData.myItemObj[i].ItemName.Contains("md", System.StringComparison.CurrentCultureIgnoreCase))
    //                    {
    //                        StartCoroutine(AddressableDownloader.Instance.DownloadAddressableObj(_CharacterData.myItemObj[i].ItemID, _CharacterData.myItemObj[i].ItemName, type, _CharacterData.gender != null ? _CharacterData.gender : "Male", this.gameObject.GetComponent<AvatarController>(), Color.clear));
    //                    }
    //                    else
    //                    {
    //                        WearDefaultItem(type, this.gameObject, _CharacterData.gender != null ? _CharacterData.gender : "Male");
    //                    }
    //                }
    //                else
    //                {
    //                    WearDefaultItem(_CharacterData.myItemObj[i].ItemType, this.gameObject, _CharacterData.gender != null ? _CharacterData.gender : "Male");
    //                }
    //            }
    //            else // wear the default item of that specific part.
    //            {
    //                WearDefaultItem(_CharacterData.myItemObj[i].ItemType, this.gameObject, _CharacterData.gender != null ? _CharacterData.gender : "Male");
    //            }
    //        }
    //    }

    //    #region Xana Avatar 1.0 //--> remove for xana avatar2.0
    //    //if (_CharacterData.eyeTextureName != "" && _CharacterData.eyeTextureName != null)
    //    //{
    //    //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.eyeTextureName, this.gameObject, CurrentTextureType.EyeLense));
    //    //}
    //    //if (_CharacterData.eyebrrowTexture != "" && _CharacterData.eyebrrowTexture != null)
    //    //{
    //    //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.eyebrrowTexture, this.gameObject, CurrentTextureType.EyeBrows));
    //    //}
    //    //if (_CharacterData.eyeLashesName != "" && _CharacterData.eyeLashesName != null)
    //    //{
    //    //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.eyeLashesName, this.gameObject, CurrentTextureType.EyeBrowPoints));
    //    //}
    //    //if (_CharacterData.makeupName != "" && _CharacterData.makeupName != null)
    //    //{
    //    //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture(_CharacterData.makeupName, this.gameObject, CurrentTextureType.Makeup));
    //    //}
    //    //else
    //    //{
    //    //    StartCoroutine(AddressableDownloader.Instance.DownloadAddressableTexture("nomakeup", this.gameObject, CurrentTextureType.Makeup));
    //    //}
    //    //LoadBonesData(_CharacterData, this.gameObject);

    //    //if (_CharacterData.Skin != null && _CharacterData.LipColor != null && _CharacterData.HairColor != null && _CharacterData.EyebrowColor != null && _CharacterData.EyeColor != null)
    //    //{
    //    //    // Seperate 
    //    //    if (_CharacterData.Skin != null)
    //    //    {
    //    //        StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.Skin, SliderType.Skin, this.gameObject));
    //    //    }
    //    //    if (_CharacterData.EyeColor != null)
    //    //    {
    //    //        StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.EyeColor, SliderType.EyesColor, this.gameObject));
    //    //    }
    //    //    if (_CharacterData.LipColor != null)
    //    //    {
    //    //        StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.LipColor, SliderType.LipsColor, this.gameObject));
    //    //    }

    //    //    if (_CharacterData.EyebrowColor != null)
    //    //    {
    //    //        StartCoroutine(characterBodyParts.ImplementColors(_CharacterData.EyebrowColor, SliderType.EyeBrowColor, this.gameObject));
    //    //    }
    //    //}

    //    //if (_CharacterData.SkinGerdientColor != null)
    //    //{
    //    //    characterBodyParts.ApplyGredientColor(_CharacterData.SkinGerdientColor, this.gameObject);
    //    //}
    //    //else
    //    //{
    //    //    characterBodyParts.ApplyGredientDefault(this.gameObject);
    //    //}

    //    //if (_CharacterData.SssIntensity != null)
    //    //{
    //    //    characterBodyParts.SetSssIntensity(_CharacterData.SssIntensity, this.gameObject);
    //    //}
    //    //else
    //    //{
    //    //    characterBodyParts.SetSssIntensity(characterBodyParts.defaultSssValue, this.gameObject);
    //    //}

    //    //SetItemIdsFromFile(_CharacterData);

    //    //EyesBlinking.instance.isBlinking = false;
    //    //characterBodyParts.LoadBlendShapes(_CharacterData, this.gameObject);
    //    //characterBodyParts.ApplyPresiteGredient();

    //    //EyesBlinking.instance.StoreBlendShapeValues();          // Added by Ali Hamza
    //    #endregion
    //}
    //private void SetItemIdsFromFile(SavingCharacterDataClass _CharacterData)
    //{
    //    presetValue = _CharacterData.PresetValue;
    //    hairColorPaletteId = _CharacterData.HairColorPaletteValue;
    //    skinId = _CharacterData.SkinId;
    //    faceId = _CharacterData.FaceValue;
    //    eyeBrowId = _CharacterData.EyeBrowValue;
    //    eyeBrowColorPaletteId = _CharacterData.EyeBrowColorPaletteValue;
    //    eyesId = _CharacterData.EyeValue;
    //    eyesColorId = _CharacterData.EyesColorValue;
    //    eyesColorPaletteId = _CharacterData.EyesColorPaletteValue;
    //    noseId = _CharacterData.NoseValue;
    //    lipsId = _CharacterData.LipsValue;
    //    lipsColorId = _CharacterData.LipsColorValue;
    //    lipsColorPaletteId = _CharacterData.LipsColorPaletteValue;
    //    bodyFat = _CharacterData.BodyFat;
    //    eyeLashesId = _CharacterData.EyeLashesValue;
    //    makeupId = _CharacterData.MakeupValue;
    //}

    //public void ResetForLastSaved()
    //{
    //    //body fats
    //    SaveCharacterProperties.instance.SaveItemList.BodyFat = 0;
    //    //body blends
    //    AvatarCustomizationManager.Instance.UpdateChBodyShape(0);

    //    itemDatabase.RevertSavedCloths();
    //}

    //public void LastSaved_Reset()
    //{
    //    itemDatabase.RevertSavedCloths();
    //}
    #endregion
}