using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static SummitAIChatHandler;
using UnityEditor;
using UnityEngine.Networking.Types;

[CreateAssetMenu(menuName = "ScriptableObjects/SummitDataContainer", fileName = "ScriptableObjects/SummitDataContainer")]
public class XANASummitDataContainer : ScriptableObject
{

    public GameObject maleAIAvatar;
    public GameObject femaleAIAvatar;
    public GameObject penguinAvatar;
    public string[] avatarJson;
    public DomeData summitData = new DomeData();
    public SubWorldList SubWorldData = new SubWorldList();
    public DomeInstructionData DomeInstructions = new DomeInstructionData();
    public DomeAccessData DomeAccessList = new DomeAccessData();
    public AIData aiData = new AIData();
    public static string FixedAvatarJson;
    public static Stack<StackInfoWorld> LoadedScenesInfo = new Stack<StackInfoWorld>();
    public static List<GameObject> SceneTeleportingObjects = new List<GameObject>();
    public static bool Penpenz = false;

    public static int DomeIdForSummit = 999;
    string[] s = { "ZONE-X", "ZONE X Musuem", "Xana Lobby", "XANA Festival Stage", "Xana Festival", "THE RHETORIC STAR", "ROCK?N ROLL CIRCUS", "MASAMI TANAKA", "Koto-ku Virtual Exhibition", "JJ MUSEUM", "HOKUSAI KATSUSHIKA", "Green Screen Studio", "GOZANIMATOR HARUNA GOUZU GALLERY 2021", "Genesis ART Metaverse Museum", "FIVE ELEMENTS", "DEEMO THE MOVIE Metaverse Museum", "D_Infinity_Labo", "BreakingDown Arena", "Astroboy x Tottori Metaverse Museum" };

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
    }

    public async void GetAllDomesData()
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETALLDOMES;
        string result = await GetAsyncRequest(url);
        summitData = JsonUtility.FromJson<DomeData>(result);
        SetExteriorDomeInfo();
        //// Activate Map
        //ReferencesForGamePlay.instance.FullScreenMapStatus(true);
        GameplayEntityLoader.instance.ForcedMapOpenForSummitScene();
    }

    public async void GetAllSubDomeData()
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETALLSUBDOME;
        string result = await GetAsyncRequest(url);
        SubWorldData = JsonUtility.FromJson<SubWorldList>(result);
    }

    public async Task<bool> GetAIData(string APIURL, int domeId)
    {
        string url = ConstantsGod.API_BASEURL + APIURL + domeId + "/" + 1;
        string result = await GetAsyncRequest(url);
        aiData = JsonUtility.FromJson<AIData>(result);

        return aiData.npcData.Count > 0;
    }


    public async Task<SingleDomeData> GetSingleDomeData(int _DomeId)
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETSINGLEDOMEMEDIA + _DomeId;
        string result = await GetAsyncRequest(url);
        return JsonUtility.FromJson<SingleDomeData>(result);

    }

    public async void GetDomeInstruction()
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETALLINSTRUCTION;
        string result = await GetAsyncRequest(url);
        DomeInstructions = JsonUtility.FromJson<DomeInstructionData>(result);
    }

    public async Task<InstructionData[]> GetSubDomeInstruction(int SubDomeId)
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETSUBDOMEINSTRUCTION + SubDomeId;
        string result = await GetAsyncRequest(url);
        SubDomeInstruction instructionDatas = JsonUtility.FromJson<SubDomeInstruction>(result);
        return instructionDatas.data;
    }

    public async void GetDomeAccessList()
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETALLDOMEACCESS;
        string result = await GetAsyncRequest(url);
        DomeAccessList = JsonUtility.FromJson<DomeAccessData>(result);
    }


    public async Task<DomeAccessMailInfo[]> GetSubDomeAccessList(int SubDomeId)
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETSUBDOMEACCESS + SubDomeId;
        string result = await GetAsyncRequest(url);
        SubDomeAccesInfo SubDomeAccess = JsonUtility.FromJson<SubDomeAccesInfo>(result);
        return SubDomeAccess.data;
    }


    public DomeAccessMailInfo[] GetDomeAccessData(int DomeId)
    {
        for (int i = 0; i < DomeAccessList.data.Length; i++)
        {
            if (DomeAccessList.data[i].domeId == DomeId)
            {
                return DomeAccessList.data[i].userData;
            }
        }
        return null;
    }

    async Task<string> GetAsyncRequest(string url)
    {
        string response = string.Empty;
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
            await www.SendWebRequest();
            while (!www.isDone)
                await System.Threading.Tasks.Task.Yield();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                response = www.error;
            }
            else
            {
                response = www.downloadHandler.text;
            }
            www.Dispose();
        };
        return response;

    }

    async Task<string> GetTokenBasedAsyncRequest(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
        await www.SendWebRequest();
        while (!www.isDone)
            await System.Threading.Tasks.Task.Yield();

        Debug.LogError("----"+ www.downloadHandler.text);
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            return www.error;
        }
        else
            return www.downloadHandler.text;

    }


    public async Task<string> GetAudioFile(int domeId)
    {
        while (summitData.domes.Count == 0)
        {
            await Task.Delay(1000);
        }
        for (int i = 0; i < summitData.domes.Count; i++)
        {
            if (domeId == summitData.domes[i].id)
            {
                return summitData.domes[i].domes_media_v2.bgm;
            }
        }

        return string.Empty;
    }

    public async Task<string> GetAudioFileForSubWorld(int domeId, int SubWorldId)
    {
        while (SubWorldData.data.Length == 0)
        {
            await Task.Delay(1000);
        }
        for (int i = 0; i < SubWorldData.data.Length; i++)
        {
            if (domeId == SubWorldData.data[i].domeId)
            {
                for (int j = 0; j < SubWorldData.data[i].subWorldDomes.Length; j++)
                {
                    if (SubWorldId == SubWorldData.data[i].subWorldDomes[j].subWorldId)
                    {
                        return SubWorldData.data[i].subWorldDomes[j].domes_media_v2.bgm;
                    }
                }
            }
        }

        return string.Empty;
    }


    public string[] GetDomeImage(int DomeId)
    {
        for (int i = 0; i < summitData.domes.Count; i++)
        {
            if (DomeId == summitData.domes[i].id)
            {
                if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
                {
                    return new[] { summitData.domes[i].domes_media_v2.thumbnail, summitData.domes[i].jpWorldName, summitData.domes[i].domes_media_v2.companyLogo };
                }
                else
                {
                    return new[] { summitData.domes[i].domes_media_v2.thumbnail, summitData.domes[i].name, summitData.domes[i].domes_media_v2.companyLogo };
                }
            }
        }

        return new[] { string.Empty, string.Empty, string.Empty };
    }

    public DomeGeneralData GetDomeData(int DomeId)
    {
        for (int i = 0; i < summitData.domes.Count; i++)
        {
            if (DomeId == summitData.domes[i].id)
            {
                return summitData.domes[i];
            }
        }
        return null;
    }

    public bool DomeHasSubWorld(int DomeId)
    {
        for (int i = 0; i < SubWorldData.data.Length; i++)
        {
            if (SubWorldData.data[i].domeId == DomeId)
            {
                if (SubWorldData.data[i].subWorldDomes.Length > 0)
                    return true;
                else
                    return false;
            }
        }
        return false;
    }

    public DomeGeneralData GetSubDomeData(int DomeId, int SubDomeId)
    {
        for (int i = 0; i < SubWorldData.data.Length; i++)
        {
            if (SubWorldData.data[i].domeId == DomeId)
            {
                for (int j = 0; j < SubWorldData.data[i].subWorldDomes.Length; j++)
                {
                    if (SubWorldData.data[i].subWorldDomes[j].subWorldId == SubDomeId)
                    {
                        return SubWorldData.data[i].subWorldDomes[j];
                    }

                }
            }
        }
        return null;
    }

    public InstructionData[] GetDomeInstruction(int DomeId)
    {
        for (int i = 0; i < DomeInstructions.data.Length; i++)
        {
            if (DomeId == DomeInstructions.data[i].domeId)
            {
                return DomeInstructions.data[i].instructions;
            }
        }
        return null;
    }

    public async Task<int> GetVisitorCount(string worldId)
    {
        string apiUrl = ConstantsGod.API_BASEURL + ConstantsGod.VISITORCOUNT + worldId;
        string reponse = await GetTokenBasedAsyncRequest(apiUrl);
        VisitorInfo visitorInfo = JsonUtility.FromJson<VisitorInfo>(reponse);
        if (visitorInfo.success)
            return visitorInfo.data.total_visit;
        else
            return 100;
    }


    public async Task<PresentationTab> GetPresentationTabData(string DomeId)
    {
        Debug.LogError("---" + DomeId);
        string apiUrl = ConstantsGod.API_BASEURL + ConstantsGod.GETPRESENTATIONDATA + DomeId;
        string reponse = await GetTokenBasedAsyncRequest(apiUrl);
        PresentationTab PresentationInfo = JsonUtility.FromJson<PresentationTab>(reponse);
        return PresentationInfo;
    }


    public void SetExteriorDomeInfo()
    {
        DomeGeneralData singleDomeData = GetDomeData(999);
        if (singleDomeData == null)
        {
            Debug.LogError("singleDomeData is null!");
            ConstantsHolder.WorldName = string.Empty;
            ConstantsHolder.description = string.Empty;
            ConstantsHolder.CreatorName = string.Empty;
            ConstantsHolder.DomeType = string.Empty;
            ConstantsHolder.DomeCategory = string.Empty;
            ConstantsHolder.userLimit = 0;
            return;
        }
        if (LocalizationManager.forceJapanese || GameManager.currentLanguage.Equals("ja"))
        {
            ConstantsHolder.WorldName = singleDomeData.name ?? string.Empty;
            ConstantsHolder.description = singleDomeData.jpDescription ?? string.Empty;
            ConstantsHolder.CreatorName = singleDomeData.jpCreatorName ?? string.Empty;
        }
        else
        {
            ConstantsHolder.WorldName = singleDomeData.jpWorldName ?? string.Empty;
            ConstantsHolder.description = singleDomeData.description ?? string.Empty;
            ConstantsHolder.CreatorName = singleDomeData.creatorName ?? string.Empty;
        }
        ConstantsHolder.DomeType = singleDomeData.domeType ?? string.Empty;
        ConstantsHolder.DomeCategory = singleDomeData.domeCategory ?? string.Empty;
        ConstantsHolder.userLimit = singleDomeData.maxPlayer;

        Debug.LogError(ConstantsHolder.CreatorName + "--" + ConstantsHolder.WorldName + "--" + ConstantsHolder.description + "---" + ConstantsHolder.DomeType + "----" + ConstantsHolder.DomeCategory);
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.EnteredPlayMode:
                summitData.domes.Clear();
                break;

            case PlayModeStateChange.ExitingPlayMode:
                summitData.domes.Clear();
                break;
        }
    }
#endif
    #region DomeInfo

    [System.Serializable]
    public class DomeData
    {
        public List<DomeGeneralData> domes;
    }

    [System.Serializable]
    public class SingleDomeData
    {
        public MediaInformation dome;
    }

    [System.Serializable]
    public class DomeGeneralData
    {
        public int subWorldId;
        public int id;
        public string name;
        public string description;
        public string creatorName;
        public string jpWorldName;
        public string jpDescription;
        public string jpCreatorName;
        //public string bgm;
        //public string thumbnail;
        public bool worldType;
        public int worldId;
        public string world;
        public string experienceType;
        public int builderWorldId;
        public bool IsPenguin;
        public bool Ishumanoid;
        public int AvatarIndex;
        public string Avatarjson;
        //public string world360Image;
        //public string companyLogo;
        public int maxPlayer;
        public bool is_penpenz;
        public int isDomeOn;
        public int isAccessGiven;
        //public List<SubWorldInfo> SubWorlds;
        //public bool isSubWorld;
        public string domeType;
        public string domeCategory;
        //public string mediaType;
        //public string proportionType;
        //public bool isYoutubeUrl;
        //public string videoType;
        //public string mediaUpload;
        public bool isBuilderGame;
        //public List<InstructionData> instruction;
        public MediaInformation domes_media_v2;
    }

    [System.Serializable]
    public class MediaInformation
    {
        public string bgm;
        public string thumbnail;
        public string companyLogo;
        public MediaJson[] mediajson;
        public int isAccessGiven;
    }

    [System.Serializable]
    public class MediaJson
    {
        public string mediaType;
        public bool isYoutubeUrl;
        public string mediaUrl;
        public string videoUrlType;
        public string proportionType;
    }


    [System.Serializable]
    public class SubWorldList
    {
        public bool success;
        public SubWorldInfo[] data;
        public string msg;
    }

    [System.Serializable]
    public class SubWorldInfo
    {
        public int domeId;
        public DomeGeneralData[] subWorldDomes;

    }
    //[System.Serializable]
    //public class SubWorldGeneralMedia
    //{
    //    public DomeGeneralData subWorldGeneralInfos;
    //}

    [System.Serializable]
    public class DomeInstructionData
    {
        public AllDomeInstruction[] data;
    }

    [System.Serializable]
    public class AllDomeInstruction
    {
        public int domeId;
        public InstructionData[] instructions;
    }

    [System.Serializable]
    public class SubDomeInstruction
    {
        public InstructionData[] data;
    }

    [System.Serializable]
    public class InstructionData
    {
        public int id;
        public string instructionThumbnail;
        public string instructionName;
        public string instructionDescription;
        public string jpInstructionDescription;
        public string jpInstructionName;
        public int isAccessGiven;
    }


    [System.Serializable]
    public class OfficialWorldDetails
    {
        public int id;
        public string label;
        public string icon;
        public int userLimit;
        public string subWorldType;
        public string subWorldCategory;
        public string creatorName;
        public string description;
    }


    [System.Serializable]
    public class StackInfoWorld
    {
        public string id;
        public string name;
        public int user_limit;
        public string thumbnail;
        public string banner;
        public string thumbnail_new;
        public string description;
        public string creator;
        public string domeType;
        public string domeCategory;
        public bool isBuilderWorld;
        public int domeId;
        public bool haveSubWorlds;
        public bool IsSubWorld;
        public bool isFromSummitWorld;
        public Vector3[] playerTrasnform;
    }
    #endregion



    #region AINPC Data Classes
    [System.Serializable]
    public class AIData
    {
        public List<AINPCInfo> npcData;
    }

    [System.Serializable]
    public class AINPCInfo
    {
        public int id;
        public int domeId;
        public string language;
        public string name;
        public string jpName;
        public int avatarId;
        public string avatarCategory;
        public string personalityURL;
        public float[] rotationPositionArray;
        public float[] spawnPositionArray;
        public bool isAvatarPerformer;
        public AnimationData[] animations;
        public int isAccessGiven;
    }
    [System.Serializable]
    public class AnimationData
    {
        public string name;
        public float playTime;
    }
    #endregion

    #region Visitor Count
    [System.Serializable]
    public class VisitorInfo
    {
        public bool success;
        public VisitorData data;
    }

    [System.Serializable]
    public class VisitorData
    {
        public int total_visit;
    }
    #endregion


    #region DomeAccess Classes
    [System.Serializable]
    public class DomeAccessData
    {
        public AllDomeAccess[] data;
    }

    [System.Serializable]
    public class AllDomeAccess
    {
        public int domeId;
        public DomeAccessMailInfo[] userData;
    }

    [System.Serializable]
    public class DomeAccessMailInfo
    {
        public string email;
        public string avatar;
        public int isAccessGiven;
    }

    [System.Serializable]
    public class SubDomeAccesInfo
    {
        public DomeAccessMailInfo[] data;
    }

    #endregion

    #region Presentation Data Classes
    [System.Serializable]
    public class PresentationTab
    {
        public PresentationTabData[] data;
    }
    [System.Serializable]
    public class PresentationTabData
    {
        public int id;
        public string domeId;
        public string email;
        public string wallet_Address;
        public int area_id;
        public bool is_mute;
        public bool isAccessGiven;

    }
    #endregion
}
