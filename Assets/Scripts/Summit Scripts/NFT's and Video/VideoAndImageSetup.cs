using BD;
using SuperStar.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class VideoAndImageSetup : MonoBehaviour
{
    [HideInInspector]
    public int compressionLevel;

    public bool IsImageNFT;
    public bool IsVideoNFT;

    public RawImage Image16x9;
    public RawImage Image9x16;
    public RawImage Image1x1;
    public RawImage Image4x3;

    NFTRatio NFTRatio;
    Texture2D ThumbnailTexture;
    string ImageName;
    string ImageDescription;

    string ImageNameJP;
    string ImageDescriptionJP;

    string VideoUrl;
    bool IsLiveVideo;
    bool IsPrerecorded;
    bool IsYoutubeVideo;
    
    public void Init(NFTContentHolder.DomeImageVideoData domeImageVideoData)
    {
        AddListenerOnButton();
        CheckForNFTType(domeImageVideoData);
        SetupNameAndDesription(domeImageVideoData);
        SetUpThumbnailImage(domeImageVideoData);
        SetupVideo(domeImageVideoData);
    }

    void AddListenerOnButton()
    {
        Image16x9.GetComponent<Button>().onClick.AddListener(OnNFTTap);
        Image1x1.GetComponent<Button>().onClick.AddListener(OnNFTTap);
        Image4x3.GetComponent<Button>().onClick.AddListener(OnNFTTap);
        Image9x16.GetComponent<Button>().onClick.AddListener(OnNFTTap);
    }

    void CheckForNFTType(NFTContentHolder.DomeImageVideoData domeImageVideoData)
    {
        if(string.IsNullOrEmpty(domeImageVideoData.videoUrl))
        {
            IsVideoNFT=false;
            IsImageNFT = true;
        }
        else
        {
            IsVideoNFT = true;
            IsImageNFT = false;
        }
    }

    void SetupNameAndDesription(NFTContentHolder.DomeImageVideoData domeImageVideoData)
    {
        if (!string.IsNullOrEmpty(domeImageVideoData.name))
        {
            ImageName = domeImageVideoData.name;
        }

        if (!string.IsNullOrEmpty(domeImageVideoData.jpName))
        {
            ImageNameJP = domeImageVideoData.jpName;
        }

        if (!string.IsNullOrEmpty(domeImageVideoData.description))
        {
            ImageDescription = domeImageVideoData.description;
        }

        if (!string.IsNullOrEmpty(domeImageVideoData.jpDescription))
        {
            ImageDescriptionJP = domeImageVideoData.jpDescription;
        }

    }

    void SetUpThumbnailImage(NFTContentHolder.DomeImageVideoData domeImageVideoData)
    {
        if (!string.IsNullOrEmpty(domeImageVideoData.thumbnail))
        {
            string thumbnailUrl=domeImageVideoData.thumbnail+"?width=" + compressionLevel;
            if (domeImageVideoData.proportionType == "16:9")
            {
                NFTRatio = NFTRatio._16x9;
                DownloadThumbnail(thumbnailUrl, Image16x9);
            }
            else if (domeImageVideoData.proportionType == "9:16")
            {
                NFTRatio = NFTRatio._9x16;
                DownloadThumbnail(thumbnailUrl, Image9x16);
            }
            else if (domeImageVideoData.proportionType == "1:1")
            {
                NFTRatio = NFTRatio._1x1;
                DownloadThumbnail(thumbnailUrl, Image1x1);
            }
            else if (domeImageVideoData.proportionType == "4:3")
            {
                NFTRatio = NFTRatio._4x3;
                DownloadThumbnail(thumbnailUrl, Image4x3);
            }
        }

    }

    void DownloadThumbnail(string url, RawImage rawImage)
    {
        if (!string.IsNullOrEmpty(url))
        {
            if (AssetCache.Instance.HasFile(url))
            {
                ApplyThumbnailTexture(url, rawImage);
            }
            else
            {
                AssetCache.Instance.EnqueueOneResAndWait(url, url, (success) =>
                {
                    if (success)
                    {
                        ApplyThumbnailTexture(url, rawImage);
                    }
                });
            }
        }
    }

    void ApplyThumbnailTexture(string url, RawImage rawImage)
    {
        Texture2D texture = AssetCache.Instance.LoadImage(url);
        if (rawImage != null && texture != null)
        {
            rawImage.texture = texture;
            ThumbnailTexture = texture;
        }
        rawImage.gameObject.SetActive(true);
    }


    void SetupVideo(NFTContentHolder.DomeImageVideoData domeImageVideoData)
    {
        if (!string.IsNullOrEmpty(domeImageVideoData.videoUrl))
        {
            VideoUrl = domeImageVideoData.videoUrl;
        }

        if (!string.IsNullOrEmpty(domeImageVideoData.videoType))
        {
            if (domeImageVideoData.videoType == "Prerecorded")
            {
                IsLiveVideo = false;
                IsPrerecorded = true;
            }
            else
            {
                IsLiveVideo = true;
                IsPrerecorded = false;
            }
        }

        if (domeImageVideoData.isYoutubeUrl)
        {
            IsYoutubeVideo = true;
        }
        else
        {
            IsYoutubeVideo = false;
        }
    }

    //void SetUpVideo(string videoUrl, string videoType, bool IsYoutubeVideo)
    //{
    //    if (IsYoutubeVideo)
    //    {
    //        if (videoType == "Prerecorded")
    //        {

    //        }
    //        else
    //        {

    //        }
    //    }
    //    else
    //    {
    //        awsVideoPlayer.url = videoUrl;
    //        awsPlayer.SetActive(true);
    //    }
    //}

    void OnNFTTap()
    {
        string _Name, _Descirption;
        if(GameManager.currentLanguage=="en")
        {
            _Name = ImageName;
            _Descirption = ImageDescription;
        }
        else
        {
            _Name = ImageNameJP;
            _Descirption = ImageDescriptionJP;
        }

        if(IsImageNFT)
        {
            DomeNFTApiHandler.Instance.OpenImageDecriptionPanel(ThumbnailTexture,_Name,_Descirption,NFTRatio);
        }
        else
        {
            DomeNFTApiHandler.Instance.OpenVideoDescriptionPanel(IsYoutubeVideo,IsLiveVideo,IsPrerecorded,VideoUrl,_Name,_Descirption);
        }
    }
}
