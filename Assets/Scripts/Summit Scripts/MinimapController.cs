using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using static XANASummitDataContainer;
using SuperStar.Helpers;

public class MinimapController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float moveDuration = 1.0f;
    public List<GameObject> MapHighlightObjs;

    public GameObject mainScreen; // Reference to the parent (Main Screen)
    public GameObject childObject; // Reference to the child object containing the Grandchild
    private GameObject grandChildPing; // Reference to the Grandchild named "Ping"

    public List<Transform> CetegoryObjects;
    public List<CategoriesDomeInfo> CategoriesDomeInfos;
    public GameObject NameItemPrefab;

    public Image selectedWorldBannerImage;
    public TMP_Text selectedWorldNameText;
    public TMP_Text selectedWorldCreatorName;
    public GameObject WorldIconFrame_BorderObj;
    public GameObject goBtn; // Changed to Button for interactable control

    public XANASummitDataContainer dataManager;
    public TMPro.TMP_Text totalVisitCount;

    private Dictionary<string, int> nameToIndexDictionary;
    [SerializeField]
    private GameObject CategoryPrefab;

    private int selectedDomeId = -1; // Track the currently selected dome ID

    private void OnEnable()
    {
        totalVisitCount.text = "" + ConstantsHolder.visitorCount;
    }

    void Start()
    {
        StartCoroutine(WaitForDataManager());
    }

    IEnumerator WaitForDataManager()
    {
        // Wait until dataManager is set and data is loaded
        while (dataManager == null || dataManager.summitData.domes == null || dataManager.summitData.domes.Count == 0)
        {
            yield return null;
        }

        IntCategories();
    }

    void IntCategories()
    {
        var uniqueCategories = new HashSet<string>();
        var domes = dataManager.summitData.domes;

        foreach (var dome in domes)
        {
            if (string.IsNullOrWhiteSpace(dome.domeCategory))
            {
                continue;
            }

            if (uniqueCategories.Add(dome.domeCategory))
            {
                GameObject newCategory = Instantiate(CategoryPrefab, scrollRect.content);
                newCategory.name = dome.domeCategory;

                // Assuming the TMP_Text component is named "minimapCategory" in the prefab
                MinimapCategory minimapCatrgory = newCategory.GetComponent<MinimapCategory>();
                if (minimapCatrgory != null)
                {
                    minimapCatrgory.NameTMPText.text = TextLocalization.GetLocaliseTextByKey(dome.domeCategory);
                    minimapCatrgory.CategoryBtn.onClick.AddListener(() => ExpandChild(minimapCatrgory.transform.GetSiblingIndex()));
                }

                CetegoryObjects.Add(newCategory.transform);

                // Add to CategoriesDomeInfos list
                CategoriesDomeInfos.Add(new CategoriesDomeInfo
                {
                    categoryName = dome.domeCategory,
                    categoryIndex = CategoriesDomeInfos.Count,
                    MyDomes = new List<int>(),
                    DomeNamePrefix = new List<string>()
                });
            }
        }

        nameToIndexDictionary = new Dictionary<string, int>();
        for (int i = 0; i < CategoriesDomeInfos.Count; i++)
        {
            nameToIndexDictionary[CategoriesDomeInfos[i].categoryName] = i;
        }

        for (int i = 0; i < MapHighlightObjs.Count; i++)
        {
            int index = i;

            if (index > 166) // 3 Domes are skipped and used for Internal
                index += 3;

            if (index >= 173)
                index += 3;

            MapHighlightObjs[i].GetComponent<Button>().onClick.AddListener(() => ItemClicked_Icon(index));
        }

        InitializeSubBtns();
    }

    void InitializeSubBtns()
    {
        var domesDictionary = new Dictionary<int, string>();
        foreach (var dome in dataManager.summitData.domes)
        {
            if (dome.world.Equals("BreakingDownGame"))
                continue;
            if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
            {
                domesDictionary[dome.id] = dome.jpWorldName;
            }
            else
            {
                domesDictionary[dome.id] = dome.name;
            }

            AddDataInCategoryFromServer(dome.domeCategory, dome.id);
        }

        for (int i = 0; i < CategoriesDomeInfos.Count; i++)
        {
            if (i >= CetegoryObjects.Count)
            {
                Debug.LogError("Mismatch between CategoriesDomeInfos and CetegoryObjects lists.");
                continue;
            }

            Transform parentObj = CetegoryObjects[i];
            for (int j = 0; j < CategoriesDomeInfos[i].MyDomes.Count; j++)
            {
                int domeIdToCheck = CategoriesDomeInfos[i].MyDomes[j];

                String domePrefix = "";
                if (domesDictionary.TryGetValue(domeIdToCheck, out string domeName))
                    domePrefix = domeName;

                if (string.IsNullOrWhiteSpace(domePrefix))
                    continue;

                GameObject newObj = Instantiate(NameItemPrefab, parentObj);
                newObj.GetComponent<MapItemName>().manager = this;
                newObj.GetComponent<MapItemName>().SetItemName(domePrefix, domeIdToCheck, false);
            }

            if (parentObj.childCount <= 1)
            {
                parentObj.gameObject.SetActive(false);
                Debug.Log("Disabling Category due to No Element: " + parentObj.name);
            }
        }
    }

    void AddDataInCategoryFromServer(string _cetegory, int _id)
    {
        if (nameToIndexDictionary.TryGetValue(_cetegory, out int index))
        {
            if (CategoriesDomeInfos[index].MyDomes == null)
                CategoriesDomeInfos[index].MyDomes = new List<int>();

            if (!CategoriesDomeInfos[index].MyDomes.Contains(_id))
                CategoriesDomeInfos[index].MyDomes.Add(_id);
        }
        else
        {
            Debug.Log("Category Not Found: " + _cetegory);
        }
    }

    public void ItemClicked_Icon(int ind)
    {
        Debug.Log("Icon Clicked: " + ind);

        int arratInd = ind;

        if (ind == 176)// This is Penpenz
        {
            arratInd = 170;
            ind = 173;
        }
        else if (ind == 177) // This is Ninga Dao
        {
            arratInd = 171;
            ind -= 1;
        }
        else if (ind == 178)// This is SMBC
        {
            arratInd = 172;
            ind -= 1;
        }
        else if (ind > 166)// 3 Index are skipped and used for Internal
        {
            arratInd -= 3;
            Debug.Log("Modify Item Clicked: " + arratInd);
        }

        NextStep(ind, arratInd);
    } // This is the function that is called when the Map Icon button is clicked
    public void ItemClicked(int ind) // This is the function that is called when the Name button is clicked
    {
        Debug.Log("Name 2 Clicked: " + ind);

        int arratInd = ind;

        if (ind == 176) // This is ninga DAO 
        {
            arratInd = 171;
        }
        else if (ind == 177) // This is SMBC 
        {
            arratInd = 172;
        }
        else if (ind > 166) // 3 Index are skipped and used for Internal
        {
            arratInd -= 3;
            Debug.Log("Modify Item Clicked: " + arratInd);
        }

        NextStep(ind, arratInd);
    }
    void NextStep(int ind, int arratInd)
    {
        grandChildPing = MapHighlightObjs[arratInd];
        string areaName = grandChildPing.name;

        // Disable the goBtn initially
        goBtn.GetComponent<Button>().interactable = false;

        StartCoroutine(MoveChildToCenterOfMainScreen());
        EnableSelectedImage(arratInd);

        // Get the Thumbnail URL
        var dome = dataManager.summitData.domes.FirstOrDefault(d => d.id == (ind + 1));
        if (dome != null)
        {
            selectedDomeId = dome.id; // Track the selected dome ID

            if (!string.IsNullOrEmpty(dome.domes_media_v2.thumbnail))
            {
                string ThumbnailUrl = dome.domes_media_v2.thumbnail + "?width=" + ConstantsHolder.DomeImageCompression;
                StartCoroutine(DownloadTexture(ThumbnailUrl));
                selectedWorldNameText.text = dome.name;
                if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
                {
                    selectedWorldNameText.text = dome.jpWorldName;
                    selectedWorldCreatorName.text = "作成者 : " + dome.jpCreatorName;
                }
                else
                {
                    selectedWorldNameText.text = dome.name;
                    selectedWorldCreatorName.text = "Creator : " + dome.creatorName;
                }
                selectedWorldBannerImage.transform.parent.gameObject.SetActive(true);
                WorldIconFrame_BorderObj.SetActive(true);
            }
            else
            {
                selectedWorldBannerImage.transform.parent.gameObject.SetActive(false);
                WorldIconFrame_BorderObj.SetActive(false);
            }

            // Enable the goBtn if the dome is already spawned
            if (DomeMinimapDataHolder.OnInitDome != null)
            {
                goBtn.GetComponent<Button>().interactable = true;
            }
        }
        else
        {
            selectedWorldBannerImage.transform.parent.gameObject.SetActive(false);
            WorldIconFrame_BorderObj.SetActive(false);
        }

        goBtn.gameObject.SetActive(true);

        if (ind == 176)
            DomeMinimapDataHolder.OnSetDomeId?.Invoke(168, areaName);
        if (ind == 177)
            DomeMinimapDataHolder.OnSetDomeId?.Invoke(178, areaName);
        else
            DomeMinimapDataHolder.OnSetDomeId?.Invoke(ind + 1, areaName);
    }

    IEnumerator MoveChildToCenterOfMainScreen()
    {
        Vector3 startPosition = childObject.transform.position;
        Vector3 targetPosition = mainScreen.transform.position - grandChildPing.transform.position + startPosition;

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            childObject.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        childObject.transform.position = targetPosition;
    }

    void EnableSelectedImage(int _SelectedImage)
    {
        foreach (GameObject obj in MapHighlightObjs)
        {
            obj.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        }

        MapHighlightObjs[_SelectedImage].GetComponent<Image>().color = new Color(1, 1, 1, 1f);
    }

    IEnumerator DownloadTexture(string ThumbnailUrl)
    {
        if (AssetCache.Instance.HasFile(ThumbnailUrl))
        {
            AssetCache.Instance.LoadSpriteIntoImage(selectedWorldBannerImage, ThumbnailUrl, changeAspectRatio: true);
        }
        else
        {
            AssetCache.Instance.EnqueueOneResAndWait(ThumbnailUrl, ThumbnailUrl, (success) =>
            {
                if (success)
                {
                    AssetCache.Instance.LoadSpriteIntoImage(selectedWorldBannerImage, ThumbnailUrl, changeAspectRatio: true);
                }
            });
        }
        yield return null;
    }



    IEnumerator GetVisitorCount(string worldId)
    {
        string apiUrl = ConstantsGod.API_BASEURL + ConstantsGod.VISITORCOUNT + worldId;
        UnityWebRequest www = UnityWebRequest.Get(apiUrl);
        www.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
        www.SendWebRequest();
        while (!www.isDone)
            yield return null;
        string str = www.downloadHandler.text;
        VisitorInfo visitorInfo = JsonUtility.FromJson<VisitorInfo>(str);
        if (visitorInfo.success)
            totalVisitCount.text = "" + visitorInfo.data.total_visit;
        else
            totalVisitCount.text = "" + 100;
    }
    Sprite ConvertToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }


    int selectedCategoryIndex = -1;
    public void ExpandChild(int _Index)
    {
        selectedCategoryIndex = _Index;

        //Disable Other Categories
        for (int i = 0; i < CetegoryObjects.Count; i++)
        {
            if (i != _Index)
            {
                CategoriesController(i, false);
            }
        }

        // Get the status of the child
        bool childStatus = CetegoryObjects[_Index].GetChild(1).gameObject.activeSelf;
        // Reverse the Status
        childStatus = !childStatus;

        if (childStatus)
            selectedCategoryIndex = _Index;
        else
            selectedCategoryIndex = -1;

        CategoriesController(_Index, childStatus);

        Invoke(nameof(AddDelay), timeDelay);
    }
    void CategoriesController(int ind, bool status)
    {
        Vector3 localPos = CetegoryObjects[ind].localPosition;
        for (int i = 1; i < CetegoryObjects[ind].childCount; i++)
        {
            CetegoryObjects[ind].GetChild(i).gameObject.SetActive(status);
        }
    }
    void AddDelay()
    {
        float normalizedPos = 0.0f;
        switch (selectedCategoryIndex)
        {
            case 0:
            case 1:
                normalizedPos = 1.0f;
                break;

            case 2:
                normalizedPos = 0.93f;
                break;

            case 3:
            case 5:
            case 8:
                normalizedPos = 0.86f;
                break;

            case 4:
                normalizedPos = 0.76f;
                break;

            case 6:
                normalizedPos = 0.27f;
                break;

            case 7:
                normalizedPos = 0.57f;
                break;

            case 9:
                normalizedPos = 0.23f;
                break;

            case 10:
                normalizedPos = 0.78f;
                break;

            case 11:
                normalizedPos = 0.68f;
                break;

            case 12:
                normalizedPos = 0.0f;
                break;

            default:
                normalizedPos = 1f;
                break;
        }

        scrollRect.verticalNormalizedPosition = normalizedPos;
    }

    public float timeDelay = 0.02f;
}


[System.Serializable]
public class CategoriesDomeInfo
{
    public string categoryName;
    public int categoryIndex;
    public List<int> MyDomes;
    public List<string> DomeNamePrefix;
}
