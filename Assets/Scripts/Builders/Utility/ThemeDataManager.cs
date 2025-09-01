using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ThemeDataManager
{
    internal Dictionary<(string, string), List<SDAThemeData>> AssetThemes;
    private Dictionary<string, Dictionary<string, ThemeMaterialData>> _assetThemeData;

    public ThemeDataManager()
    {
        AssetThemes = new();
        _assetThemeData = new Dictionary<string, Dictionary<string, ThemeMaterialData>>();
        _ = LoadAllThemesAsync();
    }

    async Task LoadAllThemesAsync()
    {
        await Task.WhenAll(LoadThemeDataAsync(), LoadAssetThemeDataAsync());
    }

    //Use builder bucket for theme data
    public const string GetMainnetBucket = "https://xana-builder.s3.ap-southeast-1.amazonaws.com/xanabuilder/";
    public const string GetTestnetBucket = "https://xana-builder-test.s3.ap-southeast-1.amazonaws.com/";
    public static string GetSDAAssetThemeData = "Json-Files/SDAAssetThemeData.json";
    public static string GetSDAThemeData = "Json-Files/SDAThemeData.json";

    async Task LoadThemeDataAsync()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get((APIBasepointManager.instance.IsXanaLive ? GetMainnetBucket : GetTestnetBucket) + GetSDAThemeData))
        {
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError(": Error: " + webRequest.error);
                return;
            }

            if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("ProtocolError " + webRequest.error);
                return;
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var data = JsonConvert.DeserializeObject<SDAThemeRequestData[]>(webRequest.downloadHandler.text);

                if (data != null)
                {
                    foreach (var requestData in data)
                    {
                        if (!AssetThemes.ContainsKey((requestData.subcategoryid, requestData.subcategoryname)))
                        {
                            AssetThemes.Add((requestData.subcategoryid, requestData.subcategoryname), new List<SDAThemeData>());
                        }

                        AssetThemes[(requestData.subcategoryid, requestData.subcategoryname)].Add(new SDAThemeData
                        {
                            themeid = requestData.themeid,
                            themename = requestData.themename,
                            themethumbnail = requestData.themethumbnail
                        });
                    }
                }
                else
                    Debug.LogError("JSON Deserialization failed or data is null for Theme Data.");
            }
        }
    }

    async Task LoadAssetThemeDataAsync()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get((APIBasepointManager.instance.IsXanaLive ? GetMainnetBucket : GetTestnetBucket) + GetSDAAssetThemeData))
        {
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError(": Error: " + webRequest.error);
                return;
            }

            if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("ProtocolError " + webRequest.error);
                return;
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var data = JsonConvert.DeserializeObject<AssetThemeRequestData[]>(webRequest.downloadHandler.text);

                if (data != null)
                {
                    foreach (var requestData in data)
                    {
                        if (!_assetThemeData.ContainsKey(requestData.assetid))
                        {
                            _assetThemeData.Add(requestData.assetid, new Dictionary<string, ThemeMaterialData>());
                        }

                        _assetThemeData[requestData.assetid].Add(requestData.themeid, new ThemeMaterialData
                        {
                            material = requestData.material,
                            diffuse = requestData.diffuse,
                            metallic = requestData.metallic,
                            normal = requestData.normal,
                            height = requestData.height,
                            emission = requestData.emission,
                            occlusion = requestData.occlusion
                        });
                    }
                }
                else
                    Debug.LogError("JSON Deserialization failed or data is null for Asset Theme Data.");
            }
        }
    }

    internal ThemeMaterialData GetThemeMaterialData(string assetId, string themeId)
    {
        if (_assetThemeData.ContainsKey(assetId) && _assetThemeData[assetId].ContainsKey(themeId))
        {
            return _assetThemeData[assetId][themeId];
        }

        return null;
    }

    internal List<string> GetAssetThemesData(IEnumerable<string> assetIds)
    {
        var themeIds = new HashSet<string>();

        foreach (var id in assetIds)
        {
            if (_assetThemeData.TryGetValue(id, out var themes))
            {
                foreach (var themeId in themes.Keys)
                {
                    themeIds.Add(themeId);
                }
            }
        }

        return themeIds.ToList();
    }
}

[Serializable]
public class SDAThemeRequestData
{
    public string themeid;
    public string themename;
    public string subcategoryid;
    public string subcategoryname;
    public string themethumbnail;
}

[Serializable]
public class SDAThemeData
{
    public string themeid;
    public string themename;
    public string themethumbnail;
}


[Serializable]
public class AssetThemeRequestData
{
    public string assetid;
    public string themeid;
    public string material;
    public string diffuse;
    public string metallic;
    public string normal;
    public string height;
    public string emission;
    public string occlusion;
}

[Serializable]
public class ThemeMaterialData
{
    public string material;
    public string diffuse;
    public string metallic;
    public string normal;
    public string height;
    public string emission;
    public string occlusion;
}