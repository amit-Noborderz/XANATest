
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebSocketSharp;
using TMPro;
using UnityEngine.SceneManagement;

public class CustomLocalization : MonoBehaviour
{
    public string LocalizeURL, LocalizeDateStamp;
    private string _path;
    private Coroutine prevCoroutine;
    public bool _forceJapanese;
    public static bool forceJapanese;
    public static Dictionary<string, RecordsLanguage> localisationDict;
    [SerializeField]
    public bool isLocalizationOn;
    [SerializeField]
    public string currentLanguage;
    [SerializeField]
    public ForceLanguage forceLanguage;
    public static bool IsReady;
    public static CustomLocalization Instance;
    public TMP_FontAsset jpFont; // Medium font
    public TMP_FontAsset jpBoldFont;
    public TMP_FontAsset JPHeavyNormalFont;
    

    public TMP_FontAsset JPHeaderFont; 
    // public TMP_FontAsset JPSettingHeaderFont; 

    public TMP_FontAsset JPTitleFont;
    public TMP_FontAsset JPSemiBoldFont; 
    public TMP_FontAsset JPMediumFont;

    public TMP_FontAsset JPOrbitronBoldFont;
    public TMP_FontAsset JPBoldOutlineFont;
    public TMP_FontAsset JPHeavyOutlineFont;


    public Color JPHeaderColor;
    public Color EngHeaderColor;
    public Color EngSettingHeaderColor;
    public Color JPSettingHeaderColor;
    public TMP_FontAsset JPLangFont;
    public TMP_FontAsset JPPlayerNameXX;
    public TMP_FontAsset JPPowpupTitleBoldOutline;
    public TMP_FontAsset JPPowpupSmallBoldOutline;


    private static string filePath;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }
        else { Instance = this; }

        filePath = Path.Combine(Application.persistentDataPath, "languageSettings.json");

        DontDestroyOnLoad(this.gameObject);
        _forceJapanese = false;//ProjectSetting.Singleton.ForceJapanese();
        forceJapanese = _forceJapanese;
        string deckFolder = Application.persistentDataPath + "/Deck";

        if (!Directory.Exists(deckFolder))
        {
            Directory.CreateDirectory(deckFolder);
        }
        _path = deckFolder + "/Localization.dat";
        if (!File.Exists(_path))
        {
            File.Create(_path);
        }
        currentLanguage = GetLanguage();
        //currentLanguage = "en";
        Debug.Log("Current langugae ====> " + currentLanguage);
        if (isLocalizationOn)
            prevCoroutine = StartCoroutine(GetLocalizationDataFromSheet()); //load localizationdata
    }
    public string GetText(string originalText)
    {
        if(localisationDict == null)
        return originalText;

        if (localisationDict.ContainsKey(originalText))
        {
            if (forceLanguage == ForceLanguage.Japanese && !localisationDict[originalText].Japanese.IsNullOrEmpty())
                return localisationDict[originalText].Japanese;

            else if (forceLanguage == ForceLanguage.Chinese && !localisationDict[originalText].Chinese.IsNullOrEmpty())
                return localisationDict[originalText].Chinese;

            else if (forceLanguage == ForceLanguage.English && !localisationDict[originalText].English.IsNullOrEmpty())
                return localisationDict[originalText].English;
            else
            {
                //Debug.Log("Current langugae ====> " + currentLanguage);

                switch (currentLanguage)
                {
                    case "en":
                        return localisationDict[originalText].English;
                    case "ja":
                        return localisationDict[originalText].Japanese;
                    case "zh":
                        return localisationDict[originalText].Chinese;
                }
            }
        }

        return originalText;
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    currentLanguage = "ja";
        //    EventManager.OnInvokeCangeLangauge();
        //}
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    currentLanguage = "en";
        //    EventManager.OnInvokeCangeLangauge();
        //}
    }
    #region GetLanguage
    public static string GetLanguage()
    {
        //if (!PlayerPrefs.GetString("GameLanguage").IsNullOrEmpty())
        //    return PlayerPrefs.GetString("GameLanguage");

        if(LoadLanguage().IsNullOrEmpty() == false)
        {
            return LoadLanguage();
        }
        string newLanguage = Application.systemLanguage.ToString();

        if (newLanguage == "English")
        {
            return "en";
        }
        else if (newLanguage == "Japanese")
        {
            return "ja";
        }
        else if (newLanguage == "Chinese")
        {
            return "zh";
        }
        else
            return "en";
        //if (!PlayerPrefs.GetString("gameLanguage").IsNullOrEmpty())
        //    return PlayerPrefs.GetString("gameLanguage");

        //#if UNITY_EDITOR
        //        string newLanguage = Application.systemLanguage.ToString();

        //        if (newLanguage == "English")
        //        {
        //            return "en";
        //        }
        //        else if (newLanguage == "Japanese")
        //        {
        //            return "ja";
        //        }
        //        else if (newLanguage == "Chinese")
        //        {
        //            return "zh";
        //        }
        //        else
        //            return "";
        //#elif UNITY_ANDROID
        //        try
        //        {
        //            var locale = new AndroidJavaClass("java.util.Locale");
        //            var localeInst = locale.CallStatic<AndroidJavaObject>("getDefault");
        //            var name = localeInst.Call<string>("getLanguage");
        //            return name;
        //        }
        //        catch (System.Exception e)
        //        {
        //            return "Error";
        //        }
        //#elif UNITY_IOS || UNITY_EDITOR
        //     string newLanguage = Application.systemLanguage.ToString();

        //        if (newLanguage == "English")
        //        {
        //            return "en";
        //        }
        //        else if (newLanguage == "Japanese")
        //        {
        //            return "ja";
        //        }
        //         else if (newLanguage == "Chinese")
        //        {
        //            return "zh";
        //        }
        //        else
        //            return "";
        //#else
        //        return "en";// this is builder running code
        //#endif
    }
    #endregion
    IEnumerator CheckIfSheetUpdated()
    {
        var www = UnityWebRequest.Get(LocalizeDateStamp);
        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError)
        {
            Debug.LogError(www.error);
            StopAllCoroutines();
            StartCoroutine(CheckIfSheetUpdated());
            //Awake();
        }
        else
        {
            var json = www.downloadHandler.text;
            var dateTime = DateTime.Parse(json);

            if (DateTime.Parse(PlayerPrefs.GetString("DateTime")) < dateTime || !PlayerPrefs.HasKey("DateTime"))
            {
                PlayerPrefs.SetString("DateTime", json);
                //StartCoroutine(GetLocalizationDataFromSheet()); //devid temp comment
            }
            else
            {
                print("Not Updated USe previous sheet");
            }
        }
    }

    IEnumerator GetLocalizationDataFromSheet()
    {
        Debug.Log("Getting data");
        var www = UnityWebRequest.Get(LocalizeURL);
        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError)
        {
            Debug.LogError(www.error);
            IsReady = false;

            Coroutine current = StartCoroutine(GetLocalizationDataFromSheet()); // recall on error
            if (prevCoroutine != null)
            {
                StopCoroutine(prevCoroutine);
            }

            prevCoroutine = current;
        }
        else
        {
            var json = www.downloadHandler.text;

            if (!json.IsNullOrEmpty())
            {
                RecordsLanguage[] localisationSheet = CSVSerializer.Deserialize<RecordsLanguage>(json);
                localisationDict = localisationSheet.GroupBy(p => p.Keys, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase); //Remove Duplicate entries
                IsReady = true;
            }
            else
            {
                Debug.LogError("Json is empty");
            }
        }
    }

    #region Unused Functions
    void ReadDataFromFile()
    {
        //    StreamReader reader = new StreamReader(path);
        //    Debug.Log(reader.ReadToEnd());
        //      string json = reader.ReadToEnd();
        // reader.Close();
        //  Debug.Log(json);
        //        RecordsLanguage[] avc = CSVSerializer.Deserialize<RecordsLanguage>(json);
    }
    public void SaveFile()
    {
        string deckFolder = Application.persistentDataPath + "/Deck";

        if (!Directory.Exists(deckFolder))
        {
            Directory.CreateDirectory(deckFolder);
        }
        string destination = deckFolder + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);
        file.Close();
    }
    #endregion

    public void SaveLanguage(string language)
    {
        LanguageSettings settings = new LanguageSettings();
        settings.selectedLanguage = language;

        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved language: {language}");
    }

    public static string LoadLanguage()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            LanguageSettings settings = JsonUtility.FromJson<LanguageSettings>(json);
            Debug.Log($"Loaded language: {settings.selectedLanguage}");
            return settings.selectedLanguage;
        }
        else
        {
            Debug.Log("No language settings found, using default.");
            return "en"; // Default language
        }
    }
}

[System.Serializable]
public class LanguageSettings
{
    public string selectedLanguage;
}

public class RecordsLanguage
{
    public string Keys;
    public string English;
    public string Japanese;
    public string Korean;
    public string Chinese;
}
public enum ForceLanguage
{
    English,
    Japanese,
    Chinese,
    None
}