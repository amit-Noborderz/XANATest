using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperStar.Helpers;
using Toyota;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SummitDomeImageHandler : MonoBehaviour
{
    public XANASummitDataContainer XANASummitDataContainer;
    public TMPro.TMP_FontAsset DometextFont;
    public Material DomeTextMaterial;
    public NFT_Holder_Manager CommonScreen;
    public DomeTapVideoManager DomeTapVideoManager;

    public static Action<int> ShowNftData;
    public static Action VideoIsClosed;
    public static Action CloseLoaderObject;

    private Dictionary<string, Texture> _textureCache = new Dictionary<string, Texture>();

    AdvancedYoutubePlayer _youtubePlayer;
    GameObject _portraitLoader;
    GameObject _landscapeLoader;


    void OnEnable()
    {
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated += ApplyDomeShader;
        VideoIsClosed += ReleaseMemoryForVideo;
        ShowNftData += SetInfo;
        CloseLoaderObject += CloseLoader;
    }

    private void OnDisable()
    {
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated -= ApplyDomeShader;
        VideoIsClosed -= ReleaseMemoryForVideo;
        ShowNftData -= SetInfo;
        CloseLoaderObject -= CloseLoader;
    }

    void ReleaseMemoryForVideo()
    {
        Resources.UnloadUnusedAssets();
    }

    void ApplyDomeShader()
    {
        for (int i = 0; i < XanaWorldDownloader.AllDomes.Count; i++)
        {
            SummitDomeShaderApply SummitDomeShaderApplyRef = XanaWorldDownloader.AllDomes[i].GetComponent<SummitDomeShaderApply>();
            string[] DomeData = XANASummitDataContainer.GetDomeImage(SummitDomeShaderApplyRef.DomeId);
            SummitDomeShaderApplyRef.ImageUrl = DomeData[0];
            SummitDomeShaderApplyRef.LogoUrl = DomeData[2];
            if (!string.IsNullOrEmpty(DomeData[1]) && string.IsNullOrEmpty(DomeData[2]))
            {
                //TMPro.TextMeshPro DomeText1 = SummitDomeShaderApplyRef.DomeText.AddComponent<TMPro.TextMeshPro>();
                // Apply to Inner Dome Text
                if (SummitDomeShaderApplyRef.InnerDomeText != null)
                {
                    TMPro.TextMeshPro domeTextInner = SummitDomeShaderApplyRef.InnerDomeText.GetComponent<TMPro.TextMeshPro>()
                        ?? SummitDomeShaderApplyRef.InnerDomeText.AddComponent<TMPro.TextMeshPro>();


                    domeTextInner.font = DometextFont;
                    domeTextInner.fontMaterial = DomeTextMaterial;
                    domeTextInner.enableAutoSizing = true;
                    domeTextInner.fontSizeMin = 3;
                    domeTextInner.fontSizeMax = 5.5f;
                    //domeTextInner.fontSize = 6f;
                    domeTextInner.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                    domeTextInner.alignment = TMPro.TextAlignmentOptions.Center;
                    domeTextInner.text = DomeData[1];
                }

                // Apply to Outer Dome Text
                if (SummitDomeShaderApplyRef.OuterDomeText != null)
                {
                    TMPro.TextMeshPro domeTextOuter = SummitDomeShaderApplyRef.OuterDomeText.GetComponent<TMPro.TextMeshPro>()
                        ?? SummitDomeShaderApplyRef.OuterDomeText.AddComponent<TMPro.TextMeshPro>();

                    domeTextOuter.font = DometextFont;
                    domeTextOuter.fontMaterial = DomeTextMaterial;
                    //domeTextOuter.fontSize = 4.5f;
                    domeTextOuter.enableAutoSizing = true;
                    domeTextOuter.fontSizeMin = 3;
                    domeTextOuter.fontSizeMax = 4.5f;
                    domeTextOuter.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                    domeTextOuter.alignment = TMPro.TextAlignmentOptions.Center;
                    domeTextOuter.text = DomeData[1];
                }
            }
            SummitDomeShaderApplyRef.Init();

        }
    }
    public void Enable_PDF_Panel()
    {
        if (!ScreenOrientationManager._instance.isPotrait)
            CommonScreen.pdfPanel_L.SetActive(true);
        else
            CommonScreen.pdfPanel_P.SetActive(true);

        ReferencesForGamePlay.instance.eventSystemObj.SetActive(false);
        PlayerCameraController.instance.isReturn = true;
    }
    public void EnableControlls()
    {

        if (GamePlayUIHandler.inst.gameObject.activeInHierarchy)
        {
            GamePlayUIHandler.inst.gamePlayUIParent.SetActive(true);
        }

        ReferencesForGamePlay.instance.eventSystemObj.SetActive(true);
        PlayerCameraController.instance.isReturn = false;
    }

    private PMY_Ratio DetermineRatio(string proportionType, string mediaType)
    {
        switch (proportionType)
        {
            case "1:1":
                return mediaType == "Video" || mediaType == "Live" ? PMY_Ratio.OneXOneWithoutDes : PMY_Ratio.OneXOneWithDes;
            case "16:9":
                return mediaType == "Video" || mediaType == "Live" ? PMY_Ratio.SixteenXNineWithoutDes : PMY_Ratio.SixteenXNineWithDes;
            case "9:16":
                return mediaType == "Video" || mediaType == "Live" ? PMY_Ratio.NineXSixteenWithoutDes : PMY_Ratio.NineXSixteenWithDes;
            case "4:3":
                return mediaType == "Video" || mediaType == "Live" ? PMY_Ratio.FourXThreeWithoutDes : PMY_Ratio.FourXThreeWithDes;
            default:
                return mediaType == "Video" || mediaType == "Live" ? PMY_Ratio.OneXOneWithoutDes : PMY_Ratio.OneXOneWithDes;
        }
    }

    public async void SetInfo(int domeID)
    {
        var domedata = XANASummitDataContainer.GetDomeData(domeID);
        //string compersionPrfex;

        if (domedata == null)
        {
            return;
        }
        else if (domedata.domes_media_v2.mediajson == null)
            return;

        PMY_Ratio PMY_Ratio = DetermineRatio(domedata.domes_media_v2.mediajson[0].proportionType, domedata.domes_media_v2.mediajson[0].mediaType);

        int ratioId = (int)PMY_Ratio;
        if (domedata.domes_media_v2.mediajson[0].mediaType == "PDF")
        {
            if (string.IsNullOrEmpty(domedata.domes_media_v2.mediajson[0].mediaUrl))
                return;

            CommonScreen.pdfViewer_L.FileURL = domedata.domes_media_v2.mediajson[0].mediaUrl;
            CommonScreen.pdfViewer_P.FileURL = domedata.domes_media_v2.mediajson[0].mediaUrl;
            Enable_PDF_Panel();
        }
        //else if (type == PMY_DataType.Quiz)
        //{
        //    quizPanel_L.GetComponent<PMY_QuizController>().SetQuizData(quizData);

        //    quizPanel_P.GetComponent<PMY_QuizController>().SetQuizData(quizData);
        //    EnableQuizPanel();
        //}

        else
        {
            //var images = await DownloadDomeTexture(domedata.domes_media_v2.thumbnail);
            // Setting Landscape Data
            if (domedata.domes_media_v2.mediajson[0].mediaType == "Image")
            {
                CommonScreen.ratioReferences[ratioId].l_image.gameObject.SetActive(true);
                CommonScreen.ratioReferences[ratioId].p_image.gameObject.SetActive(true);
            }
            // CommonScreen.ratioReferences[ratioId].p_videoPlayer.gameObject.SetActive(true);
            //CommonScreen.ratioReferences[ratioId].l_videoPlayer.gameObject.SetActive(true);
            if (ratioId < 4)
            {
                if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
                {
                    CommonScreen.ratioReferences[ratioId].l_Title.text = domedata.jpWorldName;
                    CommonScreen.ratioReferences[ratioId].l_Aurthur.text = domedata.jpCreatorName;
                    CommonScreen.ratioReferences[ratioId].l_Description.text = domedata.jpDescription;
                }
                else
                {
                    CommonScreen.ratioReferences[ratioId].l_Title.text = domedata.name;
                    CommonScreen.ratioReferences[ratioId].l_Aurthur.text = domedata.creatorName;
                    CommonScreen.ratioReferences[ratioId].l_Description.text = domedata.description;// + "\n" + "<link=" + url + "><u>" + url + "</u></link>";
                }
            }
            if (domedata.domes_media_v2.mediajson[0].mediaType == "Image")
            {
                string ImageUrl = domedata.domes_media_v2.mediajson[0].mediaUrl + "?width=" + ConstantsHolder.DomeImageCompression;
                DownloadDomeTexture(ImageUrl, CommonScreen.ratioReferences[ratioId].l_image);
                //var image = await DownloadDomeTexture(domedata.domes_media_v2.mediajson[0].mediaUrl);
                //CommonScreen.ratioReferences[ratioId].l_image.texture = image;
                CommonScreen.ratioReferences[ratioId].l_videoPlayer.gameObject.SetActive(false);
            }
            else
            {
                CommonScreen.ratioReferences[ratioId].l_image.gameObject.SetActive(false);
                CommonScreen.ratioReferences[ratioId].l_videoPlayer.url = domedata.domes_media_v2.mediajson[0].mediaUrl;
            }

            // Setting Potraite Data
            if (ratioId < 4)
            {
                if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
                {
                    CommonScreen.ratioReferences[ratioId].p_Title.text = domedata.jpWorldName; ;
                    CommonScreen.ratioReferences[ratioId].p_Aurthur.text = domedata.jpCreatorName;
                    CommonScreen.ratioReferences[ratioId].p_Description.text = domedata.jpDescription;
                }
                else
                {
                    CommonScreen.ratioReferences[ratioId].p_Title.text = domedata.name; ;
                    CommonScreen.ratioReferences[ratioId].p_Aurthur.text = domedata.creatorName;
                    CommonScreen.ratioReferences[ratioId].p_Description.text = domedata.description;// + "\n" + "<link=" + url + "><u>" + url + "</u></link>";
                }
            }
            //CommonScreen.ratioReferences[ratioId].p_image.texture = images;
            if (domedata.domes_media_v2.mediajson[0].mediaType == "Image")
            {
                string ImageUrl= domedata.domes_media_v2.mediajson[0].mediaUrl+"?width=" + ConstantsHolder.DomeImageCompression;
                DownloadDomeTexture(ImageUrl, CommonScreen.ratioReferences[ratioId].p_image);
                //CommonScreen.ratioReferences[ratioId].p_image.texture = image;
                CommonScreen.ratioReferences[ratioId].p_videoPlayer.gameObject.SetActive(false);
            }
            else
            {
                CommonScreen.ratioReferences[ratioId].p_image.gameObject.SetActive(false);
                CommonScreen.ratioReferences[ratioId].p_videoPlayer.url = domedata.domes_media_v2.mediajson[0].mediaUrl;
            }

            if (!ScreenOrientationManager._instance.isPotrait) // for Landscape
            {
                if (domedata.domes_media_v2.mediajson[0].mediaType == "Image")
                {
                    CommonScreen.LandscapeObj.SetActive(true);
                    CommonScreen.PotraiteObj.SetActive(false);
                    CommonScreen.ratioReferences[ratioId].l_obj.SetActive(true);
                    CommonScreen.ratioReferences[ratioId].p_obj.SetActive(false);
                }

                if (domedata.domes_media_v2.mediajson[0].mediaType == "Video")
                {
                    DomeTapVideoManager.PlayVideo(domedata.domes_media_v2.mediajson[0].isYoutubeUrl, domedata.domes_media_v2.mediajson[0].videoUrlType, domedata.domes_media_v2.mediajson[0].mediaUrl);

                    //_landscapeLoader = CommonScreen.ratioReferences[ratioId].l_Loader.gameObject;
                    //_landscapeLoader.SetActive(true);
                    //_portraitLoader = CommonScreen.ratioReferences[ratioId].p_Loader.gameObject;
                    //_portraitLoader.SetActive(false);
                    //_youtubePlayer = CommonScreen.ratioReferences[ratioId].l_obj.GetComponent<AdvancedYoutubePlayer>();
                    //_youtubePlayer.ResetVideoURL();
                    //if (domedata.domes_media_v2.mediajson[0].videoUrlType == "Live" && domedata.domes_media_v2.mediajson[0].isYoutubeUrl)
                    //{
                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.GetComponent<RawImage>().enabled = false;
                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.enabled = false;
                    //    CommonScreen.ratioReferences[ratioId].l_PrerecordedPlayer.SetActive(false);
                    //    CommonScreen.ratioReferences[ratioId].l_LivePlayer.SetActive(true);
                    //    _youtubePlayer.IsLive = true;
                    //    _youtubePlayer.VideoId = domedata.domes_media_v2.mediajson[0].mediaUrl;
                    //    _youtubePlayer.PlayVideo();
                    //}
                    //else if (domedata.domes_media_v2.mediajson[0].videoUrlType == "Prerecorded" && domedata.domes_media_v2.mediajson[0].isYoutubeUrl)
                    //{
                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.GetComponent<RawImage>().enabled = true;
                    //    CommonScreen.ratioReferences[ratioId].l_PrerecordedPlayer.SetActive(true);
                    //    CommonScreen.ratioReferences[ratioId].l_LivePlayer.SetActive(false);
                    //    _youtubePlayer.IsLive = false;
                    //    _youtubePlayer.VideoId = _youtubePlayer.ExtractVideoIdFromUrl(domedata.domes_media_v2.mediajson[0].mediaUrl);
                    //    _youtubePlayer.PlayVideo();
                    //}
                    //else if (!domedata.domes_media_v2.mediajson[0].isYoutubeUrl)
                    //{
                    //    if (CommonScreen.ratioReferences[ratioId].l_PrerecordedPlayer)
                    //        CommonScreen.ratioReferences[ratioId].l_PrerecordedPlayer.SetActive(false);

                    //    if (CommonScreen.ratioReferences[ratioId].l_LivePlayer)
                    //        CommonScreen.ratioReferences[ratioId].l_LivePlayer.SetActive(false);

                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.GetComponent<RawImage>().enabled = true;
                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.enabled = true;
                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.url = domedata.domes_media_v2.mediajson[0].mediaUrl;
                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.prepareCompleted += (vid) => { NFT_Holder_Manager.instance.videoReady(); };
                    //    CommonScreen.ratioReferences[ratioId].l_videoPlayer.Play();
                    //}

                    //    OnVideoEnlargeAction?.Invoke();
                }
            }
            else // for Potraite
            {

                if (domedata.domes_media_v2.mediajson[0].mediaType == "Image")
                {
                    CommonScreen.LandscapeObj.SetActive(false);
                    CommonScreen.PotraiteObj.SetActive(true);
                    CommonScreen.ratioReferences[ratioId].l_obj.SetActive(false);
                    CommonScreen.ratioReferences[ratioId].p_obj.SetActive(true);
                }

                if (domedata.domes_media_v2.mediajson[0].mediaType == "Video")
                {
                    DomeTapVideoManager.PlayVideo(domedata.domes_media_v2.mediajson[0].isYoutubeUrl, domedata.domes_media_v2.mediajson[0].videoUrlType, domedata.domes_media_v2.mediajson[0].mediaUrl);
                    //_landscapeLoader = CommonScreen.ratioReferences[ratioId].l_Loader.gameObject;
                    //_landscapeLoader.SetActive(false);
                    //_portraitLoader = CommonScreen.ratioReferences[ratioId].p_Loader.gameObject;
                    //_portraitLoader.SetActive(true);

                    //if (domedata.domes_media_v2.mediajson[0].videoUrlType == "Live" && domedata.domes_media_v2.mediajson[0].isYoutubeUrl)
                    //{
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.GetComponent<RawImage>().enabled = false;
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.enabled = false;
                    //    CommonScreen.ratioReferences[ratioId].p_PrerecordedPlayer.SetActive(false);
                    //    CommonScreen.ratioReferences[ratioId].p_LivePlayer.SetActive(true);

                    //    CommonScreen.ratioReferences[ratioId].p_LivePlayer.GetComponent<StreamYoutubeVideo>().StreamYtVideo(domedata.domes_media_v2.mediajson[0].mediaUrl, true);
                    //}
                    //else if (domedata.domes_media_v2.mediajson[0].videoUrlType == "Prerecorded" && domedata.domes_media_v2.mediajson[0].isYoutubeUrl)
                    //{
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.GetComponent<RawImage>().enabled = true;
                    //    CommonScreen.ratioReferences[ratioId].p_PrerecordedPlayer.SetActive(true);
                    //    CommonScreen.ratioReferences[ratioId].p_LivePlayer.SetActive(false);

                    //    CommonScreen.ratioReferences[ratioId].p_PrerecordedPlayer.GetComponent<StreamYoutubeVideo>().StreamYtVideo(domedata.domes_media_v2.mediajson[0].mediaUrl, false);
                    //}
                    //else if (!domedata.domes_media_v2.mediajson[0].isYoutubeUrl)
                    //{
                    //    Debug.Log("Ratio " + ratioId + "  " + domedata.name);
                    //    CommonScreen.ratioReferences[ratioId].p_PrerecordedPlayer.SetActive(false);
                    //    CommonScreen.ratioReferences[ratioId].p_LivePlayer.SetActive(false);
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.GetComponent<RawImage>().enabled = true;
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.enabled = true;
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.prepareCompleted += (vid) => { NFT_Holder_Manager.instance.videoReady(); };
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.url = domedata.domes_media_v2.mediajson[0].mediaUrl;
                    //    CommonScreen.ratioReferences[ratioId].p_videoPlayer.Play();

                    //}

                    // OnVideoEnlargeAction.Invoke();
                }

            }
        }
        if (GamePlayUIHandler.inst.gameObject.activeInHierarchy)
        {
            GamePlayUIHandler.inst.gamePlayUIParent.SetActive(false);
        }

        void DownloadDomeTexture(string url,RawImage rawImage)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (AssetCache.Instance.HasFile(url))
                {
                    rawImage.texture = AssetCache.Instance.LoadImage(url);
                }
                else
                {
                    AssetCache.Instance.EnqueueOneResAndWait(url, url, (success) =>
                    {
                        if (success)
                        {
                            rawImage.texture = AssetCache.Instance.LoadImage(url);
                        }
                    });
                }
            }
        }


        //async Task<Texture> DownloadDomeTexture(string url)
        //{
        //    if (_textureCache.ContainsKey(url))
        //        return _textureCache[url];

        //    UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        //    await request.SendWebRequest();
        //    if ((request.result == UnityWebRequest.Result.ConnectionError) || (request.result == UnityWebRequest.Result.ProtocolError))
        //        Debug.Log(request.error);
        //    else
        //    {
        //        var texture = DownloadHandlerTexture.GetContent(request);
        //        _textureCache[url] = texture;
        //        return texture;
        //    }
        //    request.Dispose();
        //    return null;
        //}

    }

    void CloseLoader()
    {
        if (_landscapeLoader)
            _landscapeLoader.SetActive(false);
        if (_portraitLoader)
            _portraitLoader.SetActive(false);
    }
}
