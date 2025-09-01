using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RedirectionPopupHandler : MonoBehaviour
{
    public TMPro.TextMeshProUGUI RedirectionText;
    public GameObject RedirectionPopup;
    private string RedirectUrl;

    public DomeRedirection Redirections;
    private void OnEnable()
    {
        BuilderEventManager.OpenRedirectionPopup += OpenPopup;
        BuilderEventManager.AfterPlayerInstantiated += GetRedirectionUrl;
    }

    private void OnDisable()
    {
        BuilderEventManager.OpenRedirectionPopup -= OpenPopup;
        BuilderEventManager.AfterPlayerInstantiated -= GetRedirectionUrl;
    }

    void GetRedirectionUrl()
    {
        if (ConstantsHolder.IsSubWorld && ConstantsHolder.isFromXANASummit)
            GetRedirectionUrls(ConstantsGod.GETDOMEREDIRECTION, ConstantsHolder.SubDomeId);
        else if (ConstantsHolder.isFromXANASummit)
            GetRedirectionUrls(ConstantsGod.GETDOMEREDIRECTION, ConstantsHolder.domeId);
    }

    async void GetRedirectionUrls(string APIUrl,int DomeId)
    {
        string response =await GetAsyncRequest(ConstantsGod.API_BASEURL+ APIUrl + DomeId);
        Redirections =JsonUtility.FromJson<DomeRedirection>(response);
    }

    public void OnContinue()
    {
        Application.OpenURL(RedirectUrl);
        OnCancel();
    }

    void OpenPopup(int Index,string url, string msg)
    {
        if(ConstantsHolder.isFromXANASummit)
        {
            for(int i=0;i< Redirections.data.Length;i++)
            {
                if (Redirections.data[0].isAccessGiven == 0)
                    return;
                if (i == Index)
                {
                    RedirectUrl = Redirections.data[i].redirectionUrl;
                    RedirectionPopup.SetActive(true);
                    break;
                }
            }
        }
        else
        {
            if (string.IsNullOrEmpty(url))
                return;
            RedirectUrl = url;
            RedirectionPopup.SetActive(true);
        }

        if (!string.IsNullOrEmpty(msg))
            RedirectionText.text = msg;
    }

    public void OnCancel()
    {
        RedirectionPopup.SetActive(false);
    }

    async Task<string> GetAsyncRequest(string url)
    {
        string response = string.Empty;
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
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

    [System.Serializable]
    public class DomeRedirection
    {
        public RedirectionData[] data;
    }
    [System.Serializable]
    public class RedirectionData
    {
        public int id;
        public string redirectionUrl;
        public int isAccessGiven;
    }
}
