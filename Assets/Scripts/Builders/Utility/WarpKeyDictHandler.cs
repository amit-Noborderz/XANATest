using Models;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class WarpKeyDictHandler
{
    private readonly Dictionary<string, WarpKeyData> warpKeyDictionary = new();

    #region UPDATERS

    public void UpdateWithStartpointData(string oldKey, string newKey, string runtimeID)
    {
        if (warpKeyDictionary.ContainsKey(oldKey))
        {
            warpKeyDictionary[oldKey].startPointId = string.Empty;
            warpKeyDictionary[oldKey].startPos = Constants.NanVector3;
        }
        if (string.Equals(newKey, Constants.SELECT_KEY)) return;

        if (!warpKeyDictionary.ContainsKey(newKey)) warpKeyDictionary.Add(newKey, new WarpKeyData());
        warpKeyDictionary[newKey].startPointId = runtimeID;
    }

    public void UpdateWithEndpointData(string oldKey, string newKey, string runtimeID)
    {
        if (warpKeyDictionary.ContainsKey(oldKey))
        {
            warpKeyDictionary[oldKey].endPointId = string.Empty;
            warpKeyDictionary[oldKey].endPos = Constants.NanVector3;
        }
        if (string.Equals(newKey, Constants.SELECT_KEY)) return;

        if (!warpKeyDictionary.ContainsKey(newKey)) warpKeyDictionary.Add(newKey, new WarpKeyData());
        warpKeyDictionary[newKey].endPointId = runtimeID;
    }

    public void RemoveStartEndPointData(string key, bool isStartPoint = true)
    {
        if (warpKeyDictionary.ContainsKey(key))
        {
            _ = isStartPoint
                ? warpKeyDictionary[key].startPointId = string.Empty
                : warpKeyDictionary[key].endPointId = string.Empty;
            _ = isStartPoint
                ? warpKeyDictionary[key].startPos = Constants.NanVector3
                : warpKeyDictionary[key].endPos = Constants.NanVector3;
        }
    }

    #endregion

    #region Validations

    public bool IsValidKey(string key, bool showToast = false)
    {
        if (!warpKeyDictionary.ContainsKey(key)) return false;

        if (warpKeyDictionary[key].startPointId.IsNotEmpty() &&
            warpKeyDictionary[key].endPointId.IsNotEmpty()) return true;

        if (warpKeyDictionary[key].startPointId.IsNullOrEmpty() && warpKeyDictionary[key].endPointId.IsNullOrEmpty())
        {
            // if (showToast) ToastManager.Show(Constants.WarpHasMissingKey);
            return false;
        }

        if (warpKeyDictionary[key].endPointId.IsNullOrEmpty())
        {
            // if (showToast) ToastManager.Show(Constants.MissingEndKey);
            return false;
        }

        if (warpKeyDictionary[key].startPointId.IsNullOrEmpty())
        {
            // if (showToast) ToastManager.Show(Constants.MissingStartKey);
            return false;
        }

        return true;
    }

    public bool HasInvalidData(bool showToast = false)
    {
        foreach (var item in warpKeyDictionary)
        {
            if (string.IsNullOrEmpty(item.Key)) continue;
            if ((string.IsNullOrEmpty(item.Value.startPointId) && string.IsNullOrEmpty(item.Value.endPointId)) ||
                (!string.IsNullOrEmpty(item.Value.startPointId) &&
                 !string.IsNullOrEmpty(item.Value.endPointId))) continue;
            // if (showToast) ToastManager.Show(Constants.WarpHasMissingKey);
            return true;
        }

        return false;
    }

    #endregion

    #region Getters

    public List<string> GetListOfKeys() => new List<string>(warpKeyDictionary.Keys);

    public List<string> GetDropdownKeys(bool isStartPoint, string key = null)
    {
        List<string> keyList = new List<string>();
        keyList.Add(Constants.SELECT_KEY);
        if (key.IsNotEmpty() && !key.Equals(Constants.SELECT_KEY)) keyList.Add(key); 

        foreach (var item in warpKeyDictionary)
        {
            if (!string.IsNullOrEmpty(item.Key) && string.IsNullOrEmpty(isStartPoint ? item.Value.startPointId : item.Value.endPointId)) keyList.Add(item.Key);
        }

        return keyList;
    }
    
    public Vector3 GetPosition(string key, bool isStartPoint)
    {
        if (warpKeyDictionary.ContainsKey(key))
            return isStartPoint ? warpKeyDictionary[key].startPos : warpKeyDictionary[key].endPos;
        return Vector3.zero;
    }


    public List<string> GetKeyListAssociatedWithStartOrEndPoints(bool forStartPoint = true)
    {
        List<string> keyList = new List<string>();
        foreach (var item in warpKeyDictionary)
        {
            if (!string.IsNullOrEmpty(item.Key) &&
                !string.IsNullOrEmpty(forStartPoint ? item.Value.startPointId : item.Value.endPointId))
                keyList.Add(item.Key);
            //if (IsVectorUnInitialized(forStartPoint ? item.Value.startPos : item.Value.endPos))
        }

        return keyList;
    }

    #endregion

    #region Setters

    /// <summary>
    /// Returns false is key is already present otherwise returns true, additionally shows Toast warning for the same key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool TryAddNewKey(string key)
    {
        if (warpKeyDictionary.ContainsKey(key))
        {
            // ToastManager.Show(Constants.SameKeyWarning);
            return false;
        }
        warpKeyDictionary.Add(key, new WarpKeyData());
        return true;
    }

    public void AddExistingKey(WarpFunctionComponentData data, string id, Vector3 pos)
    {
        string key = data.isWarpPortalStart ? data.warpPortalStartKeyValue : data.warpPortalEndKeyValue;

        if (!warpKeyDictionary.ContainsKey(key)) warpKeyDictionary.Add(key, new WarpKeyData());
        
        if (data.isWarpPortalStart)
        {
            warpKeyDictionary[key].startPointId = id;
            warpKeyDictionary[key].startPos = pos;
        }
        else
        {
            warpKeyDictionary[key].endPointId = id;
            warpKeyDictionary[key].endPos = pos;
        }
    }

    public void SetKeyDropdownValues(List<string> keyList)
    {
        foreach (var item in keyList)
        {
            if (warpKeyDictionary.ContainsKey(item))
                continue;

            warpKeyDictionary.Add(item, new WarpKeyData());
        }
    }

    public void SetListOfSystemStartPoint(List<PortalSystemStartPoint> portalDataForStartPointLocationList)
    {
        foreach (var item in portalDataForStartPointLocationList)
        {
            if (warpKeyDictionary.ContainsKey(item.indexPortalStartKey) &&
                !string.IsNullOrEmpty(warpKeyDictionary[item.indexPortalStartKey].startPointId))
                warpKeyDictionary[item.indexPortalStartKey].startPos = item.portalStartLocation;
        }
    }

    public void SetListOfSystemEndPoint(List<PortalSystemEndPoint> portalDataForEndPointLocationList)
    {
        foreach (var item in portalDataForEndPointLocationList)
        {
            if (warpKeyDictionary.ContainsKey(item.indexPortalEndKey) &&
                !string.IsNullOrEmpty(warpKeyDictionary[item.indexPortalEndKey].startPointId))
                warpKeyDictionary[item.indexPortalEndKey].startPos = item.portalEndLocation;
        }
    }

    // no need of setters for portalStartKeyvaluesChosen and endkeychosen as it can be maintained and derived from the original dict

    #endregion

    #region Helpers

    private bool IsVectorUnInitialized(Vector3 vector) =>
        (float.IsNaN(vector.x) && float.IsNaN(vector.y) && float.IsNaN(vector.z));

    #endregion
}

public class WarpKeyData
{
    public string startPointId;
    public string endPointId;
    public Vector3 startPos = Constants.NanVector3;
    public Vector3 endPos = Constants.NanVector3;
}