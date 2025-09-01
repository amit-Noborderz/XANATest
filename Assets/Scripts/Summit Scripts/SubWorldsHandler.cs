using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static XANASummitDataContainer;

public class SubWorldsHandler : MonoBehaviour
{
    [SerializeField]
    private StayTimeTrackerForSummit _stayTimeTrackerForSummit;
    private string _domeID;
    private bool _isBuilderWorld;
    public bool IsEnteringInSubWorld = false;
    public GameObject SubworldListParent;
    public Transform ContentParent;
    public GameObject SubworldPrefab;

    [Header("Description Panel Objects")]
    public GameObject DescriptionPanelParent;
    public Image ThumbnailImage;
    public TMPro.TextMeshProUGUI WorldName;
    public TMPro.TextMeshProUGUI WorldDescription;
    public TMPro.TextMeshProUGUI WorldCreatorName;
    public TMPro.TextMeshProUGUI WorldType;
    public TMPro.TextMeshProUGUI WorldCategory;
    public TMPro.TextMeshProUGUI WorldEstTime;
    public TMPro.TextMeshProUGUI DomeId;
    public Button EnterButton;
    public GameObject EnterButtonAnimation;
    public Button BackButton;

    public XANASummitDataContainer XANASummitDataContainer;
    public XANASummitSceneLoading XANASummitSceneLoadingInstance;

    public static Action<Sprite, string, string, string, string, bool, string, string, Vector3, bool, int> OpenSubWorldDescriptionPanel;
    //public static int CurrentlyLoadedDomes;

    private string worldId;
    private Vector3 playerReturnPosition;
    private List<GameObject> subworldsList = new List<GameObject>();
    //public OfficialWorldDetails selectedWold;

    // Start is called before the first frame update
    void OnEnable()
    {
        //BuilderEventManager.AfterWorldInstantiated += AddSubWorld;
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated += AddSubWorld;
        OpenSubWorldDescriptionPanel += OpenDescirptionPanel;

        EnterButton.onClick.AddListener(EnterWorld);
        BackButton.onClick.AddListener(OnBack);

    }

    private void OnDisable()
    {
        //BuilderEventManager.AfterWorldInstantiated -= AddSubWorld;
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated -= AddSubWorld;
        OpenSubWorldDescriptionPanel -= OpenDescirptionPanel;

        EnterButton.onClick.RemoveListener(EnterWorld);
        BackButton.onClick.RemoveListener(OnBack);
    }


    /// <summary>
    /// Code looks complicated because we have handled all condition like we more number of subworlds added at backend side in scene does not have same number of teleport object and vice versa
    /// </summary>
    void AddSubWorld()
    {
        if (ConstantsHolder.HaveSubWorlds)
        {
            for (int i = 0; i < XANASummitDataContainer.SubWorldData.data.Length; i++)
            {
                if (ConstantsHolder.domeId == XANASummitDataContainer.SubWorldData.data[i].domeId)
                {
                    for (int j = 0; j < XANASummitDataContainer.SceneTeleportingObjects.Count; j++)
                    {
                        //if (j < XANASummitDataContainer.SceneTeleportingObjects.Count)
                        //{
                        int subworldIndex = XANASummitDataContainer.SceneTeleportingObjects[j].GetComponent<SummitSubWorldIndex>().SubworldIndex;
                        if (subworldIndex < XANASummitDataContainer.SubWorldData.data.Length)
                        {
                            XANASummitDataContainer.SceneTeleportingObjects[j].gameObject.AddComponent<OnTriggerSceneSwitch>();
                            XANASummitDataContainer.SceneTeleportingObjects[j].gameObject.GetComponent<OnTriggerSceneSwitch>().DomeId = -1;
                            if (XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[subworldIndex].worldType)
                                XANASummitDataContainer.SceneTeleportingObjects[j].gameObject.GetComponent<OnTriggerSceneSwitch>().WorldId = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[subworldIndex].builderWorldId.ToString();
                            else
                                XANASummitDataContainer.SceneTeleportingObjects[j].gameObject.GetComponent<OnTriggerSceneSwitch>().WorldId = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[subworldIndex].worldId.ToString();
                        }
                        //}
                    }
                    return;
                }
            }
        }
    }



    public Task<bool> CreateSubWorldList(XANASummitDataContainer.DomeGeneralData domeGeneralData, Vector3 PlayerReturnPosition)
    {
        bool hasSubworldCheck = false;
        ClearOldData();
        SubworldListParent.SetActive(true);
        for (int i = 0; i < XANASummitDataContainer.SubWorldData.data.Length; i++)
        {
            if (XANASummitDataContainer.SubWorldData.data[i].domeId == domeGeneralData.id)
            {
                for (int j = 0; j < XANASummitDataContainer.SubWorldData.data[i].subWorldDomes.Length; j++)
                {
                    GameObject temp = Instantiate(SubworldPrefab, ContentParent);
                    SubWorldPrefab _SubWorldPrefab = temp.GetComponent<SubWorldPrefab>();
                    if (XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].worldType)
                        _SubWorldPrefab.WorldId = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].builderWorldId.ToString();
                    else
                        _SubWorldPrefab.WorldId = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].worldId.ToString();
                    if (XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].name.Contains("D + Infinity Labo") || XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].name.Contains("D +  Infinity Labo"))
                    {
                        XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].name = "D_Infinity_Labo";
                    }
                    _SubWorldPrefab.SubDomeId = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].subWorldId;
                    _SubWorldPrefab.SubWorldName = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].name;
                    if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
                        _SubWorldPrefab.WorldDescription = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].jpDescription;
                    else
                        _SubWorldPrefab.WorldDescription = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].description;
                    _SubWorldPrefab.CreatorName = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].creatorName;
                    _SubWorldPrefab.IsBuilderWorld = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].worldType;
                    _SubWorldPrefab.WorldCategory = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].domeCategory;
                    _SubWorldPrefab.WorldTimeEstimate = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].name;
                    _SubWorldPrefab.WorldDomeId = XANASummitDataContainer.SubWorldData.data[i].domeId.ToString();
                    _SubWorldPrefab.ThumbnailUrl = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].domes_media_v2.thumbnail;
                    _SubWorldPrefab.WorldName.text = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].name;
                    _SubWorldPrefab.subWorldType = XANASummitDataContainer.SubWorldData.data[i].subWorldDomes[j].domeType;
                    _SubWorldPrefab.PlayerReturnPosition = PlayerReturnPosition;
                    _SubWorldPrefab.Init();
                    subworldsList.Add(temp);
                }
            }
        }
        return new Task<bool>(() => hasSubworldCheck);

    }

    async void OpenDescirptionPanel(Sprite thumbnailImage, string _worldId, string worldName, string worldDesCription, string creatorName, bool worldType, string worldCategory, string worldDomeId, Vector3 _playerReturnPosition, bool isBuilderWorld, int SubDomeId)
    {
        worldId = _worldId;
        _isBuilderWorld = isBuilderWorld;
        _domeID = worldDomeId;
        IsEnteringInSubWorld = true;
        //BuilderEventManager.LoadSceneByName?.Invoke(worldId, _playerReturnPosition);
        BuilderEventManager.LoadNewScene?.Invoke(int.Parse(_domeID), _playerReturnPosition,true);
        InstructionData[] instructionDatas = await XANASummitDataContainer.GetSubDomeInstruction(SubDomeId);
        LoadingHandler.Instance.LoadInstructionData(instructionDatas);
        (bool IsSpecialAvatar, string SpecialAvatarJson) = DomeAccessHandler.CheckPlayerForSpecialAvtar(await XANASummitDataContainer.GetSubDomeAccessList(ConstantsHolder.SubDomeId));
        if (IsSpecialAvatar)
        {
            ConstantsHolder.isFixedHumanoid = true;
            XANASummitDataContainer.FixedAvatarJson = SpecialAvatarJson;
            ConstantsHolder.xanaConstants.SetPlayerProperties(SpecialAvatarJson);
        }
    }

    public void CloseSubWorldList()
    {
        SubworldListParent.SetActive(false);
        ConstantsHolder.xanaConstants.haveSubDomeEnabled = false;
    }

    public void EnterWorld()
    {
        BuilderEventManager.LoadSceneByName?.Invoke(worldId, playerReturnPosition);
        XANASummitSceneLoading.setPlayerPositionDelegate += OnEnteredIntoWorld;
        EnterButton.interactable = false;
        BackButton.interactable = false;
        EnterButtonAnimation.SetActive(true);
        //_isEnteringInSubWorld = true;
    }

    public void OnEnteredIntoWorld()
    {
        SubworldListParent.SetActive(false);
        DescriptionPanelParent.SetActive(false);
        EnterButton.interactable = true;
        BackButton.interactable = true;
        EnterButtonAnimation.SetActive(false);
    }

    public void OnBack()
    {
        DescriptionPanelParent.SetActive(false);
    }

    void ClearOldData()
    {
        int x = subworldsList.Count;
        for (int i = 0; i < x; i++)
            Destroy(subworldsList[i]);

        subworldsList.Clear();
        SubworldListParent.SetActive(false);
    }
    public void CallAnalyticsForSubWorlds()
    {
        if (_stayTimeTrackerForSummit != null)
        {
            if (_stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea)
                _stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea = false;
            _stayTimeTrackerForSummit.DomeId = int.Parse(_domeID);
            _stayTimeTrackerForSummit.DomeWorldId = int.Parse(worldId);
            _stayTimeTrackerForSummit.IsBuilderWorld = _isBuilderWorld;
            string eventName;
            if (_isBuilderWorld)
                eventName = "TV_Dome_" + _stayTimeTrackerForSummit.DomeId + "_BW_" + _stayTimeTrackerForSummit.DomeWorldId;
            else
                eventName = "TV_Dome_" + _stayTimeTrackerForSummit.DomeId + "_XW_" + _stayTimeTrackerForSummit.DomeWorldId;
            GlobalConstants.SendFirebaseEventForSummit(eventName);
            _stayTimeTrackerForSummit.StartTrackingTime();
        }
    }


}
