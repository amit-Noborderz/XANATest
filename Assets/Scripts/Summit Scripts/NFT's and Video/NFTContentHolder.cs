using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;

[CreateAssetMenu(menuName = "ScriptableObjects/NFTContent", fileName = "ScriptableObjects/NFTContentHolder")]
public class NFTContentHolder : ScriptableObject
{
    public DomeNFT DomeNFTDataHolder = new DomeNFT();

   
    public async Task GetNFTDATA(string APIUrl,string DomeId)
    {
        int xcounter = 0;
        CheckAgainOnError:
        using (UnityWebRequest request = UnityWebRequest.Get(ConstantsGod.API_BASEURL +APIUrl + DomeId))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                xcounter++;
                if (xcounter < 2)
                    goto CheckAgainOnError;

            }
            else
                DomeNFTDataHolder = JsonUtility.FromJson<DomeNFT>(request.downloadHandler.text);

            request.Dispose();
        }
    }

    public async Task<string> GetLiveVideoUrl(string LiveYoutubeUrl)
    {
        RequestUrl requestUrl = new RequestUrl();
        requestUrl.url = LiveYoutubeUrl;
        string json=JsonUtility.ToJson(requestUrl);
        using (UnityWebRequest request = UnityWebRequest.Post(ConstantsGod.LIVEVIDEOAPI, "POST"))
        {
            byte[] jsonToSend = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
               

            }
            else
            {
                LiveVideoResponse response = new LiveVideoResponse();
                response = JsonUtility.FromJson<LiveVideoResponse>(request.downloadHandler.text);
                return response.streamable_url;
            }


            
            request.Dispose();
        }
        return string.Empty;
    }


    public async Task<string> GetPrerecordedVideoUrl(string YoutubeUrl)
    {
        RequestUrl requestUrl = new RequestUrl();
        requestUrl.url = YoutubeUrl;
        string json = JsonUtility.ToJson(requestUrl);
        using (UnityWebRequest request = UnityWebRequest.Post(ConstantsGod.PRERECORDEDVIDEOAPI, "POST"))
        {
            byte[] jsonToSend = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {


            }
            else
            {
                PrerecordedVideoResponse response = new PrerecordedVideoResponse();
                response=JsonUtility.FromJson<PrerecordedVideoResponse>(request.downloadHandler.text);
                return response.video_url;
            }
            request.Dispose();
        }
        return string.Empty;
    }

    [System.Serializable]
    public class DomeImageVideoData
    {
        public int id;
        public int dome_id;
        public int index;
        public string name;
        public string jpName;
        public string description;
        public string jpDescription;
        public string thumbnail;
        public int type;
        public string videoType;
        public string videoUrl;
        public bool isYoutubeUrl;
        public string proportionType;
        public int groupId;
        public int isAccessGiven;
    }

    [System.Serializable]
    public class DomeNFT
    {
        public int width;
        public List<DomeImageVideoData> getcontentbyDomeId;
    }

    [System.Serializable]
    public class RequestUrl
    {
        public string url;
    }

    [System.Serializable]
    public class LiveVideoResponse
    {
        public string streamable_url;
    }

    [System.Serializable]
    public class PrerecordedVideoResponse
    {
        public string video_url;
    }

}
public enum NFTRatio
{
    None,
    _16x9,
    _9x16,
    _1x1,
    _4x3
}

