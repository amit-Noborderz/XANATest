using RenderHeads.Media.AVProVideo;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using ZeelKheni.YoutubePlayer.Components;

public class DomeNFTApiHandler : MonoBehaviour
{
    public bool isCustomSizeNFT = false;
    public NFTContentHolder NFTContentHolder;
    public GameObject NFTPrefab;
    public Transform NFTParent;
    public GameObject DescriptionPanel;

    [Header("Image NFT Content")]
    public GameObject Image16x9;
    public GameObject Image9x16;
    public GameObject Image1x1;
    public GameObject Image4x3;

    [Header("Video NFT Content")]
    public GameObject VideoParent;
    public TextMeshProUGUI VideoName;
    public TextMeshProUGUI VideoDescription;
    public GameObject AWSPlayer;
    public GameObject YoutubePrerecordedPlayer;
    public GameObject YoutubeLiveVideoPlayer;
    public GameObject VideoLoader;
    public List<GameObject> placeholder = new List<GameObject>();

    public static DomeNFTApiHandler Instance;

    private void OnEnable()
    {
        Instance = this;
        BuilderEventManager.AfterPlayerInstantiated += LoadNFTOnPlayerReady;
    }

    private void OnDisable()
    {
        BuilderEventManager.AfterPlayerInstantiated -= LoadNFTOnPlayerReady;
    }

    async void LoadNFTOnPlayerReady()
    {
        if (ConstantsHolder.IsSubWorld && ConstantsHolder.isFromXANASummit)
        {
            await NFTContentHolder.GetNFTDATA(ConstantsGod.GETSUBDOMENFT, ConstantsHolder.SubDomeId.ToString());
        }
        else if (ConstantsHolder.isFromXANASummit)
            await NFTContentHolder.GetNFTDATA(ConstantsGod.GETDOMENFT, ConstantsHolder.domeId.ToString());
        if(isCustomSizeNFT)
            LoadNFTWithCustomSize();
        else
            LoadNFT();
    }

    void LoadNFT()
    {
        for (int i = 0; i < NFTContentHolder.DomeNFTDataHolder.getcontentbyDomeId.Count; i++)
        {
            if (i >= placeholder.Count)
                return;

            if (NFTContentHolder.DomeNFTDataHolder.getcontentbyDomeId[0].isAccessGiven == 0)
                return;

            GameObject temp = Instantiate(NFTPrefab, NFTParent);
            temp.transform.position = placeholder[i].transform.position;
            temp.transform.rotation = placeholder[i].transform.rotation;
            temp.GetComponent<VideoAndImageSetup>().compressionLevel = NFTContentHolder.DomeNFTDataHolder.width;
            temp.GetComponent<VideoAndImageSetup>().Init(NFTContentHolder.DomeNFTDataHolder.getcontentbyDomeId[i]);
        }
    }
    void LoadNFTWithCustomSize()
    {
        var domeNFTDataList = NFTContentHolder.DomeNFTDataHolder.getcontentbyDomeId;
        if (domeNFTDataList == null || domeNFTDataList.Count == 0)
            return;
        // Check access only once
        if (domeNFTDataList[0].isAccessGiven == 0)
            return;
        int count = Mathf.Min(domeNFTDataList.Count, placeholder.Count);
        for (int i = 0; i < count; i++)
        {
            Debug.Log($"Loading NFT {i} of {count}");
            var sizeContainer = placeholder[i]?.GetComponent<NFtSizeContaniner>();
            if (sizeContainer == null)
                continue;
            var nftData = domeNFTDataList[i];
            Transform referenceObj = GetReferenceTransform(sizeContainer, nftData.proportionType);
            if (referenceObj == null)
                continue;
            GameObject temp = Instantiate(NFTPrefab, NFTParent);
            temp.transform.localScale = Vector3.one; // Reset scale to avoid scaling issues
            temp.transform.position = Vector3.zero;
            var videoAndImageSetup = temp.GetComponent<VideoAndImageSetup>();
            if (videoAndImageSetup == null)
                continue;
            RectTransform canvasRect = GetCanvasRectTransform(videoAndImageSetup, nftData.proportionType);
            if (canvasRect == null)
                continue;
            var refRect = referenceObj.GetComponent<RectTransform>();
            if (refRect == null)
                continue;
            canvasRect.localPosition = refRect.localPosition;
            canvasRect.sizeDelta = refRect.sizeDelta;
            canvasRect.localRotation = refRect.localRotation;
            videoAndImageSetup.compressionLevel = NFTContentHolder.DomeNFTDataHolder.width;
            videoAndImageSetup.Init(nftData);
        }
    }
    // Helper to get the correct reference transform based on proportion type
    Transform GetReferenceTransform(NFtSizeContaniner container, string proportionType)
    {
        switch (proportionType)
        {
            case "16:9": return container._16x9?.transform;
            case "9:16": return container._9x16?.transform;
            case "1:1": return container._1x1?.transform;
            case "4:3": return container._4x3?.transform;
            default: return container._1x1?.transform;
        }
    }
    // Helper to get the correct canvas RectTransform based on proportion type
    RectTransform GetCanvasRectTransform(VideoAndImageSetup setup, string proportionType)
    {
        switch (proportionType)
        {
            case "16:9": return setup.Image16x9?.GetComponent<RectTransform>();
            case "9:16": return setup.Image9x16?.GetComponent<RectTransform>();
            case "1:1": return setup.Image1x1?.GetComponent<RectTransform>();
            case "4:3": return setup.Image4x3?.GetComponent<RectTransform>();
            default: return setup.Image1x1?.GetComponent<RectTransform>();
        }
    }
    public void OpenImageDecriptionPanel(Texture2D _Thumbnail, string _Name, string _Description, NFTRatio _nFTRatio)
    {
        DescriptionPanel.SetActive(true);
        switch (_nFTRatio)
        {
            case NFTRatio.None:
                break;
            case NFTRatio._16x9:
                Image16x9.SetActive(true);
                Image16x9.GetComponent<SetImageInfo>().Init(_Thumbnail, _Name, _Description);
                break;
            case NFTRatio._9x16:
                Image9x16.SetActive(true);
                Image9x16.GetComponent<SetImageInfo>().Init(_Thumbnail, _Name, _Description);
                break;
            case NFTRatio._1x1:
                Image1x1.SetActive(true);
                Image1x1.GetComponent<SetImageInfo>().Init(_Thumbnail, _Name, _Description);
                break;
            case NFTRatio._4x3:
                Image4x3.SetActive(true);
                Image4x3.GetComponent<SetImageInfo>().Init(_Thumbnail, _Name, _Description);
                break;
        }
    }

    public async void OpenVideoDescriptionPanel(bool IsYoutubeVideo, bool _IsLiveVideo, bool _IsPrerecorded, string _VideoUrl, string _Name, string _Description)
    {
        VideoName.text = _Name;
        VideoDescription.text = _Description;
        DescriptionPanel.SetActive(true);
        VideoParent.SetActive(true);

        if (IsYoutubeVideo)
        {
            YoutubeLiveVideoPlayer.SetActive(true);
            AWSPlayer.SetActive(false);
            if (_IsLiveVideo)
            {
                VideoLoader.SetActive(true);
                string streamableurl = await NFTContentHolder.GetLiveVideoUrl(_VideoUrl);
                YoutubeLiveVideoPlayer.GetComponent<MediaPlayer>().MediaPath.Path = streamableurl;
                YoutubeLiveVideoPlayer.GetComponent<MediaPlayer>().OpenMedia(YoutubeLiveVideoPlayer.GetComponent<MediaPlayer>().MediaPath, true);
                VideoLoader.SetActive(false);
            }
            else
            {
                VideoLoader.SetActive(true);
                string streamableurl = await NFTContentHolder.GetPrerecordedVideoUrl(_VideoUrl);
                YoutubeLiveVideoPlayer.GetComponent<MediaPlayer>().MediaPath.Path = streamableurl;
                YoutubeLiveVideoPlayer.GetComponent<MediaPlayer>().Loop = true;
                YoutubeLiveVideoPlayer.GetComponent<MediaPlayer>().OpenMedia(YoutubeLiveVideoPlayer.GetComponent<MediaPlayer>().MediaPath, true);
                VideoLoader.SetActive(false);
            }
        }
        else if (!IsYoutubeVideo && _IsPrerecorded)
        {
            YoutubeLiveVideoPlayer.SetActive(false);
            AWSPlayer.SetActive(true);
            AWSPlayer.GetComponent<VideoPlayer>().url = _VideoUrl;
            AWSPlayer.GetComponent<VideoPlayer>().isLooping = true;
            AWSPlayer.GetComponent<VideoPlayer>().Play();
        }
    }

    public void CloseDescriptionPanel()
    {
        DescriptionPanel.SetActive(false);
        VideoParent.SetActive(false);
        Image16x9.SetActive(false);
        Image1x1.SetActive(false);
        Image4x3.SetActive(false);
        Image9x16.SetActive(false);
        AWSPlayer.SetActive(false);
        YoutubeLiveVideoPlayer.SetActive(false);
        VideoLoader.SetActive(false);
    }
}
